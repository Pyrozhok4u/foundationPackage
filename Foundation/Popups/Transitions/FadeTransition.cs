using System;
using DG.Tweening;
using UnityEngine;

namespace Foundation.Popups.Transitions
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeTransition : Transition
    {
        [SerializeField] private float _animateTime = 1f;
        [SerializeField] private CanvasGroup _canvasGroup;

        private void OnValidate()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.GetComponent<CanvasGroup>();
            }
        }

        public override void In(Action onCompleted)
        {
            gameObject.SetActive(true);
            _canvasGroup.DOKill();
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.DOFade(1, _animateTime).OnComplete(() =>
            {
                onCompleted?.Invoke();
                _canvasGroup.interactable = true;
            });
        }

        public override void Out(Action onCompleated)
        {
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(0, _animateTime).OnStart(() =>
            {
                _canvasGroup.interactable = false;
            }).OnComplete(() =>
            {
                onCompleated?.Invoke();
                gameObject.SetActive(false);
            });
        }
    }
}
