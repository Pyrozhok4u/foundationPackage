using System;
using Google.Protobuf;
using Foundation.Logger;
using BestHTTP.SocketIO;
using Foundation.TimerUtils;
using System.Collections.Generic;
using Foundation.ServicesResolver;
using BestHTTP.SocketIO.Transports;
using BestHTTP.SocketIO.JsonEncoders;
using Foundation.ConfigurationResolver;
using PlatformSupport.Collections.ObjectModel;
using UnityEngine;

namespace Foundation.Network
{
	enum ResponseErrorCodes
	{
		Internal = 666, // Retry!
		BadRequest = 777, // Don't retry!
		UnsupportedRoute = 888, // Don't retry!
		Processing = 999, // Retry!
	}

	public enum SocketConnectionStatus
	{
		Connected,
		Disconnected,
	}

	public class SocketService : BaseService, ISocketService
	{
		private string _uri;
		private int _retryCount;
		private int _retryDelay;
		private string _request = "request";
		private string _response = "response";
		private string _push = "push";
		private SocketConfig _config;
		private Timer _connectionTimer;
		private SocketManager _socketManager;
		private Dictionary<string, string> _query;
		private readonly List<ISocketRequest> _callbacks = new List<ISocketRequest>();
		private readonly List<ISocketResponse> _listeners = new List<ISocketResponse>();
		public event OnConnectionStatusChangeHandler OnConnectionStatusChange;

		private readonly SocketOptions _socketOptions = new SocketOptions()
		{
			ConnectWith = TransportTypes.WebSocket,
			Reconnection = false,
			ReconnectionAttempts = 0,
			ReconnectionDelay = TimeSpan.Zero,
			ReconnectionDelayMax = TimeSpan.Zero,
			RandomizationFactor = 0,
			AutoConnect = false,
			AdditionalQueryParams = new ObservableDictionary<string, string>()
		};

		protected override void Initialize()
		{
			_config = GetConfig<SocketConfig>();
			SocketManager.DefaultEncoder = new DefaultJSonEncoder();
		}

		public void Connect(string uri = null, Dictionary<string, string> query = null)
		{
			_uri = (uri ?? _config.Uri) + "/socket.io/";
			if (query != null)
			{
				_query = query;
				foreach (KeyValuePair<string, string> entry in _query)
				{
					_socketOptions.AdditionalQueryParams.Add(entry.Key, entry.Value);
				}
			}
			_socketManager = new SocketManager(new Uri(_uri), _socketOptions);
			_socketManager.Socket.AutoDecodePayload = false;
			_socketManager.Socket.On(_push, OnPush);
			_socketManager.Socket.On(_response, OnResponse);
			_socketManager.Socket.On(SocketIOEventTypes.Error, OnError);
			_socketManager.Socket.On(SocketIOEventTypes.Connect, OnConnect);
			_socketManager.Socket.On(SocketIOEventTypes.Disconnect, OnDisconnect);
			_connectionTimer = TimerService.StartTimerMs(_config.ConnectionTimeout, OnConnectionTimeout);
			_socketManager.Open();
		}

		public void Emit<T>(string eventName, IMessage message, Action<APIResponse<T>> callback = null)
			where T : IMessage<T>, new()
		{
			SocketRequest<T> request = new SocketRequest<T>(eventName, message, _config, ServiceResolver, callback);
			_callbacks.Add(request);
			request.Emit();
		}

		public void Register<T>(string eventName, Action<APIResponse<T>> callback)
			where T : IMessage<T>, new()
		{
			SocketResponse<T> socketResponse = new SocketResponse<T>(eventName, callback);
			_listeners.Add(socketResponse);
		}

		public void Unregister(string eventName)
		{
			for (int i = 0; i < _listeners.Count; i++)
			{
				if (eventName == _listeners[i].EventName)
				{
					_listeners.RemoveAt(i);
					i--;
				}
			}
		}

		public void Send(byte[] message)
		{
			_socketManager.Socket.Emit(_request, message);
		}

		public void RemoveCallback(ISocketRequest request)
		{
			_callbacks.Remove(request);
		}

		private void OnPush(Socket socket, Packet packet, object[] args)
		{
			CmdResponse cmdResponse;
			try
			{
				cmdResponse = CmdResponse.Parser.ParseFrom(packet.Attachments[0]);
			}
			catch (Exception e)
			{
				this.LogException(e);
				return;
			}
			object firstResponse = null;
			for (int i = 0; i < _listeners.Count; i++)
			{
				ISocketResponse response = _listeners[i];
				if (response == null)
				{
					continue;
				}
				if (response.EventName != cmdResponse.EventName)
				{
					continue;
				}
				if (firstResponse == null)
				{
					firstResponse = response;
					response.OnResponseReceived(cmdResponse.Payload);
				}
				else
				{
					response.OnResponseReceived(firstResponse);
				}
			}
		}

		private void OnConnectionTimeout()
		{
			_socketManager.Close();
			_retryCount += 1;
			_retryDelay = _retryDelay == 0 ? _config.ConnectionRetryDelay : _retryDelay * _config.ConnectionBackoff;
			if (_retryCount >= _config.ConnectionRetryCount)
			{
				this.LogError("Socket Reconnection Timeout!");
				OnConnectionStatusChange?.Invoke(SocketConnectionStatus.Disconnected);
				return;
			}
			this.LogWarning("Socket Reconnecting!");
			Connect(_uri, _query);
		}

		private void OnResponse(Socket socket, Packet packet, object[] args)
		{
			CmdResponse cmdResponse;
			try
			{
				cmdResponse = CmdResponse.Parser.ParseFrom(packet.Attachments[0]);
			}
			catch (Exception e)
			{
				this.LogException(e);
				return;
			}
			List<ISocketResponse> listeners = new List<ISocketResponse>();
			for (int i = 0; i < _listeners.Count; i++)
			{
				ISocketResponse response = _listeners[i];
				if (response == null)
				{
					continue;
				}
				if (response.EventName == cmdResponse.EventName)
				{
					listeners.Add(response);
				}
			}
			foreach (ISocketRequest request in _callbacks)
			{
				if (cmdResponse.MsgId == request.MsgId)
				{
					request.StopTimers();
					request.Listeners = listeners;
					switch (cmdResponse.ErrorCode)
					{
						case (int)ResponseErrorCodes.Processing:
						case (int)ResponseErrorCodes.Internal:
							request.Retry();
							break;
						case (int)ResponseErrorCodes.UnsupportedRoute:
							request.OnRequestError(cmdResponse.ErrorCode, "Socket Service Emit Unsupported Route.");
							break;
						case (int)ResponseErrorCodes.BadRequest:
							request.OnRequestError(cmdResponse.ErrorCode, "Socket Service Emit Bad Request.");
							break;
						default:
							request.OnRequestComplete(cmdResponse.Payload);
							_callbacks.Remove(request);
							break;
					}
					_callbacks.Remove(request);
					break;
				}
			}
		}

		private void OnDisconnect(Socket socket, Packet packet, object[] args)
		{
			this.LogError("Socket Has Been Disconnected!");
			if (!Application.isPlaying)
			{
				return;
			}
			_socketManager.Close();
			Connect(_uri);
		}

		private void OnConnect(Socket socket, Packet packet, object[] args)
		{
			this.Log("Socket Connected!");
			_retryCount = 0;
			_retryDelay = 0;
			_connectionTimer.StopTimer();
			OnConnectionStatusChange?.Invoke(SocketConnectionStatus.Connected);
		}

		private void OnError(Socket socket, Packet packet, params object[] args)
		{
			Error error = args[0] as Error;
			switch (error?.Code)
			{
				case SocketIOErrors.User:
					this.LogError("Exception in an event handler!");
					break;
				case SocketIOErrors.Internal:
					this.LogError("Internal error! Message: " + error.Message);
					break;
				default:
					this.LogError("Server error! Message: " + error?.Message);
					break;
			}
		}

		public override void Dispose()
		{
			_connectionTimer?.StopTimer();
			_socketManager?.Close();
			_socketManager = null;
			_callbacks.Clear();
			_listeners.Clear();
		}
	}
}
