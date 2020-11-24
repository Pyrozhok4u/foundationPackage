using System;
using Foundation.ServicesResolver;

namespace Foundation.Network
{
	public interface IServerHTTPService : IService
	{
		string JWT { get; set; }

		HTTPRequestConfig GetBasicConfig();

		void Post<T, TU>(string uri, T message, Action<APIResponse<TU>> callback, HTTPRequestConfig requestConfig = null, Action<APIProgress> progressCallback = null)
			where T : class where TU : class, new();

		void Get<T>(string uri, Action<APIResponse<T>> callback, HTTPRequestConfig requestConfig = null, Action<APIProgress> progressCallback = null)
			where T : class, new();

	}
}
