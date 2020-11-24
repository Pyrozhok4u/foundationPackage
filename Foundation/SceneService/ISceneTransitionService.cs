using System;
using Foundation.ServicesResolver;

namespace Foundation.SceneService
{
    public delegate void SceneTransitionEvent(int sceneIndex);

    public interface ISceneTransitionService : IService
    {
        event SceneTransitionEvent OnSceneTransitionStarted;
        event SceneTransitionEvent OnSceneLoadingStarted;
        event SceneTransitionEvent OnSceneLoadingCompleted;
        event SceneTransitionEvent OnSceneTransitionCompleted;

        void LoadScene<T>(T scene, SceneTransition transition, SceneTransitionEvent onSceneTransitionCompleted = null) where T : Enum;
        void LoadScene(int sceneIndex, SceneTransition transition, SceneTransitionEvent onSceneTransitionCompleted = null);
    }
}
