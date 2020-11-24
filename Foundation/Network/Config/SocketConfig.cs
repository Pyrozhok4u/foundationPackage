using Foundation.ConfigurationResolver;
using Foundation.Logger;
using UnityEngine;

namespace Foundation.Network
{
	public class SocketConfig : BaseConfig
	{
		private const int _minRetryCount = 0;
		private const int _minBackoff = 1;
		private const int _minTimeout = 20;
		private const int _minRetryDelay = 20;
		private const int _maxRetryCount = 50;
		private const int _maxBackoff = 5;
		private const int _maxRetryDelay = 10000;
		private const int _maxTimeout = 60000;

		[Header("Connection Settings")]
		public string Uri;
		public string Api;

		[SerializeField] [Range(_minRetryCount, _maxRetryCount)]
		private int _connectionRetryCount;
		public int ConnectionRetryCount
		{
			get => _connectionRetryCount;
			set
			{
				if (value < _minRetryCount)
				{
					this.LogError($"SocketConfig Connection Retry Count should be more or equal to {_minRetryCount}. Retry Count will set to {_minRetryCount}");
					_connectionRetryCount = _minRetryCount;
				}
				else
				{
					_connectionRetryCount = value;
				}
			}
		}

		[SerializeField] [Range(_minBackoff, _maxBackoff)]
		private int _connectionBackoff;
		public int ConnectionBackoff
		{
			get => _connectionBackoff;
			set
			{
				if (value < _minBackoff)
				{
					this.LogError($"SocketConfig Connection Backoff should be more or equal to {_minBackoff}. Backoff will set to {_minBackoff}");
					_connectionBackoff = _minBackoff;
				}
				else
				{
					_connectionBackoff = value;
				}
			}
		}

		[SerializeField] [Range(_minRetryDelay, _maxRetryDelay)]
		private int _connectionRetryDelay;
		public int ConnectionRetryDelay
		{
			get => _connectionRetryDelay;
			set
			{
				if (value < _minRetryDelay)
				{
					this.LogError($"SocketConfig Connection Retry Delay should be more or equal to {_minRetryDelay} ms. Retry Delay will set to {_minRetryDelay} ms");
					_connectionRetryDelay = _minRetryDelay;
				}
				else
				{
					_connectionRetryDelay = value;
				}
			}
		}

		[SerializeField] [Range(_minTimeout, _maxTimeout)]
		private int _connectionTimeout;
		public int ConnectionTimeout
		{
			get => _connectionTimeout;
			set
			{
				if (value < _minTimeout)
				{
					this.LogError($"SocketConfig Connection Timeout should be more or equal to {_minTimeout} ms. Timeout will set to {_minTimeout} ms");
					_connectionTimeout = _minTimeout;
				}
				else
				{
					_connectionTimeout = value;
				}
			}
		}

		[Header("Emit Settings")]
		[SerializeField] [Range(_minRetryCount, _maxRetryCount)]
		private int _emitRetryCount;
		public int EmitRetryCount
		{
			get => _emitRetryCount;
			set
			{
				if (value < _minRetryCount)
				{
					this.LogError($"SocketConfig Emir Retry Count should be more or equal to {_minRetryCount}. Retry Count will set to {_minRetryCount}");
					_emitRetryCount = _minRetryCount;
				}
				else
				{
					_emitRetryCount = value;
				}
			}
		}

		[SerializeField] [Range(_minBackoff, _maxBackoff)]
		private int _emitBackoff;
		public int EmitBackoff
		{
			get => _emitBackoff;
			set
			{
				if (value < _minBackoff)
				{
					this.LogError($"SocketConfig Emit Backoff should be more or equal to {_minBackoff}. Backoff will set to {_minBackoff}");
					_emitBackoff = _minBackoff;
				}
				else
				{
					_emitBackoff = value;
				}
			}
		}

		[SerializeField] [Range(_minRetryDelay, _maxRetryDelay)]
		private int _emitRetryDelay;
		public int EmitRetryDelay
		{
			get => _emitRetryDelay;
			set
			{
				if (value < _emitRetryDelay)
				{
					this.LogError($"SocketConfig Emit Retry Delay should be more or equal to {_minRetryDelay} ms. Retry Delay will set to {_minRetryDelay} ms");
					_emitRetryDelay = _minRetryDelay;
				}
				else
				{
					_emitRetryDelay = value;
				}
			}
		}

		[SerializeField] [Range(_minTimeout, _maxTimeout)]
		private int _emitTimeout;
		public int EmitTimeout
		{
			get => _emitTimeout;
			set
			{
				if (value < _minTimeout)
				{
					this.LogError($"SocketConfig Emit Timeout should be more or equal to {_minTimeout} ms. Timeout will set to {_minTimeout} ms");
					_emitTimeout = _minTimeout;
				}
				else
				{
					_emitTimeout = value;
				}
			}
		}

		[SerializeField] [Range(0, _maxTimeout)]
		private int _metaBeforeDelay;
		public int MetaBeforeDelay
		{
			get => _metaBeforeDelay;
			set => _metaAfterDelay = value;
		}

		[SerializeField] [Range(0, _maxTimeout)]
		private int _metaAfterDelay;
		public int MetaAfterDelay
		{
			get => _metaAfterDelay;
			set => _metaAfterDelay = value;
		}
	}
}
