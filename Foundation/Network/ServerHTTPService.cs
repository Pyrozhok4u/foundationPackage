using System;
using Foundation.ServicesResolver;

namespace Foundation.Network
{
	public class ServerHTTPService : BaseService, IServerHTTPService
	{
		private const string JWTAuthorizationKey = "Authorization";
		private const string JWTAuthorizationValuePrefix = "Bearer ";

		public string JWT { get; set; }

		#region Post / Get

		public void Post<T, TU>(string uri, T message, Action<APIResponse<TU>> callback, HTTPRequestConfig requestConfig = null, Action<APIProgress> progressCallback = null) where T : class where TU : class, new()
		{
			requestConfig = requestConfig ?? GetBasicConfig();
			HTTPService.Post<T, TU>(uri, message, callback, requestConfig, progressCallback: progressCallback);
		}

		public void Get<T>(string uri, Action<APIResponse<T>> callback, HTTPRequestConfig requestConfig = null, Action<APIProgress> progressCallback = null) where T : class, new()
		{
			if (requestConfig == null)
			{
				requestConfig = GetBasicConfig();
			}

			requestConfig = requestConfig ?? GetBasicConfig();
			HTTPService.Get<T>(uri, callback, requestConfig: requestConfig, progressCallback: progressCallback);
		}

		#endregion

		#region Base service

		protected override void Initialize() { }

		public override void Dispose() { }

		#endregion

		public HTTPRequestConfig GetBasicConfig()
		{
			HTTPRequestConfig httpRequestConfig = HTTPService.GetBasicConfig();
			if (JWT != null)
			{
				httpRequestConfig.Headers.Add(JWTAuthorizationKey, JWTAuthorizationValuePrefix + JWT);
			}
			return httpRequestConfig;
		}

	}
}
