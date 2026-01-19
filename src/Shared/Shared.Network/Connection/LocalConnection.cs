using System;
using Shared.Network.Base;

namespace Shared.Network.Connection
{
    public class LocalConnection : IConnection
    {
        private readonly int _id;
        public event Action<IConnection> Opened;
        public event Action<IConnection, int> Closed;
        public event Action<IConnection, object> Received;
        public event Action<IConnection, SerializeError> SerializeErrored;
        public event Action<IConnection> Authenticated;

        private Action<object> _sendTargetMethod;
        public bool IsAuthenticated { get; set; }
        public string SecretKey { get; }
        public void SetSecretKey(string secret)
        {
            throw new NotImplementedException();
        }
        
        public LocalConnection(int id)
        {
            _id = id;
        }

        public void Open(Action<object> sendTargetMethod)
        {
            _sendTargetMethod = sendTargetMethod;
        }

        public int GetId()
        {
            return _id;
        }

        public string GetIpAddress()
        {
            return "127.0.0.1";
        }

        public DateTime GetLastReceiveTime()
        {
            return DateTime.UtcNow;
        }

        public bool IsConnected()
        {
            return true;
        }

        public void Send(object packet)
        {
            _sendTargetMethod.Invoke(packet);
        }

        public void Close()
        {
        }
    }
}