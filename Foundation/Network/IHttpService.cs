using System;
using System.Collections.Generic;
using Foundation.ServicesResolver;
using Foundation.Utils.SerializerUtils;

namespace Foundation.Network
{
	public enum HTTPDataFormats { JSON, Bytes, RawData }

	public interface IHttpService : IService
	{
		HTTPRequestConfig GetBasicConfig();

		void Post<T, TU>(
			string uri,
			T message,
			Action<APIResponse<TU>> callback,
			HTTPRequestConfig requestConfig = null,
			ISerializer serializer = null,
			Action<APIProgress> progressCallback = null)
			where T : class where TU : class, new();

		void Get<T>(
			string uri,
			Action<APIResponse<T>> callback,
			Dictionary<string, string> queryData = null, 
			HTTPRequestConfig requestConfig = null,
			ISerializer serializer = null,
			Action<APIProgress> progressCallback = null
			) where T : class, new();

		void GetBytes(
			string uri,
			Action<APIResponse<byte[]>> callback,
			HTTPRequestConfig requestConfig = null,
			ISerializer serializer = null);
	}
}
