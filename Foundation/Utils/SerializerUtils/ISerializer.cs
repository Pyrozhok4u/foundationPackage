namespace Foundation.Utils.SerializerUtils
{
    public interface ISerializer
    {
        string EncodeJson<T>(T value) where T : class;
        byte[] EncodeBytes<T>(T value) where T : class;

        T DecodeJson<T>(string value) where T : class, new();
        T DecodeBytes<T>(byte[] value) where T : class, new();
        
    }
}
