using Foundation.Utils.SerializerUtils;
using Google.Protobuf;

namespace Foundation.Network
{
    internal class ProtoSerializer : ISerializer
    {
        public string EncodeJson<T>(T value) where T : class
        {
            return NetworkUtils.JsonFormatter.Format(value as IMessage);
        }

        public byte[] EncodeBytes<T>(T value) where T : class
        {
            return (value as IMessage).ToByteArray();
        }

        public T DecodeJson<T>(string value) where T : class, new()
        {
            return (T)((IMessage) new T()).Descriptor.Parser.ParseJson(value);
        }

        public T DecodeBytes<T>(byte[] value) where T : class, new()
        {
            return (T)((IMessage) new T()).Descriptor.Parser.ParseFrom(value);
        }
    }
}