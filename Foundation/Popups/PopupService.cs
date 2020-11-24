using System;
using System.Collections.Generic;
using System.Linq;
using Foundation.AssetBundles;
using Foundation.Logger;
using Foundation.MonoUtils;
// using Foundation.Popups.DEMO_DELETE_ME;
using Foundation.Popups.MVC;
using Foundation.ServicesResolver;
using Foundation.Utils.OperationUtils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Foundation.Popups
{
    public class PopupService : BaseService, IPopupService
    {
        private enum PopupServiceState { Blocked, UnBlocked }

        public event PopupServiceCallback OnLastPopupClosed;
        public bool IsBlocked => _currentServiceState == PopupServiceState.Blocked;

        private PopupFactory _popupFactory;

        private PopupServiceState _currentServiceState;
        private List<PopupBehavior> _popupsQueue;
        private List<PopupBehavior> _popupsStack;
        private PopupBehavior _currentPopup;

        public PopupService()
        {
            _popupsQueue = new List<PopupBehavior>();
            _popupsStack = new List<PopupBehavior>();
        }

        protected override void Initialize()
        {
            _popupFactory = new PopupFactory(ServiceResolver);
            SetServiceState(PopupServiceState.Blocked);
        }

        #region API

        /// <summary>
        /// Insert popup bypassing priority
        /// </summary>
        /// <param name="popupContext">Model for popup</param>
        public void InsertPopup<T>(PopupContext<T> popupContext) where T : BasePopupView
        {
            PreparePopup<T>(popupContext, true);
        }
        /// <summary>
        /// Add popup to the queue. Will be shown regarding to it's priority.
        /// </summary>
        /// <param name="popupContext">Model for popup</param>
        public void AddPopup<T>(PopupContext<T> popupContext) where T : BasePopupView
        {
            PreparePopup<T>(popupContext);
        }

        /// <summary>
        /// Show new popup and navigate back to previous after closing
        /// </summary>
        /// <param name="popupContext">Model for popup</param>
        public void ShowNestedPopup<T>(PopupContext<T> popupContext) where T : BasePopupView
        {
            ClosePopup(_currentPopup, true);
            InsertPopup<T>(popupContext);
        }

        /// <summary>
        /// Close current popup
        /// </summary>
        public void CloseCurrentPopup()
        {
            ClosePopup(_currentPopup);
        }

        /// <summary>
        /// Enable/Disable state of Popup system
        /// </summary>
        public void ChangeState()
        {
            SetServiceState(IsBlocked ? PopupServiceState.UnBlocked : PopupServiceState.Blocked);
        }

        #endregion

        #region Internal methods
        private void SetServiceState(PopupServiceState state)
        {
            var oldState = _currentServiceState;
            _currentServiceState = state;
            if (oldState == PopupServiceState.Blocked && !IsBlocked)
            {
                ShowAvailablePopups();
            }
        }

        private void PreparePopup<T>(PopupContext<T> context, bool isInserted = false) where T : BasePopupView
        {
            if (!AssetBundlesService.AssetExists(context.AssetName))
            {
                this.LogError($"Popup {context.AssetName} doesn't exists in manifest!");
                return;
            }

            var popup = new PopupBehavior(context, CloseCurrentPopup);
            if (isInserted)
            {
                _popupsStack.Add(popup);
            }
            else
            {
                _popupsQueue.Add(popup);
                _popupsQueue = _popupsQueue.OrderBy(p => p.Context.Priority).ToList();
            }

            popup.OnPopupClosed += OnPopupClosed;

            ShowAvailablePopups();
        }

        private void ShowAvailablePopups()
        {
            if (IsBlocked) { return; }
            if (_currentPopup != null && _currentPopup.IsProcessing) { return; }

            PopupBehavior popup = null;
            if (_popupsStack.Count > 0)
            {
                popup = _popupsStack[_popupsStack.Count - 1];
            }
            else if (_popupsQueue.Count > 0)
            {
                popup = _popupsQueue[0];
                _popupsStack.Clear();
                _popupsQueue.Remove(popup);
                _popupsStack.Add(popup);
            }

            // Show next popup if exists...
            if (popup != null && popup.IsPending)
            {
                ShowPopup(popup);
            }
        }

        private void ShowPopup(PopupBehavior popup)
        {
            OperationsQueue
            .Do<PopupBehavior, PopupBehavior>(popup, CreatePopupAsset)
            .Then<PopupBehavior>(ShowPopup)
            .Run();
        }
        
        private void CreatePopupAsset(PopupBehavior popup, Action<Result<PopupBehavior>> operationComplete)
        {
            if (popup.PopupView == null)
            {
                _popupFactory.CreatePopupAsset(popup, operationComplete);
            }
            else
            {
                Result<PopupBehavior> result = new Result<PopupBehavior>();
                result.Data = popup;
                operationComplete.Invoke(result);
            }
        }
        
        private void ShowPopup(PopupBehavior popup, Action<Result> complete)
        {
            popup.Show();
            _currentPopup = popup;
        }

        private void ClosePopup(PopupBehavior popup, bool returnable = false)
        {
            if (popup == null) { return; }
            if (!popup.IsShowing) { return; }
            popup.IsReturnable = returnable;
            popup.Close();
        }
        #endregion

        #region Event Reactions

        private void OnPopupClosed(PopupBehavior popup)
        {
            if (!popup.IsReturnable)
            {
                _popupsStack.Remove(popup);
                Object.Destroy(popup.PopupView.gameObject);
            }
            if (_popupsQueue.Count + _popupsStack.Count == 0)
            {
                OnLastPopupClosed?.Invoke();
            }
            ShowAvailablePopups();
        }

        #endregion

        #region BaseService


        public override void Dispose()
        {
            _popupsQueue.Clear();
            _popupsStack.Clear();
            _currentPopup = null;
        }

        #endregion
    }
}
