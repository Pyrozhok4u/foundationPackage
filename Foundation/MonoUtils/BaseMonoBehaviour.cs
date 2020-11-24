using System;
using System.Collections.Generic;
using Foundation.ClientService;
using UnityEngine;

namespace Foundation.MonoUtils
{
    
    /// <summary>
    /// Base class for mono behaviour that caches the internal components for faster retrieval
    /// Initialize only on demand upon the first call to the property
    /// </summary>
    public abstract class BaseMonoBehaviour : MonoBehaviour
    {

        // Quick access to base client
        protected Client Client => Client.BaseInstance;
        
        private GameObject _cachedGameObject;
        private Transform _cachedTransform;
        private RectTransform _cachedRectTransform;
        private Dictionary<Type, Component> _cachedComponents;

        
        // ReSharper disable once InconsistentNaming
        public new GameObject gameObject
        {
            get
            {
                if (_cachedGameObject == null) { _cachedGameObject = base.gameObject; }
                return _cachedGameObject;
            }
        }
        
        // ReSharper disable once InconsistentNaming
        public new Transform transform
        {
            get
            {
                if (_cachedTransform == null) { _cachedTransform = base.transform; }
                return _cachedTransform;
            }
        }
        
        // ReSharper disable once InconsistentNaming
        public RectTransform rectTransform
        {
            get
            {
                if (_cachedRectTransform == null) { _cachedRectTransform = transform as RectTransform; }
                return _cachedRectTransform;
            }
        }

        public TComponent GetCachedComponent<TComponent>() where TComponent : Component
        {
            const int capacity = 4;
            
            if (_cachedComponents == null)
            {
                _cachedComponents = new Dictionary<Type, Component>(capacity);
            }

            if (!_cachedComponents.TryGetValue(typeof(TComponent), out Component tComponent))
            {
                tComponent = GetComponent<TComponent>();
                _cachedComponents.Add(typeof(TComponent), tComponent);
            }

            return tComponent as TComponent;
        }
    }
}
