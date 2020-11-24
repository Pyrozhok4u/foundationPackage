using Google.Protobuf;

namespace Foundation.Network
{
	public static class NetworkUtils
	{
		public static JsonFormatter JsonFormatter {
			get;
		} = new JsonFormatter(new JsonFormatter.Settings(false));
	}
}
