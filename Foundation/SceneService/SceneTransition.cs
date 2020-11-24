using System;
using Foundation.MonoUtils;

namespace Foundation.SceneService
{
	public abstract class SceneTransition : BaseMonoBehaviour
	{
		public abstract void StartEnterTransition(Action onComplete);
		public abstract void StartExitTransition(Action onComplete);
	}
}
