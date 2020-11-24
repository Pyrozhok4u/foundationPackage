using BestHTTP.Logger;
using Foundation.ConfigurationResolver;

namespace Foundation.Network
{
	public class HTTPServiceConfig : BaseConfig
	{
		public Loglevels LogLevel = Loglevels.None;

		public const int MaxTimeout = 60000;
		public const int MinTimeout = 200;

		public const int MinRetryCount = 0;
		public const int MaxRetryCount = 10;

		public const int MinBackoff = 1;
		public const int MaxBackoff = 5;

		public const int MinRetryDelay = 20;
		public const int MaxRetryDelay = 10000;

		public HTTPRequestConfig RequestConfig;
	}
}
