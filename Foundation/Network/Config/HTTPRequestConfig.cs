using System;
using System.Collections.Generic;
using Foundation.ConfigurationResolver;
using Foundation.Logger;
using UnityEngine;

namespace Foundation.Network
{
	[Serializable]
	public class HTTPRequestConfig
	{
		public HTTPDataFormats HttpDataFormat = HTTPDataFormats.JSON;

		public Dictionary<string, string> Headers = new Dictionary<string, string>();

		[SerializeField] [Range(HTTPServiceConfig.MinRetryCount, HTTPServiceConfig.MaxRetryCount)]
		private int _retryCount = HTTPServiceConfig.MinRetryCount;
		public int RetryCount
		{
			get => _retryCount;
			set
			{
				this.Assert($"HTTPConfig Retry Count should be more or equal to {HTTPServiceConfig.MinRetryCount}", value >= HTTPServiceConfig.MinRetryCount);
				if (value >= HTTPServiceConfig.MinRetryCount) { _retryCount = value; }
			}
		}

		[SerializeField] [Range(HTTPServiceConfig.MinBackoff, HTTPServiceConfig.MaxBackoff)]
		private int _backoff = HTTPServiceConfig.MinBackoff;
		public int Backoff
		{
			get => _backoff;
			set
			{
				this.Assert($"Backoff must be equal or bigger than {HTTPServiceConfig.MinBackoff}", value >= HTTPServiceConfig.MinBackoff);
				if (value >= HTTPServiceConfig.MinBackoff) { _backoff = value; }
			}
		}

		[Header("Duration In Milliseconds")]
		[SerializeField] [Range(HTTPServiceConfig.MinRetryDelay, HTTPServiceConfig.MaxRetryDelay)]
		private int _retryDelay = HTTPServiceConfig.MinRetryDelay;
		public int RetryDelay
		{
			get => _retryDelay;
			set
			{
				this.Assert($"Retry Delay equal or bigger than {HTTPServiceConfig.MinRetryDelay} ms", value >= HTTPServiceConfig.MinRetryDelay);
				if (value >= HTTPServiceConfig.MinRetryDelay) { _retryDelay = value; }
			}
		}

		[SerializeField] [Range(HTTPServiceConfig.MinTimeout, HTTPServiceConfig.MaxTimeout)]
		private int _timeout = HTTPServiceConfig.MinTimeout;
		public int Timeout
		{
			get => _timeout;
			set
			{
				this.Assert($"HTTPConfig Timeout must be equal or bigger than {HTTPServiceConfig.MinTimeout} ms", value >= HTTPServiceConfig.MinTimeout);
				if (value >= HTTPServiceConfig.MinTimeout) { _timeout = value; }
			}
		}
	}
}
