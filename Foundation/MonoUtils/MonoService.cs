using System.Collections.Generic;
using Foundation.ConfigurationResolver;
using Foundation.ServicesResolver;
using UnityEngine;
using UnityEngine.Events;

namespace Foundation.MonoUtils
{
    /// <summary>
    /// A service to better handle Update style callbacks
    /// </summary>
    public class MonoService : BaseService
    {
        public class OnUpdateHandler : UnityEvent<float> {}
        public OnUpdateHandler OnUpdate { get; } = new OnUpdateHandler();
        
        #if ENABLE_LATE_UPDATE
        public class OnLateUpdateHandler : UnityEvent<float> {}
        public OnLateUpdateHandler OnLateUpdate { get; } = new OnLateUpdateHandler();
        #endif
        
        #if ENABLE_FIXED_UPDATE
        public class OnFixedUpdateHandler : UnityEvent<float> {}
        public OnFixedUpdateHandler OnFixedUpdate { get; } = new OnFixedUpdateHandler();
        #endif
        
        public class OnPerSecondUpdateHandler : UnityEvent<float> {}
        public OnPerSecondUpdateHandler OnPerSecondUpdate { get; } = new OnPerSecondUpdateHandler();
        
        public List<UnityAction<float>>[] OnSlowUpdate { get; private set; }

        public class OnApplicationFocusHandler : UnityEvent<bool> {}
        public OnApplicationFocusHandler OnApplicationFocusEvent { get; } = new OnApplicationFocusHandler();
    
        public class OnApplicationPauseHandler : UnityEvent<bool> {}
        public OnApplicationPauseHandler OnApplicationPauseEvent { get; } = new OnApplicationPauseHandler();
    
        public class OnApplicationQuitHandler : UnityEvent {}
        public OnApplicationQuitHandler OnApplicationQuitEvent { get; } = new OnApplicationQuitHandler();

        private int _slowUpdateRate;
        private float _slowUpdateTimeSum = 0f;
        private int _slowUpdateFrameCount = 0;
        private float _slowUpdateGroupAverage = 0f;
        private Dictionary<UnityAction<float>, int> _slowUpdateInsertionDict = new Dictionary<UnityAction<float>, int>();
        private float _perSecondUpdateCounter = 0f;

        private Queue<float> _frameDurations = new Queue<float>();
        private MonoServiceInjector _monoServiceInjector;
        private bool _disposed;
        
        protected override void Initialize()
        {
            MonoConfig config = GetConfig<MonoConfig>();
            SetMonoInjector();
            
            // Initialize Slow Update
            _slowUpdateRate = config.SlowUpdateRate;
            OnSlowUpdate = new List<UnityAction<float>>[_slowUpdateRate];
            for (int i = 0; i < _slowUpdateRate; i++)
            {
                OnSlowUpdate[i] = new List<UnityAction<float>>();
            }
        }

        private void SetMonoInjector()
        {
            GameObject injectorObject = new GameObject("MonoServiceInjector");
            _monoServiceInjector = injectorObject.AddComponent<MonoServiceInjector>();
            _monoServiceInjector.Init(this);
            GameObject.DontDestroyOnLoad(injectorObject);
        }
        
        internal void Update() {
            float deltaTime = Time.deltaTime;
            OnUpdate?.Invoke(deltaTime);
            PerSecondUpdate(deltaTime);
            SlowUpdate(deltaTime);
        }

        private void PerSecondUpdate(float deltaTime) {
            _perSecondUpdateCounter += deltaTime;
            if (!(_perSecondUpdateCounter >= 1f)) { return; }
            
            OnPerSecondUpdate.Invoke(_perSecondUpdateCounter);
            _perSecondUpdateCounter = 0f;
        }

        private void SlowUpdate(float deltaTime)
        {

            _frameDurations.Enqueue(deltaTime);
            _slowUpdateTimeSum += deltaTime;
            // Check if enough frames passed for the next slow update
            if (_frameDurations.Count < _slowUpdateRate) { return; }
            
            
            List<UnityAction<float>> slowUpdateGroup = OnSlowUpdate[_slowUpdateFrameCount];
            if (slowUpdateGroup != null)
            {
                for (int i = 0; i < slowUpdateGroup.Count; i++)
                {
                    slowUpdateGroup[i]?.Invoke(_slowUpdateTimeSum);
                }
            }
            
            _slowUpdateTimeSum -= _frameDurations.Dequeue();
            _slowUpdateFrameCount = (_slowUpdateFrameCount + 1) % _slowUpdateRate;
        }

        public void RegisterSlowUpdate(UnityAction<float> controlledUpdate)
        {
            if(_disposed) { return; }
            
            // No registered yet register to bucket 0
            if (_slowUpdateGroupAverage == 0f)
            {
                OnSlowUpdate[0].Add(controlledUpdate);
                _slowUpdateInsertionDict.Add(controlledUpdate, 0);
            }
            else
            {
                bool controlledUpdateInserted = false;
                for (int i = 0; i < OnSlowUpdate.Length; i++) {
                    // Find a bucket with less than average load, register to that
                    if (OnSlowUpdate[i].Count < _slowUpdateGroupAverage) {
                        OnSlowUpdate[i].Add(controlledUpdate);
                        _slowUpdateInsertionDict.Add(controlledUpdate, i);
                        controlledUpdateInserted = true;
                        break;
                    }
                }
                
                // Everything is uniform, insert into first bucket
                if (!controlledUpdateInserted) {
                    OnSlowUpdate[0].Add(controlledUpdate);
                    _slowUpdateInsertionDict.Add(controlledUpdate, 0);
                }
            }
            
            // Update average
            _slowUpdateGroupAverage += 1f / _slowUpdateRate;
        }

        public void UnregisterSlowUpdate(UnityAction<float> controlledUpdate)
        {
            if(_disposed) { return; }

            if (!_slowUpdateInsertionDict.ContainsKey(controlledUpdate))
            {
                return;
            }
            
            OnSlowUpdate[_slowUpdateInsertionDict[controlledUpdate]].Remove(controlledUpdate);
            _slowUpdateInsertionDict.Remove(controlledUpdate);
            //Update average
            _slowUpdateGroupAverage -= 1f / _slowUpdateRate;
        }

        #if ENABLE_LATE_UPDATE
        internal void LateUpdate()
        {
            OnLateUpdate?.Invoke(Time.deltaTime);
        }
        #endif

        #if ENABLE_FIXED_UPDATE
        internal void FixedUpdate()
        {
            OnFixedUpdate?.Invoke(Time.deltaTime);
        }
        #endif
        
        internal void OnApplicationFocus(bool hasFocus)
        {
            OnApplicationFocusEvent?.Invoke(hasFocus);
        }

        internal void OnApplicationPause(bool pauseStatus)
        {
            OnApplicationPauseEvent?.Invoke(pauseStatus);
        }

        internal void OnApplicationQuit()
        {
            OnApplicationQuitEvent?.Invoke();
        }

        public override void Dispose()
        {
            _disposed = true;
            OnUpdate.RemoveAllListeners();
            OnPerSecondUpdate.RemoveAllListeners();
            #if ENABLE_LATE_UPDATE
            OnLateUpdate.RemoveAllListeners();
            #endif
            
            #if ENABLE_FIXED_UPDATE
            OnFixedUpdate.RemoveAllListeners();
            #endif

            foreach (List<UnityAction<float>> list in OnSlowUpdate)
            {
                list.Clear();
            }

            OnSlowUpdate = null;
            
            OnApplicationFocusEvent.RemoveAllListeners();
            OnApplicationPauseEvent.RemoveAllListeners();
            OnApplicationQuitEvent.RemoveAllListeners();

            if (_monoServiceInjector != null)
            {
                #if UNITY_EDITOR
                GameObject.DestroyImmediate(_monoServiceInjector.gameObject);
                #else
                GameObject.Destroy(_monoServiceInjector.gameObject);
                #endif
            }
        }
    }
}
