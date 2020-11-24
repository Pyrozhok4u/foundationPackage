using System;
using DG.Tweening;
using UnityEngine;

namespace Foundation.SceneService
{
	[RequireComponent(typeof(CanvasGroup))]
	public class SceneTransitionFade : SceneTransition
	{
		[Range(0, 5)]
		[SerializeField] private float _animationTime;
		[SerializeField] private CanvasGroup _canvasGroup;

		private void OnValidate()
		{
			if (_canvasGroup == null)
			{
				_canvasGroup = GetComponent<CanvasGroup>();
			}
		}

		public override void StartEnterTransition(Action onComplete = null)
		{
			Transition(true, onComplete);
		}

		public override void StartExitTransition(Action onComplete = null)
		{
			Transition(false, onComplete);
		}

		private void Transition(bool isTransitionIn, Action onComplete = null)
		{
			_canvasGroup.DOKill(true);
			_canvasGroup.DOFade(isTransitionIn ? 1 : 0, _animationTime).OnComplete(() => onComplete?.Invoke());
		}


	}
}
