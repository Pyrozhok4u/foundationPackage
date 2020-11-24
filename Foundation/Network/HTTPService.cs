using System;
using BestHTTP;
using System.Text;
using System.Collections.Generic;
using Foundation.ServicesResolver;
using Foundation.Utils.SerializerUtils;

namespace Foundation.Network
{
	public class HTTPService : BaseService, IHttpService
	{
		public string JWT { get; set; }

		private const string QuerySymbolEqual = "=";
		private const string QuerySymbolAdd = "&";
		private const string QuerySymbolQuestionMark = "?";

		private HTTPRequestConfig _defaultRequestConfig;
		private HTTPServiceConfig _serviceConfig;
		private readonly StringBuilder _getQueryString = new StringBuilder();
		private readonly ProtoSerializer _defaultSerializer = new ProtoSerializer();

		protected override void Initialize()
		{
			_serviceConfig = GetConfig<HTTPServiceConfig>();
			_defaultRequestConfig = _serviceConfig.RequestConfig;

			HTTPManager.Logger.Level = _serviceConfig.LogLevel;
			HTTPManager.RequestTimeout = TimeSpan.FromSeconds(HTTPServiceConfig.MaxTimeout + 1);
			HTTPManager.ConnectTimeout = TimeSpan.Zero;
		}

		public HTTPRequestConfig GetBasicConfig()
		{
			HTTPRequestConfig requestConfig = new HTTPRequestConfig();
			requestConfig.RetryCount = _defaultRequestConfig.RetryCount;
			requestConfig.RetryDelay = _defaultRequestConfig.RetryDelay;
			requestConfig.HttpDataFormat = _defaultRequestConfig.HttpDataFormat;
			requestConfig.Timeout = _defaultRequestConfig.Timeout;
			requestConfig.Backoff = _defaultRequestConfig.Backoff;

			requestConfig.Headers = new Dictionary<string, string>();
			foreach (KeyValuePair<string,string> pair in _defaultRequestConfig.Headers)
			{
				requestConfig.Headers.Add(pair.Key, pair.Value);
			}
			return requestConfig;
		}

		public void Post<T, TU>(string uri, T message, Action<APIResponse<TU>> callback,
			HTTPRequestConfig requestConfig = null, ISerializer serializer = null, Action<APIProgress> progressCallback = null)
			where T : class where TU : class, new()
		{
			serializer = serializer ?? _defaultSerializer;
			requestConfig = requestConfig ?? GetBasicConfig();
			APIRequest<T, TU> apiRequest = new APIRequest<T, TU>(uri, message, requestConfig, ServiceResolver, callback, serializer, progressCallback);
			apiRequest.Send();
		}

		public void Get<T>(string uri, Action<APIResponse<T>> callback, Dictionary<string, string> queryData = null,
			HTTPRequestConfig requestConfig = null, ISerializer serializer = null, Action<APIProgress> progressCallback = null)
			where T : class, new()
		{
			serializer = serializer ?? _defaultSerializer;
			requestConfig = requestConfig ?? GetBasicConfig();
			string queryString = GetQueryString(queryData);
			APIRequest<T> apiRequest = new APIRequest<T>(uri + queryString, requestConfig, ServiceResolver, callback, serializer, progressCallback);
			apiRequest.Send();
		}

		public void GetBytes(string uri, Action<APIResponse<byte[]>> callback, HTTPRequestConfig requestConfig = null, ISerializer serializer = null)
		{
			if (requestConfig == null)
			{
				requestConfig = GetBasicConfig();
				requestConfig.HttpDataFormat = HTTPDataFormats.RawData;
			}

			Get<object>(uri, delegate(APIResponse<object> response)
			{
				APIResponse<byte[]> apiResponse = new APIResponse<byte[]>();
				apiResponse.AddMessage(response.Messages);
				if (!response.Success)
				{
					apiResponse.SetFailure("Request failed!");
					callback.Invoke(apiResponse);
					return;
				}

				byte[] bytes = response.Data as byte[];
				if (bytes == null)
				{
					apiResponse.SetFailure("Failed converting data to bytes array!");
					callback.Invoke(apiResponse);
					return;
				}

				apiResponse.Data = bytes;
				callback.Invoke(apiResponse);
			}, requestConfig: requestConfig, serializer: serializer);
		}

		private string GetQueryString(Dictionary<string, string> queryData)
		{
			// IF query data is null or empty just return empty string...
			if(queryData == null || queryData.Count == 0) { return string.Empty; }

			_getQueryString.Clear();
			_getQueryString.Append(QuerySymbolQuestionMark);

			foreach (KeyValuePair<string, string> entry in queryData)
			{
				_getQueryString.Append(entry.Key + QuerySymbolEqual + entry.Value + QuerySymbolAdd);
			}

			return _getQueryString.ToString();
		}

		public override void Dispose()
		{

		}
	}
}
