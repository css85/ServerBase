using System;

namespace Shared.Network.Base
{
    public enum SerializeError
    {
        None,
        SerializeSizeExceeded,
        SerializeExceptionRaised,
        DeserializeSizeExceeded,
        DeserializeExceptionRaised,
        DeserializeNoPacket,
    }

    public interface IConnection
    {
        event Action<IConnection> Opened;
        event Action<IConnection, int> Closed;
        event Action<IConnection, object> Received;
        event Action<IConnection, SerializeError> SerializeErrored;
        event Action<IConnection> Authenticated; 
        int GetId();
        string GetIpAddress();
        DateTime GetLastReceiveTime();
        bool IsConnected();
        void Send(object packet);
        void Close();
        bool IsAuthenticated { get; set;}
        string SecretKey { get; }
        void SetSecretKey(string secret);
    }
}