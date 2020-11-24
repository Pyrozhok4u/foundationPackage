using Foundation.Logger;
using UnityEngine;
using Debug = Foundation.Logger.Debugger;

namespace Foundation.DeviceInfoService
{
	public abstract class DeviceInfo
	{

		public string UID => _uid;
		public string ClientVersion => _clientVersion;
		public RuntimePlatform Platform => _platform;
		public DeviceType DeviceType => _deviceType;
		public string OperatingSystem => _operatingSystem;
		public Resolution ScreenResolution => _screenResolution;
		public float DiagonalSizeInches => _diagonalSizeInches;
		
		private readonly string _uid;
		private readonly string _clientVersion;
		private readonly string _resolution;
		private readonly RuntimePlatform _platform;
		private readonly DeviceType _deviceType;
		private readonly string _operatingSystem;
		private readonly Resolution _screenResolution;
		private readonly float _diagonalSizeInches;
		private readonly OSType _operatingSystemType;

		protected DeviceInfo()
		{
			_uid = SystemInfo.deviceUniqueIdentifier;
			_clientVersion = Application.version;
			_resolution = GetNativeResolution();
			_platform = Application.platform;
			_deviceType = SystemInfo.deviceType;
			_operatingSystemType = GetNativeOSType();
			_screenResolution = Screen.currentResolution;
			_operatingSystem = SystemInfo.operatingSystem;
			_diagonalSizeInches = DeviceDiagonalSizeInInches();
		}
		
		public virtual string GetResolution()
		{
			return _resolution;
		}

		public virtual OSType GetOSType()
		{
			return _operatingSystemType;
		}

		public virtual float GetDPI()
		{
			return Screen.dpi;
		}

		protected virtual string GetArchitecture()
		{
			return SystemInfo.processorType;
		}

		protected virtual string GetBrowserInfo()
		{
			return string.Empty;
		}

		protected virtual string GetAppleProvider()
		{
			return string.Empty;
		}

		private string GetNativeResolution()
		{
			string resolution = Screen.currentResolution.ToString();
			return resolution.Substring(0, resolution.IndexOf('@') - 1);
		}
		
		private float DeviceDiagonalSizeInInches()
		{
			float screenWidth = Screen.width / Screen.dpi;
			float screenHeight = Screen.height / Screen.dpi;
			float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));
			return diagonalInches;
		}
		
		private OSType GetNativeOSType()
		{
			switch (SystemInfo.operatingSystemFamily)
			{
				case OperatingSystemFamily.Other:
					this.LogError("Operating system is not identified");
					return OSType.Unknown;
				case OperatingSystemFamily.MacOSX:
					return OSType.iOS;
				case OperatingSystemFamily.Windows:
					return OSType.Windows;
				case OperatingSystemFamily.Linux:
					return OSType.Android;
			}
			return OSType.Unknown;
		}
		
		public abstract Device GetDeviceType();
		
	}
}
