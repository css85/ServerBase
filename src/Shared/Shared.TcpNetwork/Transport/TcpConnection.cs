using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Toolkit.HighPerformance;
using Shared.Network.Base;
using Shared.TcpNetwork.Base;

namespace Shared.TcpNetwork.Transport
{
    public class TcpConnection : IConnection, IDisposable
    {
        private ITcpConnectionLogger _logger;

        private int _id;
        private ITcpSetting _settings;

        private bool _isDisposed;
        private bool _isOpen;
        private Socket _socket;
        private IPEndPoint _localEndPoint;
        private IPEndPoint _remoteEndPoint;
        private InterlockedCountFlag _issueCountFlag = default(InterlockedCountFlag);
        private int _closeReason;

        private byte[] _receiveBuffer;
        private int _receiveLength;
        private byte[] _receiveLargeBufferBytes;
        private Memory<byte>? _receiveLargeBuffer;
        private int _receiveLargeLength;
        private SocketAsyncEventArgs _receiveArgs;
        private DateTime _lastReceiveTime;

        private byte[] _sendBuffer;
        private int _sendOffset;
        private int _sendLength;
        private Memory<byte>? _sendLargeBuffer;
        private int _sendLargeOffset;
        private int _sendLargeLength;
        private SocketAsyncEventArgs _sendArgs;
        private int _sendCount;
        private ConcurrentQueue<object> _sendQueue;
        private bool _isSendShutdown;
        private string _secretKey;
        private readonly IPacketSerializer _pakcetSerilizer;

        public int Id => _id;
        public bool IsOpen => _isOpen;
        public bool IsConnect => _socket != null && _socket.Connected;
        public bool IsAuthenticated { get; set; }
        public bool Active => _issueCountFlag.Flag == false;

        public Socket Socket => _socket;

        public IPEndPoint LocalEndPoint => _localEndPoint;

        public IPEndPoint RemoteEndPoint => _remoteEndPoint;

        public DateTime LastReceiveTime => _lastReceiveTime;

        public event Action<IConnection> Opened;
        public event Action<IConnection> Authenticated; 
        public event Action<IConnection, int> Closed;
        public event Action<IConnection, object> Received;
        public event Action<IConnection, SerializeError> SerializeErrored;

        public TcpConnection(ITcpConnectionLogger logger, ITcpSetting settings, Socket socket, int id,string secretKey=null)
        {
            _logger = logger;
            _id = id;
            _settings = settings;
            _pakcetSerilizer = settings.PacketSerializer;
            socket.LingerState = new LingerOption(true, 0);
            socket.NoDelay = _settings.SocketNoDelay;

            _socket = socket;
            _localEndPoint = (IPEndPoint)_socket.LocalEndPoint;
            _remoteEndPoint = (IPEndPoint)_socket.RemoteEndPoint;
            _secretKey = secretKey;
        }

        public string SecretKey => _secretKey;

        public void SetSecretKey(string secret)
        {
            _secretKey = secret;
        }

        ~TcpConnection()
        {
            Dispose(false);
        }

        public int GetId()
        {
            return Id;
        }

        public string GetIpAddress()
        {
            var remoteIpEndPoint = _socket.RemoteEndPoint as IPEndPoint;
            return remoteIpEndPoint != null
                ? remoteIpEndPoint.Address.ToString()
                : "";
        }

        public DateTime GetLastReceiveTime()
        {
            return _lastReceiveTime;
        }

        bool IConnection.IsConnected()
        {
            return IsConnect;
        }

        public void Open()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_isOpen)
                throw new InvalidOperationException("Already Opened");

            if (_settings == null)
                throw new InvalidOperationException("Settings");

            ProcessOpen();

            _lastReceiveTime = DateTime.UtcNow;

            _isOpen = true;

            var oldContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                IssueReceive();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }
        }

        public void Close()
        {
            if (_issueCountFlag.Flag)
                return;

            if (_closeReason == 0)
                _closeReason = 1;

            _logger?.LogTrace("Close connection");

            _socket?.Close();

            if (_receiveLargeBufferBytes != null)
                ArrayPool<byte>.Shared.Return(_receiveLargeBufferBytes);

            if (_issueCountFlag.SetFlag())
                ProcessClose();
        }

        private void HandleSocketError(SocketError error)
        {
            _logger?.LogTrace($"HandleSocketError: {error}");

            if (_closeReason == 0)
                _closeReason = (int)error;

            _socket.Close();
            if (_issueCountFlag.DecrementWithSetFlag())
                ProcessClose();
        }

        private void HandleSerializeError(SerializeError error)
        {
            _logger?.LogWarning($"HandleSerializeError: {error}");

            if (SerializeErrored != null)
                SerializeErrored(this, error);

            if (_isSendShutdown == false)
                HandleSocketError(SocketError.NoData);
        }

        private void ProcessOpen()
        {
            _receiveBuffer = new byte[_settings.ReceiveBufferSize];
            _receiveLength = 0;
            _receiveArgs = new SocketAsyncEventArgs();
            _receiveArgs.SetBuffer(_receiveBuffer, 0, _receiveBuffer.Length);
            _receiveArgs.Completed += OnReceiveComplete;

            _sendBuffer = new byte[_settings.SendBufferSize];
            _sendLength = 0;
            _sendOffset = 0;
            _sendArgs = new SocketAsyncEventArgs();
            _sendArgs.SetBuffer(_sendBuffer, 0, _sendBuffer.Length);
            _sendArgs.Completed += OnSendComplete;
            _sendCount = 0;
            _sendQueue = new ConcurrentQueue<object>();

            Opened?.Invoke(this);
        }

        private void ProcessClose()
        {
            Debug.Assert(_issueCountFlag.Flag, "ProcessClose assumes it's still open");

            if (Closed != null)
                Closed(this, _closeReason);

            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }

            if (_receiveArgs != null)
            {
                _receiveArgs.Dispose();
                _receiveArgs = null;
            }

            if (_sendArgs != null)
            {
                _sendArgs.Dispose();
                _sendArgs = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
                GC.SuppressFinalize(this);
            }

            _isDisposed = true;
        }


        private void IssueReceive()
        {
            if (!_issueCountFlag.Increment())
                return;

            if (_receiveLargeBuffer == null)
            {
                _receiveArgs.SetBuffer(
                    _receiveLength,
                    _receiveBuffer.Length - _receiveLength);
            }
            else
            {
                _receiveArgs.SetBuffer(
                    0,
                    _receiveBuffer.Length);
            }

            try
            {
                if (!_socket.ReceiveAsync(_receiveArgs))
                    OnReceiveComplete(_socket, _receiveArgs);
            }
            catch (SocketException e)
            {
                HandleSocketError(e.SocketErrorCode);
            }
            catch (ObjectDisposedException)
            {
                HandleSocketError(SocketError.NotConnected);
            }
        }

        private void OnReceiveComplete(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                HandleSocketError(args.SocketError);
                return;
            }

            var length = args.BytesTransferred;
            if (length == 0)
            {
                HandleSocketError(SocketError.Shutdown);
                return;
            }

            if (_receiveLargeBuffer == null)
            {
                if (TryDeserializeNormalPacket(length) == false)
                    return;
            }
            else
            {
                if (TryDeserializeLargePacket(length) == false)
                    return;

                if (_receiveLength > 0)
                {
                    if (TryDeserializeNormalPacket(0) == false)
                        return;
                }
            }
            
            if (_issueCountFlag.Decrement())
            {
                ProcessClose();
                return;
            }

            IssueReceive();
        }

        private bool TryDeserializeNormalPacket(int len)
        {
            _receiveLength += len;

            using var stream = new MemoryStream(_receiveBuffer, 0, _receiveLength);
            var readOffset = 0;
            while (true)
            {
                var packetLength = _settings.PacketSerializer.PeekLength(stream);
                if (packetLength == 0 || _receiveLength - readOffset < packetLength)
                {
                    // 패킷 크기가 최대 크기보다 큰지 확인
                    if (packetLength > _settings.ReceiveBufferMaxSize)
                    {
                        HandleSerializeError(SerializeError.DeserializeSizeExceeded);
                        return false;
                    }

                    _receiveLength -= readOffset;

                    if (packetLength > _receiveBuffer.Length)
                    {
                        // 패킷이 기본 버퍼보다 크기가 크면 Large 모드 전환

                        _receiveLargeBufferBytes = ArrayPool<byte>.Shared.Rent(packetLength);
                        _receiveLargeBuffer = new Memory<byte>(_receiveLargeBufferBytes, 0, packetLength);
                        _receiveLargeLength = _receiveLength;
                        _receiveBuffer.AsSpan(readOffset, _receiveLength).CopyTo(_receiveLargeBuffer.Value.Span);
                    }
                    else if (_receiveLength > 0)
                    {
                        // 버퍼에 남은 데이터가 있으면 맨 앞으로 이동

                        _receiveBuffer.AsSpan(readOffset, _receiveLength)
                            .CopyTo(_receiveBuffer.AsSpan(0));
                    }

                    break;
                }

                object packet;
                try
                {
                    packet = _pakcetSerilizer.Deserialize(stream);
                }
                catch (Exception e)
                {
                    if (_logger != null)
                    {
                        var bytes = Convert.ToBase64String(_receiveBuffer, 0, _receiveLength);
                        _logger.LogWarning($"Exception raised in deserializing: {bytes}", e);
                    }

                    HandleSerializeError(SerializeError.DeserializeExceptionRaised);
                    return false;
                }

                if (packet == null)
                {
                    HandleSerializeError(SerializeError.DeserializeNoPacket);
                    return false;
                }

                _lastReceiveTime = DateTime.UtcNow;
                if (Received != null)
                    Received(this, packet);

                readOffset += packetLength;
            }

            return true;
        }

        private bool TryDeserializeLargePacket(int length)
        {
            var leftLength = _receiveLargeBuffer!.Value.Length - _receiveLargeLength;
            var copyLength = Math.Min(leftLength, length);
            _receiveBuffer.AsSpan(0, copyLength).CopyTo(_receiveLargeBuffer.Value.Slice(_receiveLargeLength).Span);
            if (leftLength > length)
            {
                _receiveLength = 0;
                _receiveLargeLength += length;
            }
            else
            {
                object packet;
                using (var stream = _receiveLargeBuffer.Value.AsStream())
                {
                    try
                    {
                        packet = _pakcetSerilizer.Deserialize(stream);
                    }
                    catch (Exception e)
                    {
                        if (_logger != null)
                        {
                            var bytes = Convert.ToBase64String(_receiveLargeBuffer.Value.ToArray());
                            _logger.LogWarning($"Exception raised in deserializing: {bytes}", e);
                        }

                        HandleSerializeError(SerializeError.DeserializeExceptionRaised);
                        return false;
                    }
                }
                if (packet == null)
                {
                    HandleSerializeError(SerializeError.DeserializeNoPacket);
                    return false;
                }

                _lastReceiveTime = DateTime.UtcNow;
                if (Received != null)
                    Received(this, packet);

                var extraLength = length - leftLength;
                if (extraLength > 0)
                {
                    Array.Copy(_receiveBuffer, leftLength,
                               _receiveBuffer, 0, extraLength);
                }
                _receiveLength = extraLength;
                ArrayPool<byte>.Shared.Return(_receiveLargeBufferBytes);
                _receiveLargeBufferBytes = null;
                _receiveLargeBuffer = null;
            }

            return true;
        }

        public void Send(object packetItem)
        {
            if (packetItem == null)
                throw new ArgumentNullException("packetItem");
            if (_isSendShutdown)
                return;

            if (Interlocked.Increment(ref _sendCount) == 1)
            {
                var oldContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(null);

                try
                {
                    StartSend(packetItem);
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(oldContext);
                }
            }
            else
            {
                _sendQueue.Enqueue(packetItem);
            }
        }
        private void StartSend(object packet)
        {
            if (!_issueCountFlag.Increment())
                return;

            var packetLength = _pakcetSerilizer.EstimateLength(packet);
            var tailSize = 0;
            if (packetLength > _sendBuffer.Length)
                tailSize = packetLength - _sendBuffer.Length;

            using var stream = new HeadTailWriteStream(_sendBuffer, tailSize);
            try
            {
                _pakcetSerilizer.Serialize(stream, packet);
            }
            catch (Exception e)
            {
                _logger?.LogWarning("Exception raised in serializing", e);
                HandleSerializeError(SerializeError.SerializeExceptionRaised);
                return;
            }

            var streamLength = (int)stream.Length;
            if (streamLength <= _sendBuffer.Length)
            {
                // 전송 버퍼안으로 보낼 수 있으므로 Normal 모드

                _sendOffset = 0;
                _sendLength = streamLength;
            }
            else
            {
                // 패킷 크기가 최대 크기보다 큰지 확인

                if (streamLength > _settings.SendBufferMaxSize)
                {
                    HandleSerializeError(SerializeError.SerializeSizeExceeded);
                    return;
                }

                // 전송 버퍼를 넘어서는 크기이므로 Large 모드로 전환

                _sendOffset = 0;
                _sendLength = _sendBuffer.Length;

                _sendLargeBuffer = stream.Tail;
                _sendLargeOffset = 0;
                _sendLargeLength = streamLength - _sendBuffer.Length;

                Debug.Assert(_sendLargeBuffer != null && _sendLargeLength > 0, "It should be valid");
            }

            IssueSend();
        }


        private void IssueSend()
        {
            _sendArgs.SetBuffer(_sendOffset, _sendLength - _sendOffset);
            try
            {
                if (!_socket.SendAsync(_sendArgs))
                    OnSendComplete(_socket, _sendArgs);
            }
            catch (SocketException e)
            {
                HandleSocketError(e.SocketErrorCode);
            }
            catch (ObjectDisposedException)
            {
                HandleSocketError(SocketError.NotConnected);
            }
        }

        private void OnSendComplete(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                HandleSocketError(args.SocketError);
                return;
            }

            if (_issueCountFlag.Decrement())
            {
                ProcessClose();
                return;
            }

            _sendOffset += args.BytesTransferred;
            if (_sendOffset < _sendLength)
            {
                // Send 버퍼를 덜 보냈으면 나머지를 다시 전송 요청

                if (!_issueCountFlag.Increment())
                    return;

                IssueSend();
            }
            else if (_sendLargeBuffer != null)
            {
                // Large 모드일 때 Large 버퍼의 내용을 Send 버퍼에 복사해 전송 요청

                if (!_issueCountFlag.Increment())
                    return;

                int len = Math.Min(_sendLargeLength - _sendLargeOffset, _sendBuffer.Length);
                Debug.Assert(len > 0, "It should be large enough");

                _sendLargeBuffer.Value.Slice(_sendLargeOffset, len).CopyTo(_sendBuffer);
                _sendLargeOffset += len;
                if (_sendLargeOffset == _sendLargeLength)
                    _sendLargeBuffer = null;

                _sendLength = len;
                _sendOffset = 0;

                IssueSend();
            }
            else
            {
                // 전송 패킷 큐에 대기중인 패킷이 있으면 꺼내서 전송

                if (Interlocked.Decrement(ref _sendCount) > 0)
                {
                    object packet;
                    while (_sendQueue.TryDequeue(out packet) == false)
                    {
                    }
                    if (packet != null)
                    {
                        StartSend(packet);
                    }
                    else
                    {
                        Close();
                    }
                }
            }
        }

        public void FlushAndClose()
        {
            if (Active == false || _isSendShutdown)
                return;

            Volatile.Write(ref _isSendShutdown, true);

            if (Interlocked.Increment(ref _sendCount) == 1)
            {
                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                }
                catch (ObjectDisposedException)
                {
                }
            }
            else
            {
                _sendQueue.Enqueue(null);
            }
        }
    }
}
