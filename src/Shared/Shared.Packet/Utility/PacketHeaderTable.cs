using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shared.Model;

namespace Shared.Packet.Utility
{
    public class PacketHeaderData
    {
        public PacketType PacketType { get; set; }
        public PacketHeader Header { get; set; }
        public Type DataType { get; set; }
        public NetServiceTypeInfo ServiceType { get; set; }
        public string RequestUri { get; set; }
        public RequestMethodType HttpMethod { get; set; }
    }

    public static class PacketHeaderTable
    {
        private static Dictionary<int, Type> _headerToRequestTypeMap = new Dictionary<int, Type>();
        private static Dictionary<int, Type> _headerToResponseTypeMap = new Dictionary<int, Type>();
        private static Dictionary<int, Type> _headerToNtfTypeMap = new Dictionary<int, Type>();
        private static Dictionary<int, Type> _headerToTotalResponseTypeMap = new Dictionary<int, Type>();

        private static Dictionary<Type, PacketHeader> _typeToHeaderMap = new Dictionary<Type, PacketHeader>();

        private static Dictionary<Type, string> _typeToRequestUri = new Dictionary<Type, string>();
        private static Dictionary<string, Type> _requestUriToType = new Dictionary<string, Type>();

        private static Dictionary<Type, PacketHeaderData> _headerDataMap = new Dictionary<Type, PacketHeaderData>();

        private static List<Type> GetPacketTypes<T, TA>(this List<Type> types)
           where T : class
           where TA : Attribute
            {
                return types
                    .Where(p => p.GetCustomAttribute<TA>() != null)
                    .Where(p => p.BaseType == typeof(T))
                    .ToList();
            }

        public static void Build(string[] assemblyNames)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var types = assemblies
                .Where(p => assemblyNames.Contains(p.GetName().Name))
                .SelectMany(p => p.GetTypes()).ToList();

            var requestHeaderTypes =
                types.GetPacketTypes<RequestBase, RequestClassAttribute>()
                    .Select(p =>
                    {
                        var attribute = p.GetCustomAttribute<RequestClassAttribute>();
                        return new PacketHeaderData
                        {
                            PacketType = PacketType.Request,
                            DataType = p,
                            Header = new PacketHeader(attribute.Major, attribute.Minor),
                            RequestUri = attribute.HttpPath,
                            HttpMethod = attribute.HttpMethodType,
                            ServiceType = new NetServiceTypeInfo(attribute.ProtocolType, attribute.ServiceType)
                        };
                    })
                    .ToList();

            requestHeaderTypes.AddRange(
                types.GetPacketTypes<RequestBase, MultipleRequestClassAttribute>()
                    .Select(p =>
                    {
                        var attribute = p.GetCustomAttribute<MultipleRequestClassAttribute>();
                        return new PacketHeaderData
                        {
                            PacketType = PacketType.Request,
                            DataType = p,
                            Header = new PacketHeader(attribute.Major, attribute.Minor),
                        };
                    }));

            var responsePacketHeaderTypes =
                types.GetPacketTypes<ResponseBase, ResponseClassAttribute>()
                    .Select(p =>
                    {
                        var attribute = p.GetCustomAttribute<ResponseClassAttribute>();
                        return new PacketHeaderData
                        {
                            PacketType = PacketType.Response,
                            DataType = p,
                            Header = new PacketHeader(attribute.Major, attribute.Minor),
                            RequestUri = attribute.HttpPath,
                            HttpMethod = attribute.HttpMethodType,
                            ServiceType = new NetServiceTypeInfo(attribute.ProtocolType, attribute.ServiceType)
                        };
                    })
                    .ToList();

            responsePacketHeaderTypes.AddRange(
                types.GetPacketTypes<ResponseBase, MultipleResponseClassAttribute>()
                    .Select(p =>
                    {
                        var attribute = p.GetCustomAttribute<MultipleResponseClassAttribute>();
                        return new PacketHeaderData
                        {
                            PacketType = PacketType.Response,
                            DataType = p,
                            Header = new PacketHeader(attribute.Major, attribute.Minor)
                        };
                    }));

            responsePacketHeaderTypes.AddRange(
                requestHeaderTypes
                    .Select(p =>
                    {
                        var typeName = p.DataType.Name.Substring(0, p.DataType.Name.Length - 3) + "Res";
                        var name = $"{p.DataType.Namespace}.{typeName},{p.DataType.AssemblyQualifiedName.Split(',')[1]}";
                        return new PacketHeaderData
                        {
                            PacketType = PacketType.Response,
                            DataType = Type.GetType(name),
                            Header = p.Header,
                        };
                    })
                    .Where(p => p.DataType != null));
                    

            var ntfPacketHeaderTypes =
                types.GetPacketTypes<NtfBase, NtfClassAttribute>()
                    .Select(p =>
                    {
                        var attribute = p.GetCustomAttribute<NtfClassAttribute>();
                        return new PacketHeaderData
                        {
                            PacketType = PacketType.Ntf,
                            DataType = p,
                            Header = new PacketHeader(attribute.Major, attribute.Minor),
                            ServiceType = new NetServiceTypeInfo(attribute.ProtocolType, attribute.ServiceType)
                        };
                    })
                    .ToList();

            ntfPacketHeaderTypes.AddRange(
                types.GetPacketTypes<NtfBase, MultipleNtfClassAttribute>()
                    .Select(p =>
                    {
                        var attribute = p.GetCustomAttribute<MultipleNtfClassAttribute>();
                        return new PacketHeaderData
                        {
                            PacketType = PacketType.Ntf,
                            DataType = p,
                            Header = new PacketHeader(attribute.Major, attribute.Minor),
                        };
                    }));

            _headerToRequestTypeMap =
                requestHeaderTypes.ToDictionary(p => { return (p.Header.Major * 1000) + p.Header.Minor; }, p => p.DataType);

            var conflictHeaders = responsePacketHeaderTypes
                .GroupBy(x => (x.Header.Major * 1000) + (x.Header.Minor))
                .Where(group => group.Count() > 1)
                .SelectMany((group, header) => group.Select(x => x))
                .ToList();

            if (conflictHeaders.Count > 0)
            {
                throw new NotImplementedException($"{string.Join(",", conflictHeaders.Select(x => x.DataType))}");
            }

            _headerToResponseTypeMap =
                responsePacketHeaderTypes.ToDictionary(p => { return (p.Header.Major * 1000) + p.Header.Minor; },
                    p => p.DataType);

            _headerToNtfTypeMap =
                ntfPacketHeaderTypes.ToDictionary(p => { return (p.Header.Major * 1000) + p.Header.Minor; },
                    p => p.DataType);

            var totalResponseTypes = responsePacketHeaderTypes.ToList();
            totalResponseTypes.AddRange(ntfPacketHeaderTypes);
            _headerToTotalResponseTypeMap = totalResponseTypes.ToDictionary(p => { return (p.Header.Major * 1000) + p.Header.Minor; },
                p => p.DataType);

            _typeToRequestUri = requestHeaderTypes
                .Where(x => !string.IsNullOrWhiteSpace(x.RequestUri))
                .ToDictionary(k => k.DataType, v => v.RequestUri);

            _requestUriToType = requestHeaderTypes
                .Where(x => !string.IsNullOrWhiteSpace(x.RequestUri))
                .ToDictionary(k => k.RequestUri, v => v.DataType);

            var headerTypes = requestHeaderTypes.ToList();
            headerTypes.AddRange(responsePacketHeaderTypes);
            headerTypes.AddRange(ntfPacketHeaderTypes);
            _typeToHeaderMap = headerTypes.ToDictionary(p => p.DataType, p => p.Header);

            _headerDataMap = headerTypes.ToDictionary(p => p.DataType,p=>p);
        }

        public static PacketHeaderData GetHeaderData(Type packetType)
        {
            return _headerDataMap.TryGetValue(packetType, out var data)
                ? data
                : throw new NotSupportedException($"not registered packet header Type{packetType.Name}");
        }

        public static IEnumerable<Type> GetReqTypeAll()
        {
            return _headerToRequestTypeMap.Values;
        }
        public static IEnumerable<Type> GetResTypeAll()
        {
            return _headerToResponseTypeMap.Values;
        }

        public static IEnumerable<Type> GetAllTypeAll()
        {
            return _headerDataMap.Keys;
        }

        public static PacketHeader GetHeader(Type packetType)
        {
            return _typeToHeaderMap[packetType];
        }

        public static string GetRequestUri(Type requestType)
        {
            if (!_typeToRequestUri.TryGetValue(requestType, out var requestUri))
                return null;

            return requestUri;
        }

        public static Type GetReqType(string requestUri)
        {
            if (!_requestUriToType.TryGetValue(requestUri, out var type))
                return null;

            return type;
        }

        public static Type GetReqType(PacketHeader header)
        {
            return GetReqType(header.Major, header.Minor);
        }
        
        public static Type GetResType(PacketHeader header)
        {
            return GetResType(header.Major, header.Minor);
        }

        public static Type GetReqType(byte major, byte minor)
        {
            return _headerToRequestTypeMap.TryGetValue((major * 1000) + minor, out var type)
                ? type
                : throw new NotSupportedException($"not registered REQ packet header major: {major}, minor: {minor}");
        }

        public static Type GetResType(byte major, byte minor)
        {
            return _headerToResponseTypeMap.TryGetValue((major * 1000) + minor, out var type)
                ? type
                : throw new NotSupportedException($"not registered RES packet header major: {major}, minor: {minor}");
        }

        public static Type GetTotalResType(byte major, byte minor)
        {
            return _headerToTotalResponseTypeMap.TryGetValue((major * 1000) + minor, out var type)
                ? type
                : throw new NotSupportedException($"not registered total RES packet header major: {major}, minor: {minor}");
        }

        public static Type GetNtfType(byte major, byte minor)
        {
            return _headerToNtfTypeMap.TryGetValue(major * 1000 + minor, out var type) ? type 
                : throw new NotSupportedException($"not registered ntf packet header major: {major}, minor: {minor}");
        }

        public static int ConvertHeaderToInt(PacketHeader header)
        {
            return (header.Major* 1000) + header.Minor;
        }

    }
}
