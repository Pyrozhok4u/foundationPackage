using System;
using DG.Tweening;
using UnityEngine;

namespace Foundation.Popups.Transitions
{
    public class AnimatorTransition : Transition
    {
        [SerializeField] private Animator _animator;
        private const string InKey = "In";
        private const string OutKey = "Out";

        private Action _onInCompleted, _onOutCompleted;
        private static readonly int InIntKey = Animator.StringToHash(InKey);
        private static readonly int OutIntKey = Animator.StringToHash(OutKey);

        public override void In(Action onCompleted)
        {
            gameObject.SetActive(true);
            _onInCompleted = onCompleted;
            _animator.SetTrigger(InIntKey);
            float duration = _animator.GetCurrentAnimatorStateInfo(0).length;
            DOVirtual.DelayedCall(duration, OnInComplete);
        }

        public override void Out(Action onCompleted)
        {
            _onOutCompleted = onCompleted;
            _animator.SetTrigger(OutIntKey);
            float duration = _animator.GetCurrentAnimatorStateInfo(0).length;
            DOVirtual.DelayedCall(duration, OnOutComplete);
        }

        public void OnInComplete()
        {
            _onInCompleted?.Invoke();
        }

        public void OnOutComplete()
        {
            gameObject.SetActive(false);
            _onOutCompleted?.Invoke();
        }
    }
}
