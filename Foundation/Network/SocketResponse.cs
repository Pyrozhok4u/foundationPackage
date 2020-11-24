using System;
using Google.Protobuf;

namespace Foundation.Network
{
	public interface ISocketResponse
	{
		string EventName { get; }
		void OnResponseReceived(ByteString byteString);
		void OnResponseReceived(object apiResponse);
	}

	public class SocketResponse<T> : ISocketResponse  where T : IMessage, new()
	{
		public string EventName { get; }
		public APIResponse<T> ApiResponse { get; private set; }

		private readonly Action<APIResponse<T>> _callback;

		public SocketResponse(string eventName, Action<APIResponse<T>> callback)
		{
			EventName = eventName;
			_callback = callback;
		}

		public SocketResponse(Action<APIResponse<T>> callback)
		{
			_callback = callback;
		}

		public void OnResponseReceived(APIResponse<T> apiResponse)
		{
			_callback(apiResponse);
		}

		public void OnResponseReceived(object response)
		{
			SocketResponse<T> socketResponse = response as SocketResponse<T>;
			_callback(socketResponse?.ApiResponse);
		}

		public void OnResponseReceived(ByteString byteString)
		{
			ApiResponse = new APIResponse<T>();
			try
			{
				ApiResponse.Data = (T) new T().Descriptor.Parser.ParseFrom(byteString);
			}
			catch (Exception e)
			{
				ApiResponse.SetFailure(e.Message);
				ApiResponse.ErrorCode = (int)APIErrorCodes.Parse;
				throw;
			}
			_callback?.Invoke(ApiResponse);
		}
	}
}
