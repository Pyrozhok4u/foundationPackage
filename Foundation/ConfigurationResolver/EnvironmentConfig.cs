using UnityEngine;

namespace Foundation.ConfigurationResolver
{
    public class EnvironmentConfig : ScriptableObject
    {
        public enum Environment {Dev, Stage, Prod};

        [SerializeField] private Environment _environment = Environment.Dev;

        public Environment GetEnvironment()
        {
            return _environment;
        }

        public void SetEnvironment(Environment environment)
        {
            _environment = environment;
        }

        public static EnvironmentConfig LoadConfig()
        {
            string path =  typeof(EnvironmentConfig).Name;
            return Resources.Load<EnvironmentConfig>(path);
        }
    }
}
