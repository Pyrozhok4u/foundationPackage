using Foundation.Utils.OperationUtils;

namespace Foundation.Network
{
	public enum APIErrorCodes
	{
		Parse = 1,
		Error = 2,
		Aborted = 3,
		TimedOut = 4,
		Unknown = 5,
	}

	public class APIResponse<T> : Result<T>
	{
		public int ErrorCode;
	}
}
