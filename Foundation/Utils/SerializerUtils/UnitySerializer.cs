using System;
using Foundation.ServicesResolver;
using UnityEngine;

namespace Foundation.Utils.SerializerUtils
{
    public class UnitySerializer : BaseService, ISerializer
    {
        protected override void Initialize() { }
        
        public override void Dispose() { }

        public string EncodeJson<T>(T value) where T : class
        {
            return JsonUtility.ToJson(value);
        }

        public byte[] EncodeBytes<T>(T value) where T : class
        {
            return Convert.FromBase64String(EncodeJson<T>(value));
        }

        public T DecodeJson<T>(string value) where T : class, new()
        {
            return JsonUtility.FromJson<T>(value);
        }

        public T DecodeBytes<T>(byte[] value) where T : class, new()
        {
            return DecodeJson<T>(Convert.ToBase64String(value));
        }

        public void DeserializeOverwrite<T>(string value, T obj) where T : class
        {
            JsonUtility.FromJsonOverwrite(value, obj);
        }

    }
}
