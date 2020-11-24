#if UNITY_IOS
using System;

namespace Foundation.DeviceInfoService.Providers
{
	public class IOSInfoProvider : DeviceInfo
	{
		public bool IsDeviceIsIPad => _isDeviceIPad;
		public bool IsDeviceIsIPhone => _isDeviceIPhone;
		public bool IsDeviceIsIPod => _isDeviceIPod;

		private readonly Device _device;
		private readonly bool _isDeviceIPad;
		private readonly bool _isDeviceIPhone;
		private readonly bool _isDeviceIPod;

		public IOSInfoProvider()
		{
			string deviceGeneration = UnityEngine.iOS.Device.generation.ToString();
			_isDeviceIPad = deviceGeneration.Contains("iPad");
			_isDeviceIPhone = deviceGeneration.Contains("iPhone");
			_isDeviceIPod = deviceGeneration.Contains("iPod");
			_device = IsDeviceIsIPad ? Device.Tablet : Device.Smartphone;
		}

		public override Device GetDeviceType()
		{
			return _device;
		}

		protected override string GetAppleProvider()
		{
			return string.Empty;
		}
	}
}
#endif
