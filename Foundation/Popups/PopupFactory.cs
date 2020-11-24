using System;
using Foundation.AssetBundles;
using Foundation.Popups.MVC;
using Foundation.Utils.OperationUtils;
using Foundation.ServicesResolver;
    
namespace Foundation.Popups
{
    public class PopupFactory
    {
        private readonly IAssetBundlesService _assetBundleService;

        public PopupFactory(ServiceResolver servicesResolver)
        {
            _assetBundleService = servicesResolver.Resolve<IAssetBundlesService>();
        }

        public void CreatePopupAsset(PopupBehavior popupBehavior, Action<Result<PopupBehavior>> operationComplete)
        {
            _assetBundleService.LoadAsset(popupBehavior.Context.AssetName, popupBehavior.Context.FallbackAssetName, delegate(Result<BasePopupView> result)
            {
                Result<PopupBehavior> popupResult = new Result<PopupBehavior>();
                popupResult.Data = popupBehavior;
                BasePopupView popupView = result.Data;
                popupBehavior.OnPopupLoaded(popupView);
                // Lastly, invoke callback
                operationComplete.Invoke(popupResult);
            });
        }
    }
}
