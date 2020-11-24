using System;
using Foundation.Popups.Transitions;

namespace Foundation.Popups.MVC
{
    public sealed class PopupBehavior
    {
        private enum PopupState
        {
            Initialize,
            Pending,
            Transition,
            Showing,
            Closed
        }

        internal bool IsShowing { get { return _state.Equals(PopupState.Showing); } }
        internal bool IsPending { get { return _state.Equals(PopupState.Pending); } }
        internal bool IsInitializing { get { return _state.Equals(PopupState.Initialize); } }
        internal bool IsInTransition { get { return _state.Equals(PopupState.Transition); } }
        internal bool IsProcessing { get { return IsInitializing || IsShowing || IsInTransition; } }
        internal bool IsReturnable { get; set; }
        internal BasePopupView PopupView { get; private set; }
        internal IPopupContext Context { get; private set; }

        internal event Action<PopupBehavior> OnPopupClosed;

        private PopupState _state;
        private Transition _transition;
        private Action _onPopupClosed;

        internal PopupBehavior(IPopupContext context, Action onPopupClosed)
        {
            Context = context;
            _onPopupClosed = onPopupClosed;
            SetState(PopupState.Pending);
        }

        internal void Show()
        {
            SetState(PopupState.Transition);
            _transition.In(() =>
            {
                //return to popup - reset the flag
                if (IsReturnable) { IsReturnable = false; }
                SetState(PopupState.Showing);
            });
            Context.OnPopupDisplayed?.Invoke();
        }

        internal void Close()
        {
            SetState(PopupState.Transition);
            _transition.Out(() =>
            {
                if (IsReturnable)
                {
                    SetState(PopupState.Pending);
                }
                else
                {
                    SetState(PopupState.Closed);
                }
                OnPopupClosed?.Invoke(this);
                Context.OnPopupClosed?.Invoke();
            });
        }
        
        internal void OnPopupLoaded(BasePopupView popupView)
        {
            PopupView = popupView;
            _transition = popupView.Transition;
            popupView.OnPopupClosed(_onPopupClosed);
            Context.OnPopupLoaded.Invoke(popupView);
        }
        
        private void SetState(PopupState newState)
        {
            _state = newState;
        }
    }
}
