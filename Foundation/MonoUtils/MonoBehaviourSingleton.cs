using UnityEngine;

namespace Foundation.MonoUtils
{
    /// <summary>
    /// Inherit from this base class to create a singleton.
    /// e.g. public class MyClassName : Singleton<MyClassName> {}
    /// </summary>
    public class MonoBehaviourSingleton<T> : BaseMonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        /// <summary>
        /// Access singleton instance through this propriety.
        /// </summary>
        public static T Instance => _instance;
        
        protected virtual void Awake()
        {
            // Initialize singleton or destroy if already exists
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}