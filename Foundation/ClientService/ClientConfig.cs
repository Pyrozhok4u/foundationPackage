using UnityEngine;

namespace Foundation.ClientService
{
    [CreateAssetMenu(fileName = "ClientConfig", menuName = "Foundation/Configurations/Create Client Config", order = 1)]
    public class ClientConfig : ScriptableObject
    {
        public string Version;

        public static ClientConfig LoadConfig()
        {
            string path = typeof(ClientConfig).Name;
            return Resources.Load<ClientConfig>(path);
        }
    }
}
