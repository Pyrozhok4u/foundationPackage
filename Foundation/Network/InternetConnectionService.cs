using System.Collections.Generic;
using System.Diagnostics;
using Foundation.ServicesResolver;
using Foundation.TimerUtils;
using UnityEngine;
using UnityEngine.Networking;

namespace Foundation.Network
{
	public enum ConnectionSpeed
	{
		Slow,
		Stable,
	}

	public class InternetConnectionService : BaseService
	{
		private string _address;
		private bool _isConnected = true;
		private int _interval;
		private int _pingIterator;
		private int _avgPingTime;
		private int _pingMaxIterations;
		private int _disconnectionCount;
		private int _disconnectionTreshold;
		private int _connectionCheckInterval;
		private int _connectionSpeedThreshold;
		private Timer _checkTimer;
		private Timer _checkInterval;
		private UnityWebRequest _request;
		private ConnectionSpeed _connectionSpeed = ConnectionSpeed.Stable;
		private readonly List<int> _pingTimes = new List<int>();
		private readonly Stopwatch _stopWatch = new Stopwatch();

		public delegate void OnConnectionStatusHandler(bool isConnected);
		public event OnConnectionStatusHandler OnConnectionStatusChange;

		public delegate void OnConnectionSpeedHandler(ConnectionSpeed connectionSpeed);
		public event OnConnectionSpeedHandler OnConnectionSpeedChange;

		protected override void Initialize()
		{

		}

		public void StartCheckInterval()
		{
			InternetConnectionConfig config = GetConfig<InternetConnectionConfig>();
			_address = config.DNS;
			_interval = config.Interval;
			_pingMaxIterations = config.PingMaxIterations;
			_disconnectionTreshold = config.DisconnectionTreshold;
			_connectionSpeedThreshold = config.ConnectionSpeedThreshold;
			_connectionCheckInterval = config.ConnectionCheckInterval;
			_checkInterval = TimerService.StartRecurringTimerMs(_connectionCheckInterval, CheckInternetConnection);
		}

		public override void Dispose()
		{
			_stopWatch?.Stop();
			_request?.Dispose();
			_checkTimer?.StopTimer();
			_checkInterval?.StopTimer();
		}

		private void CheckInternetConnection()
		{
			_request?.Dispose();
			_checkTimer?.StopTimer();
			_stopWatch.Restart();
			_request = new UnityWebRequest(_address);
			_request.SendWebRequest().completed += OnComplete;
			_checkTimer = TimerService.StartTimerMs(_interval, OnTimeout);
		}

		private void OnTimeout()
		{
			_request.Dispose();
			InternetIsNotAvailable();
		}

		private void OnComplete(AsyncOperation obj)
		{
			if (_request.isNetworkError)
			{
				InternetIsNotAvailable();
			}
			else
			{
				InternetAvailable();
			}
		}

		private void InternetIsNotAvailable()
		{
			_disconnectionCount++;
			if (_disconnectionCount <= _disconnectionTreshold || !_isConnected)
			{
				return;
			}
			_isConnected = false;
			OnConnectionStatusChange?.Invoke(_isConnected);
		}

		private void InternetAvailable()
		{
			_disconnectionCount = 0;
			int connectionSpeedTime = (int)_stopWatch.ElapsedMilliseconds;
			_pingTimes.Add(connectionSpeedTime);
			_pingIterator = _pingIterator >= _pingMaxIterations ? 0 : _pingIterator + 1;
			int totalPingTime = 0;
			foreach (int pingTime in _pingTimes)
			{
				totalPingTime += pingTime;
			}
			_avgPingTime = totalPingTime / _pingTimes.Count;
			ConnectionSpeed currentConnectionSpeed = _avgPingTime >= _connectionSpeedThreshold
				? ConnectionSpeed.Slow
				: ConnectionSpeed.Stable;
			if (currentConnectionSpeed != _connectionSpeed)
			{
				OnConnectionSpeedChange?.Invoke(currentConnectionSpeed);
				_connectionSpeed = currentConnectionSpeed;
			}
			if (!_isConnected)
			{
				_isConnected = true;
				OnConnectionStatusChange?.Invoke(_isConnected);
			}
		}
	}
}
