using DG.Tweening;
using Foundation.Logger;
using Foundation.MonoUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Foundation.DebugUtils.ErrorAlertService
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ErrorAlertPopup : BaseMonoBehaviour
    {
        [SerializeField] private Button _okButton;
        [SerializeField] private Text _title;
        [SerializeField] private Text _message;
        [SerializeField] private Text _trace;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField, Range(0,1)] private float _animationTime = 1;

        private void OnValidate()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
            if (_okButton == null)
            {
                var buttonsFound = GetComponentsInChildren<Button>();
                if (buttonsFound != null && buttonsFound.Length > 0)
                {
                    _okButton = buttonsFound[0];
				}
				else
				{
                    this.LogError("Button not found for Error Alert Popup");
				}
            }
        }

        public void Init(string condition, string stacktrace, LogType type)
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _title.text = type.ToString();
            _message.text = condition;
            _trace.text = stacktrace;
            _okButton.onClick.AddListener(CloseAlertPopup);
            gameObject.SetActive(true);
            _canvasGroup.DOFade(1, _animationTime).OnComplete(() => _canvasGroup.interactable = true);
        }

        public void CloseAlertPopup()
        {
            _canvasGroup.DOFade(0, _animationTime).OnComplete(() =>
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            });
        }
    }
}
