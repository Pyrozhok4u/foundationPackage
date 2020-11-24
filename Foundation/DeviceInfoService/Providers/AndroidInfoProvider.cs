#if UNITY_ANDROID
using UnityEngine;

namespace Foundation.DeviceInfoService.Providers
{
	public class AndroidInfoProvider : DeviceInfo
	{
		private readonly Device _device;
		private readonly float _dpi;
		private readonly string _architecture;
		
		public AndroidInfoProvider()
		{
			_device = GetDevice();
			_dpi = GetNativeDPI();
			_architecture = GetNativeArchitecture();
		}
		
		public override Device GetDeviceType()
		{
			return _device;
		}

		public override float GetDPI()
		{
			return _dpi;
		}

		protected override string GetArchitecture()
		{
			return _architecture;
		}
		
		private string GetNativeArchitecture()
		{
			var system = new AndroidJavaClass("java.lang.System");
			return system.CallStatic<string>("getProperty", "os.arch");
		}
		
		/// <summary>
		/// recommended way of getting DPI for Android 
		/// will have to check on device both values
		/// </summary>
		private float GetNativeDPI()
		{
			AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject metrics = new AndroidJavaObject("android.util.DisplayMetrics");
			activity.Call<AndroidJavaObject>("getWindowManager").Call<AndroidJavaObject>("getDefaultDisplay").Call("getMetrics", metrics);
			var dpi = (metrics.Get<float>("xdpi") + metrics.Get<float>("ydpi")) * 0.5f;
			return (metrics.Get<float>("xdpi") + metrics.Get<float>("ydpi")) * 0.5f;
		}


		private Device GetDevice()
		{
			float aspectRatio = Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);
			bool isTablet = DiagonalSizeInches > 6.5f && aspectRatio < 2f;
			return isTablet ? Device.Tablet : Device.Smartphone;
		}
	}
}
#endif
