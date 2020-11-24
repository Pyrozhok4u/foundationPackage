using System.Collections.Generic;
using AppsFlyerSDK;
using Foundation.ServicesResolver;

namespace Foundation.AppsFlyer
{
	public class AppsFlyerService : BaseService, IAppsFlyerConversionData
	{

		protected override void Initialize()
		{
			AppsFlyerConfig config = GetConfig<AppsFlyerConfig>();
			AppsFlyerSDK.AppsFlyer.setIsDebug(config.IsDebug);
			AppsFlyerSDK.AppsFlyer.initSDK(config.DevKey, config.AppId);
			AppsFlyerSDK.AppsFlyer.startSDK();
		}

		public void onConversionDataSuccess(string conversionData)
		{
			AppsFlyerSDK.AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
			Dictionary<string, object> conversionDataDictionary = AppsFlyerSDK.AppsFlyer.CallbackStringToDictionary(conversionData);
		}

		public void onConversionDataFail(string error)
		{
			AppsFlyerSDK.AppsFlyer.AFLog("onConversionDataFail", error);
		}

		public void onAppOpenAttribution(string attributionData)
		{
			AppsFlyerSDK.AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
			Dictionary<string, object> attributionDataDictionary = AppsFlyerSDK.AppsFlyer.CallbackStringToDictionary(attributionData);
		}

		public void onAppOpenAttributionFailure(string error)
		{
			AppsFlyerSDK.AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
		}

		public override void Dispose()
		{

		}
	}
}
