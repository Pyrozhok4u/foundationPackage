using System;
using Foundation.MonoUtils;
using Foundation.Utils.OperationUtils;
using UnityEngine;

namespace Foundation.Popups.MVC
{
    [Serializable]
    public class PopupContext<T> : IPopupContext where T : BasePopupView
    {
        public string AssetName { get; }
        public string FallbackAssetName { get; }

        public Action<BasePopupView> OnPopupLoaded { get; }

        public Action OnPopupDisplayed { get; }
        public Action OnPopupClosed { get; }
        
        public int Priority => _priority == -1 ? int.MaxValue : _priority;

        private int _priority;
        
        internal PopupContext(string assetName, string fallbackAssetName, int priority, Action<T> onPopupLoaded,  Action onPopupDisplayed, Action onPopupClosed = null)
        {
            AssetName = assetName;
            FallbackAssetName = fallbackAssetName;
            _priority = priority;
            OnPopupLoaded = view => onPopupLoaded(view as T);
            OnPopupDisplayed = onPopupDisplayed;
            OnPopupClosed = onPopupClosed;
        }
    }
}
