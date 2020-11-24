using Foundation.ConfigurationResolver;
using Foundation.DeviceInfoService.Providers;
using Foundation.Logger;
using Foundation.ServicesResolver;
using UnityEngine;

namespace Foundation
{
	#region Global Enums
	
	public enum Device { Desktop, Smartphone, Tablet }

	public enum PlatformType { All, Android, WebGL, iOS, Unknown }

	public enum ConnectType { Amazon, Android, iOS, Web }

	public enum OSType { Android, iOS, Windows, Unknown }
	
	#endregion
}

namespace Foundation.DeviceInfoService
{
	
	public class DeviceService : BaseService
	{
		public PlatformType CurrentPlatform { get; private set; }
		public DeviceInfo Info { get; private set; }
		
		protected override void Initialize()
		{
			this.Log("Initializing Device Service");
			CurrentPlatform = GetRuntimePlatform();
			Info = GetInfoProvider(CurrentPlatform);
		}

		private DeviceInfo GetInfoProvider(PlatformType platform)
		{
			DeviceInfo deviceInfo = null;
			#if UNITY_EDITOR
			deviceInfo = new UnityEditorInfoProvider();
			#elif UNITY_ANDROID
			deviceInfo =  new AndroidInfoProvider();
			#elif UNITY_IOS
			deviceInfo =  new IOSInfoProvider();
			#elif UNITY_WEBGL
			deviceInfo =  new WebGLInfoProvider();
			#endif
			return deviceInfo;
		}

		private PlatformType GetRuntimePlatform()
		{
			PlatformType platformType = PlatformType.Unknown;
			#if UNITY_ANDROID
            platformType = PlatformType.Android;
			#elif UNITY_IOS
            platformType = PlatformType.iOS;
			#elif UNITY_WEBGL
			platformType = PlatformType.WebGL;
			#endif
			return platformType;
		}
		
		public override void Dispose()
		{
			Info = null;
		}
	}
	
	public static class PlatformTypeExtensions
	{
		public static PlatformType ToPlatformType(this RuntimePlatform runtimePlatform)
		{
			switch (Application.platform)
			{
				case RuntimePlatform.IPhonePlayer:
					return PlatformType.iOS;
				case RuntimePlatform.Android:
					return PlatformType.Android;
				case RuntimePlatform.WebGLPlayer:
					return PlatformType.WebGL;
			}
			return PlatformType.Unknown;
		}
	}
}
