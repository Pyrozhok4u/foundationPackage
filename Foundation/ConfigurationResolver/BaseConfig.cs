using Foundation.Utils.SerializerUtils;
using UnityEngine;

namespace Foundation.ConfigurationResolver
{
    public abstract class BaseConfig : ScriptableObject
    {
        public PlatformType SupportedPlatform = PlatformType.All;
        public bool IsEnabled = true;
        
        /// <summary>
        /// Use to over-write config with remote configurations
        /// </summary>
        /// <param name="json"></param>
        public void LoadFromJson(ISerializer serializer, string json)
        {
            if (serializer is UnitySerializer unitySerializer)
            {
                unitySerializer.DeserializeOverwrite(json, this);
            }
        }
    }
}

