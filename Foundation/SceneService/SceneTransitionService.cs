using System;
using DG.Tweening.Core;
using Foundation.ServicesResolver;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Foundation.SceneService
{
	public class SceneTransitionService : BaseService, ISceneTransitionService
	{
		public event SceneTransitionEvent OnSceneTransitionStarted,
			OnSceneLoadingStarted,
			OnSceneLoadingCompleted,
			OnSceneTransitionCompleted;

		private SceneTransition _currentTransition;
		
		#region BaseService

		public override void Dispose()
		{
			_currentTransition = null;
			OnSceneTransitionStarted =
				OnSceneLoadingStarted =
					OnSceneLoadingCompleted =
						OnSceneTransitionCompleted = null;
		}

		protected override void Initialize() { }
		#endregion

		#region API

		public void LoadScene<T>(T scene, SceneTransition transition, SceneTransitionEvent onSceneTransitionCompleted = null) where T : Enum
		{
			LoadScene((int)(object)scene, transition, onSceneTransitionCompleted);
		}

		public void LoadScene(int sceneIndex, SceneTransition transition, SceneTransitionEvent onSceneTransitionCompleted = null)
		{
			if (transition == null)
			{
				Debugger.LogError("Transition is null. Scene change failed!");
				return;
			}
			_currentTransition = transition;
			OnSceneTransitionStarted?.Invoke(sceneIndex);
			_currentTransition.StartEnterTransition(() =>
			{
				OnSceneLoadingStarted?.Invoke(sceneIndex);
				var load = SceneManager.LoadSceneAsync(sceneIndex);
				//load.completed += SceneLoadCompleted;
				load.completed += delegate { SceneLoadCompleted(onSceneTransitionCompleted); };
			});

		}

		#endregion

		#region Internal methods
		private void SceneLoadCompleted(SceneTransitionEvent onSceneTransitionCompleted = null)
		{
			int sceneIndex = SceneManager.GetActiveScene().buildIndex;

			OnSceneLoadingCompleted?.Invoke(sceneIndex);
			_currentTransition.StartExitTransition(() =>
			{
				// Invoke direct callback
				onSceneTransitionCompleted?.Invoke(sceneIndex);
				// Invoke general registration for the event
				OnSceneTransitionCompleted?.Invoke(SceneManager.GetActiveScene().buildIndex);
			});
		}
		#endregion
	}
}
