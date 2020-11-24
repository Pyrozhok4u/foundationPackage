#if ENABLE_ERROR_ALERTS

using Foundation.ConfigurationResolver;
using Foundation.ServicesResolver;
using UnityEngine;

namespace Foundation.DebugUtils.ErrorAlertService
{
    public class ErrorAlertService : BaseService
    {
        protected override void Initialize()
        {
            if (!GetConfig<ErrorAlertConfig>().IsEnabled) { return; }
            Application.logMessageReceivedThreaded += OnLogReceived;
        }

        public override void Dispose()
        {
            if (!GetConfig<ErrorAlertConfig>().IsEnabled) { return; }
            Application.logMessageReceivedThreaded -= OnLogReceived;
        }

        private void OnLogReceived(string condition, string stacktrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                {
                    CreatePopupAsset(condition, stacktrace, type);
                    break;
                }
            }
        }

        private void CreatePopupAsset(string condition, string stacktrace, LogType type)
        {
            var prefab = Resources.Load(ErrorAlertConfig.PrefabName) as GameObject;
            var popupGameObject = Object.Instantiate(prefab);
            var alertPopup = popupGameObject.GetComponent<ErrorAlertPopup>();
            alertPopup.Init(condition, stacktrace, type);
        }
    }
}
#endif
