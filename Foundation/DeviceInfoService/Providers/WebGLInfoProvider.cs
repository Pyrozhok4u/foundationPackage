#if UNITY_WEBGL
using System.Runtime.InteropServices;

namespace Foundation.DeviceInfoService.Providers
{
	// ReSharper disable once InconsistentNaming
	public class WebGLInfoProvider : DeviceInfo
	{
		// JS plugin located in Assets/Plugins/WebGL - return value are similar to "Chrome 40" or "Safari 10"
		[DllImport("__Internal")] private static extern string GetBrowserVersion();
		
		private readonly string _browserInfo;

		public WebGLInfoProvider()
		{
			_browserInfo = GetNativeBrowserInfo();
		}

		protected override string GetBrowserInfo()
		{
			return _browserInfo;
		}

		public override Device GetDeviceType()
		{
			return Device.Desktop;
		}
		
		private string GetNativeBrowserInfo()
		{
			return GetBrowserVersion();
		}
	}
}
#endif
