using System;
using System.Collections.Generic;
using Foundation.Logger;
using Foundation.ServicesResolver;
using Foundation.TimerUtils;
using Google.Protobuf;

namespace Foundation.Network
{
	public interface ISocketRequest
	{
		string MsgId { get; }
		List<ISocketResponse> Listeners { set; }
		void Retry();
		void StopTimers();
		void OnRequestComplete(ByteString byteString);
		void OnRequestError(int errorCode, string reason);
	}

	public class SocketRequest<T> : ISocketRequest where T : IMessage, new()
	{
		public string MsgId => _msgId;
		public List<ISocketResponse> Listeners {private get; set; } = new List<ISocketResponse>();

		private int _retryCount;
		private int _retryDelay;
		private IMessage _message;
		private Timer _retryTimer;
		private Timer _timeoutTimer;

		private readonly byte[] _buffer;
		private readonly string _msgId;
		private readonly SocketConfig _config;
		private readonly CmdRequest _cmdRequest;
		private readonly ITimerService _timerService;
		private readonly ISocketService _socketService;
		private readonly Action<APIResponse<T>> _callback;

		public SocketRequest(string eventName, IMessage message, SocketConfig config, ServiceResolver serviceResolver, Action<APIResponse<T>> callback = null)
		{
			_msgId = Guid.NewGuid().ToString();
			_config = config;
			_callback = callback;
			_timerService = serviceResolver.Resolve<ITimerService>();
			_socketService = serviceResolver.Resolve<ISocketService>();
			_cmdRequest = new CmdRequest()
			{
				EventName = eventName,
				ReqMessageName = message.GetType().Name,
				ResMessageName = typeof(T).Name,
				MsgId = _msgId,
				Api = config.Api,
				Params = message.ToByteString(),
				AfterDelay = config.MetaAfterDelay,
				BeforeDelay = config.MetaBeforeDelay
			};
			_buffer = _cmdRequest.ToByteArray();
		}

		public void Emit()
		{
			_timeoutTimer = _timerService.StartTimerMs(_config.EmitTimeout, Retry);
			_socketService.Send(_buffer);
		}

		public void Retry()
		{
			_retryCount += 1;
			_retryDelay = _retryDelay != 0 ? _retryDelay * _config.EmitBackoff : _config.EmitRetryDelay;
			if (_retryCount >= _config.EmitRetryCount)
			{
				OnRequestError((int)APIErrorCodes.TimedOut, "Socket Emit Timeout!");
				_socketService.RemoveCallback(this);
				return;
			}
			this.LogWarning($"Socket Retry! {ToString()}");
			_retryTimer = _timerService.StartTimerMs(_retryDelay, Emit);
		}

		public void StopTimers()
		{
			_retryTimer?.StopTimer();
			_timeoutTimer?.StopTimer();
		}

		public void OnRequestComplete(ByteString byteString)
		{
			SocketResponse<T> socketResponse = new SocketResponse<T>(_callback);
			socketResponse.OnResponseReceived(byteString);
			foreach (ISocketResponse listener in Listeners)
			{
				SocketResponse<T> response = listener as SocketResponse<T>;
				response.OnResponseReceived(socketResponse.ApiResponse);
			}
		}

		public void OnRequestError(int errorCode, string reason)
		{
			APIResponse<T> apiResponse = new APIResponse<T>
			{
				ErrorCode = errorCode
			};
			apiResponse.SetFailure(reason);
			this.LogError("Socket Request Error! " + ToString());
			_callback?.Invoke(apiResponse);
			foreach (ISocketResponse listener in Listeners)
			{
				SocketResponse<T> response = listener as SocketResponse<T>;
				response.OnResponseReceived(apiResponse);
			}
		}

		public override string ToString()
		{
			return "Message: " +
					$"Name: {_cmdRequest.EventName}. " +
					$"ID: {_cmdRequest.MsgId}. " +
					$"API: {_cmdRequest.Api}. " +
					$"Retry Count: {_retryCount}. " +
					$"Retry Delay: {_retryDelay}";
		}
	}
}
