using System;

namespace Foundation.Popups.MVC
{
	internal interface IPopupContext
	{
		int Priority { get; }
		string AssetName { get; }
		string FallbackAssetName { get; }

		
		Action<BasePopupView> OnPopupLoaded { get; }
		Action OnPopupDisplayed { get; }
		Action OnPopupClosed { get; }
	}
}
