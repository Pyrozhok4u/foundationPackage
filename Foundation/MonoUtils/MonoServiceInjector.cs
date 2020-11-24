using System;

namespace Foundation.MonoUtils
{
    internal class MonoServiceInjector : BaseMonoBehaviour
    {

        private MonoService _monoService;

        internal void Init(MonoService monoService)
        {
            _monoService = monoService;
        }
        
        private void Update()
        {
            _monoService?.Update();
        }

        #if ENABLE_LATE_UPDATE
        private void LateUpdate()
        {
            _monoService?.LateUpdate();
        }
        #endif

        #if ENABLE_FIXED_UPDATE
        private void FixedUpdate()
        {
            _monoService?.FixedUpdate();
        }
        #endif

        private void OnApplicationFocus(bool hasFocus)
        {
            _monoService?.OnApplicationFocus(hasFocus);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            _monoService?.OnApplicationPause(pauseStatus);
        }

        private void OnApplicationQuit()
        {
            _monoService?.OnApplicationQuit();
        }
    }
}
