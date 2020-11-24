using System;
using BestHTTP;
using Foundation.TimerUtils;
using System.Collections.Generic;
using System.Text;
using Foundation.Logger;
using Foundation.ServicesResolver;
using Foundation.Utils.OperationUtils;
using Foundation.Utils.SerializerUtils;

namespace Foundation.Network
{
	public class APIProgress
	{
		public float Progress { get; set; }
	}

	public class APIRequest<T> : IDisposable where T : class, new()
	{
		internal HTTPRequest Request;

		protected HTTPRequestConfig RequestConfig;
		protected readonly string Uri;
		protected readonly ISerializer Serializer;

		private Timer _delayTimer;
		private Timer _timeoutTimer;
		private Action<APIResponse<T>> _onComplete;
		private readonly ITimerService _timerService;
		private readonly APIProgress _apiProgress = new APIProgress();
		private readonly Action<APIProgress> _progressCallback;

		public APIRequest(string uri, HTTPRequestConfig requestConfig, ServiceResolver serviceResolver,
			Action<APIResponse<T>> callback, ISerializer serializer, Action<APIProgress> progressCallback = null)
		{
			Uri = uri;
			RequestConfig = requestConfig;
			_onComplete = callback;
			Serializer = serializer;
			_progressCallback = progressCallback;
			_timerService = serviceResolver.Resolve<ITimerService>();
		}

		public void Send()
		{
			CreateRequest();
			Request.OnDownloadProgress += OnDownloadProgress;
			_timeoutTimer = _timerService.StartTimerMs(RequestConfig.Timeout, OnTimeout);
			Request.Send();
		}

		private void OnDownloadProgress(HTTPRequest originalrequest, long downloaded, long downloadlength)
		{
			_timeoutTimer?.StopTimer();
			_apiProgress.Progress = (downloaded / (float)downloadlength);
			_timeoutTimer = _timerService.StartTimerMs(RequestConfig.Timeout, OnTimeout);
			_progressCallback?.Invoke(_apiProgress);
		}

		protected virtual void CreateRequest()
		{
			Request = new HTTPRequest(new Uri(Uri), HTTPMethods.Get, OnComplete);
			SetHeaders();
		}

		internal void SetHeaders()
		{
			foreach (KeyValuePair<string, string> entry in RequestConfig.Headers)
			{
				Request.SetHeader(entry.Key, entry.Value);
			}
		}

		internal void OnComplete(HTTPRequest req, HTTPResponse resp)
		{
			_timeoutTimer.StopTimer();
			APIResponse<T> apiResponse = new APIResponse<T>();
			string reason = String.Empty;
			int errorCode = 0;
 			switch (req.State)
			{
				// The request finished without any problem.
				case HTTPRequestStates.Finished:
					if (resp.IsSuccess)
					{
						//Parse(resp, apiResponse, ref errorCode, ref reason);
						ParseResponseResult result = ParseResponse(resp);
						if (!result.Success)
						{
							errorCode = result.ErrorCode;
							reason = result.Reason;
						}
						else
						{
							apiResponse.Data = result.Data;
							this.Log("APIRequest Success! APIResponse: " + apiResponse);
						}
					}
					else
					{
						errorCode = resp.StatusCode;
						reason = "APIRequest Server Return Error - url: " + req.Uri;
					}
					break;
				// The request finished with an unexpected error. The request's Exception property may contain more info about the error.
				case HTTPRequestStates.Error:
					errorCode = (int)APIErrorCodes.Error;
					reason = "APIRequest Finished with Error! " + (req.Exception != null
						? (req.Exception.Message + "\n" + req.Exception.StackTrace)
						: "No Exception");
					break;
				// The request aborted, initiated by the user.
				case HTTPRequestStates.Aborted:
					errorCode = (int)APIErrorCodes.Aborted;
					reason = "APIRequest Aborted!";
					break;
				// Connecting to the server is timed out.
				case HTTPRequestStates.ConnectionTimedOut:
					errorCode = (int)APIErrorCodes.TimedOut;
					reason = "APIRequest Connection Timed Out!";
					break;
				default:
					errorCode = (int)APIErrorCodes.Unknown;
					reason = "APIRequest Unknown Error: " + req.State;
					break;
			}
			if (errorCode > 0)
			{
				apiResponse.ErrorCode = errorCode;
				apiResponse.SetFailure(reason + " Error Code:" + errorCode);
				if (Retry())
				{
					CreateRequest();
					this.LogWarning("APIRequest Failed! Retry! APIResponse: " + apiResponse + " url: " + req.Uri);
					return;
				}
				this.LogError("APIRequest Failed! Return Error! APIResponse: " + apiResponse + " url: " + req.Uri);
			}
			_onComplete(apiResponse);
		}

		private ParseResponseResult ParseResponse(HTTPResponse resp)
		{
			ParseResponseResult result = new ParseResponseResult();
			try
			{
				switch (RequestConfig.HttpDataFormat)
				{
					case HTTPDataFormats.JSON:
						result.Data = Serializer.DecodeJson<T>(resp.DataAsText);
						break;
					case HTTPDataFormats.Bytes:
						result.Data = Serializer.DecodeBytes<T>(resp.Data);
						break;
					case HTTPDataFormats.RawData:
						result.Data = resp.Data as T;
						break;
					default:
						throw new Exception("Unsupported Data Format");
				}
			}
			catch (Exception e)
			{
				result.SetFailure($"APIRequest Parse Failed! Error: {e.Message}");
				result.ErrorCode = (int)APIErrorCodes.Parse;
				result.Reason = $"APIRequest Parse Failed! Error: {e.Message}";
			}
			return result;
		}

		private bool Retry()
		{
			if (RequestConfig.RetryCount == 0)
			{
				return false;
			}
			RequestConfig.RetryCount -= 1;
			RequestConfig.RetryDelay *= RequestConfig.Backoff;
			this.LogWarning($"APIRequest Retry! Retries Left: {RequestConfig.RetryCount}, Delay: {RequestConfig.RetryDelay}");
			_timerService.StartTimerMs(RequestConfig.RetryDelay, Send);
			return true;
		}

		private void OnTimeout()
		{
			Request.OnDownloadProgress -= OnDownloadProgress;
			Request.Abort();
			this.LogWarning($"APIRequest Timed Out! Timeout: {RequestConfig.Timeout}");
		}

		public void Dispose()
		{
			if (Request != null)
			{
				Request.OnDownloadProgress -= OnDownloadProgress;
			}
			Request = null;
			RequestConfig = null;
			_timeoutTimer?.StopTimer();
			_timeoutTimer = null;
			_onComplete = null;
		}

		private class ParseResponseResult : Result
		{
			public int ErrorCode;
			public string Reason;
			public T Data;
		}
	}

	public class APIRequest<T, TU> : APIRequest<TU> where T : class where TU : class, new()
	{
		private readonly T _message;
		private byte[] _cache;

		public APIRequest(string uri, T message, HTTPRequestConfig requestConfig, ServiceResolver serviceResolver,
			Action<APIResponse<TU>> callback, ISerializer serializer, Action<APIProgress> progressCallback)
			: base(uri, requestConfig, serviceResolver, callback, serializer, progressCallback)
		{
			_message = message;
		}

		private void SetData()
		{
			if (RequestConfig.HttpDataFormat == HTTPDataFormats.JSON)
			{
				Request.SetHeader("Content-Type", "application/json; charset=UTF-8");
				_cache = Request.RawData = _cache ?? Encoding.UTF8.GetBytes(Serializer.EncodeJson<T>(_message));
			}
			else
			{
				Request.SetHeader("Content-Type", "application/octet-stream");
				_cache = Request.RawData = _cache ?? Serializer.EncodeBytes<T>(_message);
			}
		}

		protected sealed override void CreateRequest()
		{
			Request = new HTTPRequest(new Uri(Uri), HTTPMethods.Post, OnComplete);
			SetHeaders();
			SetData();
		}
	}
}
