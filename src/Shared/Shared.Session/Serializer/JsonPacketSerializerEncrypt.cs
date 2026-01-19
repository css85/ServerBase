using Shared.Packet;
using Shared.TcpNetwork.Base;
using System;
using System.Text.Json;
using Shared.Packet.Extension;
using Shared.Packet.Data;
using Shared.Packet.Utility;
using System.IO;
using SampleGame.Shared.Common;

namespace Shared.Session.Serializer
{
    public class JsonPacketSerializerEncrypt : IPacketSerializer
    {
        private readonly int MAX_STACK_SIZE = 1048576;

        public JsonPacketSerializerEncrypt(JsonSerializerOptions options)
        {
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
                        if (packetData ==null && bytes.IsEmpty)
                            return Const.REQ_HEADER_SIZE;
                        
                        return Const.REQ_HEADER_SIZE + bytes.Length;
                    }
                case PacketType.Response:
                    {
                        if (packetData ==null &&  bytes.IsEmpty)
                            return Const.RES_HEADER_SIZE;
                        
                        return Const.RES_HEADER_SIZE + bytes.Length;
                    }
                case PacketType.Ntf:
                    {
                        if (packetData ==null && bytes.IsEmpty)
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
            var packetItem = (IPacketItem)packet;
            var packetType = packetItem.Type();
            var header = packetItem.Header();

            var bodySize = (packetItem.GetDataBytes()?.Length ?? 0);
            // Write Packet Type
            stream.WriteByte((byte)packetType);

            switch (packetType)
            {
                case PacketType.Request:
                    {
                        // Write Length
                        stream.Write32BitEncodedInt(Const.REQ_HEADER_SIZE + bodySize);

                        // Write Header
                        stream.WriteByte(header.Major);
                        stream.WriteByte(header.Minor);
                        stream.Write16BitEncodedUshort(packetItem.GetRequestId());

                        break;
                    }
                case PacketType.Response:
                    {
                        // Write Length
                        stream.Write32BitEncodedInt(Const.RES_HEADER_SIZE + bodySize);

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
                        stream.Write32BitEncodedInt(Const.NTF_HEADER_SIZE + bodySize);

                        // Write Header
                        stream.WriteByte(header.Major);
                        stream.WriteByte(header.Minor);

                        break;
                    }
            }

            if (bodySize > 0)
            {
                var bytes = packetItem.GetDataBytes();
                stream.Write(bytes);
            }

            stream.Flush();
        }

        public byte[] Serialize(object packet)
        {
            if(packet is not IPacketItem packetItem)
                return Array.Empty<byte>();

            var data = packetItem.GetData();
            if(data == null)
                return Array.Empty<byte>();

            return JsonTextSerializer.SerializeUtf8Bytes(data, packetItem.DataType());
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
        
        public object Deserialize(Stream stream)
        {
            var packetType = (PacketType)stream.ReadByte();
            var packetSize = stream.Read32BitEncodedInt();
            var major = (byte)stream.ReadByte();
            var minor = (byte)stream.ReadByte();

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
                            byte[] bytes = new byte[bodySize];
                            stream.Read(bytes);
                            return new ResponsePacketItem(packetHeaderData, bytes, result, requestId);
                        }
                        else
                        {
                            var packetHeaderData = PacketHeaderTable.GetHeaderData(bodyType);
                            return new ResponsePacketItem(packetHeaderData, (byte[])null, result, requestId);
                        }
                    }
                case PacketType.Ntf:
                    {
                        var bodyType = PacketHeaderTable.GetNtfType(major, minor);
                        var bodySize = packetSize - Const.NTF_HEADER_SIZE;
                        if (bodySize > 0)
                        {
                            var packetHeaderData = PacketHeaderTable.GetHeaderData(bodyType);
                            byte[] bytes = new byte[bodySize];
                            stream.Read(bytes);
                            return new NtfPacketItem(packetHeaderData, bytes);
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

        private RequestPacketItem DeserializePacketBySize(Stream stream, Type bodyType, int bodySize, UInt16 requestId)
        {
            if (bodySize > 0)
            {
                var packetHeaderData = PacketHeaderTable.GetHeaderData(bodyType);
                if (bodySize >= MAX_STACK_SIZE)
                {
                    byte[] largeBytes = new byte[bodySize];
                    stream.Read(largeBytes);
                    return new RequestPacketItem(packetHeaderData, largeBytes, requestId);
                }
                else
                {
                    byte[] bytes = new byte[bodySize];
                    stream.Read(bytes);
                    return new RequestPacketItem(packetHeaderData, bytes, requestId);
                }
            }
            else
            {
                var packetHeaderData = PacketHeaderTable.GetHeaderData(bodyType);
                return new RequestPacketItem(packetHeaderData, (byte[])null, requestId);
            }
        }
    }
}
