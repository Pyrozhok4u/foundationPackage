using System;
using Foundation.Logger;
using Foundation.MonoUtils;
using Foundation.Popups.Transitions;
using UnityEngine;
using UnityEngine.UI;

namespace Foundation.Popups.MVC
{
    [RequireComponent(typeof(Transition))]
    public abstract class BasePopupView : BaseMonoBehaviour
    {
        [SerializeField] protected Transition _transition;
        
        public Transition Transition => _transition;

        private Action _closePopup;

        internal void OnPopupClosed(Action closePopup)
        {
            _closePopup = closePopup;
        }
        
        private void OnValidate()
        {
            if (_transition == null)
            {
                _transition = gameObject.GetComponent<Transition>();
            }
        }

        public void Close()
        {
            _closePopup?.Invoke();
        }
    }
}
