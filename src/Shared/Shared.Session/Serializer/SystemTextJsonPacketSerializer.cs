using System;
using System.IO;
using System.Text.Json;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Data;
using Shared.Packet.Extension;
using Shared.Packet.Utility;
using Shared.Server.Extensions;
using Shared.TcpNetwork.Base;

namespace Shared.Session.Serializer
{
    public class SystemTextJsonPacketSerializer : IPacketSerializer
    {
        private readonly JsonSerializerOptions _options;
        private readonly int MAX_STACK_SIZE = 1048576;

        public SystemTextJsonPacketSerializer(JsonSerializerOptions options)
        {
            _options = SystemTextJsonSerializationOptions.Default;
        }

        
        public int EstimateLength(object packet)
        {
            var packetItem = (IPacketItem)packet;
            var bytes = new ReadOnlySpan<byte>(packetItem.GetDataBytes());
            var packetData = packetItem.GetData();
            switch (packetItem.Type())
            {
                case PacketType.Request:
                {
                    if(packetData==null && bytes.IsEmpty)
                        return Const.REQ_HEADER_SIZE;
                        
                    return Const.REQ_HEADER_SIZE + bytes.Length;
                }
                case PacketType.Response:
                {
                    if (packetData == null && bytes.IsEmpty)
                        return Const.RES_HEADER_SIZE;
                        
                    return Const.RES_HEADER_SIZE + bytes.Length;
                }
                case PacketType.Ntf:
                {
                    if (packetData == null && bytes.IsEmpty)
                        return Const.NTF_HEADER_SIZE;
                        
                    return Const.NTF_HEADER_SIZE + bytes.Length;
                }
                default:
                    return 0;
            }
        }

        // Type = Request
        // +--------+--------+--+----------+-----------+-------------+-----------+------------+-------+
        // | Type(1) | LEN(4) | MAJOR(1) | MINOR(1) | REQUEST_ID (2) | M_DATA (~) |
        // +--------+--------+--+----------+-----------+-------------+-----------+------------+-------+

        // Type = Response
        // +--------+--------+--+----------+-----------+-------------+-----------+------------+-------+
        // | Type(1) | LEN(4) | MAJOR(1) | MINOR(1) | REQUEST_ID (2) | RESULT (4) | M_DATA (~) |
        // +--------+--------+--+----------+-----------+-------------+-----------+------------+-------+

        // Type = Ntf
        // +--------+--------+--+----------+-----------+-------------+-----------+------------+-------+
        // | Type(1) | LEN(4) | MAJOR(1) | MINOR(1) | M_DATA (~) |
        // +--------+--------+--+----------+-----------+-------------+-----------+------------+-------+

        public void Serialize(Stream stream, object packet)
        {
            var packetItem = (IPacketItem) packet;
            var packetType = packetItem.Type();
            var header = packetItem.Header();

            // Write Packet Type
            stream.WriteByte((byte) packetType);

            switch (packetType)
            {
                case PacketType.Request:
                {
                    // Write Length
                    stream.Write32BitEncodedInt(Const.REQ_HEADER_SIZE + (packetItem.GetDataBytes()?.Length ?? 0));

                    // Write Header
                    stream.WriteByte(header.Major);
                    stream.WriteByte(header.Minor);
                    stream.Write16BitEncodedUshort(packetItem.GetRequestId());

                    break;
                }
                case PacketType.Response:
                {
                    // Write Length
                    stream.Write32BitEncodedInt(Const.RES_HEADER_SIZE + (packetItem.GetDataBytes()?.Length ?? 0));

                    // Write Header
                    stream.WriteByte(header.Major);
                    stream.WriteByte(header.Minor);
                    stream.Write16BitEncodedUshort(packetItem.GetRequestId());
                    stream.Write32BitEncodedInt(packetItem.GetResult());

                    break;
                }
                case PacketType.Ntf:
                {
                    // Write Length
                    stream.Write32BitEncodedInt(Const.NTF_HEADER_SIZE + (packetItem.GetDataBytes()?.Length ?? 0));

                    // Write Header
                    stream.WriteByte(header.Major);
                    stream.WriteByte(header.Minor);

                    break;
                }
            }

            // Write Data
            if (packetItem.GetDataBytes() != null)
                stream.Write(packetItem.GetDataBytes());

            stream.Flush();
        }

        public byte[] Serialize(object packet)
        {
            if(packet is not IPacketItem packetItem)
                return Array.Empty<byte>();

            var data = packetItem.GetData();
            if(data == null)
                return Array.Empty<byte>();

            return JsonSerializer.SerializeToUtf8Bytes(data, packetItem.DataType(), _options);
        }

        public int PeekLength(Stream stream)
        {
            var length = (int)(stream.Length - stream.Position);
            if (length < 5)
                return 0;

            // Peek Length
            Span<byte> bytes = stackalloc byte[5];
            stream.Read(bytes);
            stream.Seek(-5, SeekOrigin.Current);

            return BitConverter.ToInt32(bytes.Slice(1, 4));
        }

        private RequestPacketItem DeserializePacketBySize(Stream stream, Type bodyType, int bodySize, UInt16 requestId)
        {
            if (bodySize > 0)
            {
                var packetHeaderData = PacketHeaderTable.GetHeaderData(bodyType);
                if (bodySize >= MAX_STACK_SIZE)
                {
                    byte[] largeBytes = new byte[bodySize];
                    stream.Read(largeBytes);
                    var reader = new Utf8JsonReader(largeBytes);
                    var data = JsonSerializer.Deserialize(ref reader, bodyType, _options) as IPacketData;
                    return new RequestPacketItem(packetHeaderData, data, requestId);
                }
                else
                {
                    Span<byte> bytes = stackalloc byte[bodySize];
                    stream.Read(bytes);
                    var reader = new Utf8JsonReader(bytes);
                    var data = JsonSerializer.Deserialize(ref reader, bodyType, _options) as IPacketData;
                    return new RequestPacketItem(packetHeaderData, data, requestId);
                }
            }
            else
            {
                var packetHeaderData = PacketHeaderTable.GetHeaderData(bodyType);
                return new RequestPacketItem(packetHeaderData, (IPacketData)null, requestId);
            }
        }

        public object Deserialize(Stream stream)
        {
            var packetType = (PacketType) stream.ReadByte();
            var packetSize = stream.Read32BitEncodedInt();
            var major = (byte) stream.ReadByte();
            var minor = (byte) stream.ReadByte();

            switch (packetType)
            {
                case PacketType.Request:
                {
                    var bodyType = PacketHeaderTable.GetReqType(major, minor);
                    var requestId = stream.Read16BitEncodedUshort();

                    var bodySize = packetSize - Const.REQ_HEADER_SIZE;
                    return DeserializePacketBySize(stream, bodyType, bodySize, requestId);
                }
                case PacketType.Response:
                {
                    var bodyType = PacketHeaderTable.GetResType(major, minor);
                    var requestId = stream.Read16BitEncodedUshort();
                    var result = stream.Read32BitEncodedInt();

                    var bodySize = packetSize - Const.RES_HEADER_SIZE;
                    if (bodySize > 0)
                    {
                        var packetHeaderData = PacketHeaderTable.GetHeaderData(bodyType);
                        Span<byte> bytes = stackalloc byte[bodySize];
                        stream.Read(bytes);
                        var reader = new Utf8JsonReader(bytes);
                        var data = JsonSerializer.Deserialize(ref reader, bodyType, _options) as IPacketData;
                        return new ResponsePacketItem(packetHeaderData, data, result, requestId);
                    }
                    else
                    {
                        var packetHeaderData = PacketHeaderTable.GetHeaderData(bodyType);
                        return new ResponsePacketItem(packetHeaderData, (IPacketData)null, result, requestId);
                    }
                }
                case PacketType.Ntf:
                {
                    var bodyType = PacketHeaderTable.GetNtfType(major, minor);
                    var bodySize = packetSize - Const.NTF_HEADER_SIZE;
                    if (bodySize > 0)
                    {
                        var packetHeaderData = PacketHeaderTable.GetHeaderData(bodyType);
                        Span<byte> bytes = stackalloc byte[bodySize];
                        stream.Read(bytes);
                        var reader = new Utf8JsonReader(bytes);
                        var data = JsonSerializer.Deserialize(ref reader, bodyType, _options) as IPacketData;
                        return new NtfPacketItem(packetHeaderData, data);
                    }
                    else
                    {
                        var packetHeaderData = PacketHeaderTable.GetHeaderData(bodyType);
                        return new NtfPacketItem(packetHeaderData, Array.Empty<byte>());
                    }
                }
                default:
                    return null;
            }
        }
    }
}
