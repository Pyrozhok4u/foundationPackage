using Foundation.MonoUtils;
using Foundation.Popups.MVC;
using Foundation.ServicesResolver;

namespace Foundation.Popups
{
    public delegate void PopupServiceCallback();

    public interface IPopupService : IService
    {
        bool IsBlocked { get; }
        
        event PopupServiceCallback OnLastPopupClosed;

        void InsertPopup<T>(PopupContext<T> popupContext) where T : BasePopupView;
        void AddPopup<T>(PopupContext<T> popupContext) where T : BasePopupView;
        void ShowNestedPopup<T>(PopupContext<T> popupContext) where T : BasePopupView;
        void CloseCurrentPopup();
        void ChangeState();
    }
}
