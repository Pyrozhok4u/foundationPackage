using System;
using System.Collections.Generic;
using Foundation.ServicesResolver;
using Google.Protobuf;

namespace Foundation.Network
{
    public delegate void OnConnectionStatusChangeHandler(SocketConnectionStatus status);
    public interface ISocketService : IService
    {
        event OnConnectionStatusChangeHandler OnConnectionStatusChange;

        void Connect(string uri = null, Dictionary<string, string> query = null);
        void Emit<T>(string eventName, IMessage message, Action<APIResponse<T>> callback = null)
            where T : IMessage<T>, new();

        void Register<T>(string eventName, Action<APIResponse<T>> callback) where T : IMessage<T>, new();
        void Unregister(string eventName);
        void Send(byte[] message);
        void RemoveCallback(ISocketRequest request);
    }
}
