using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Extension;
using Shared.Packet.Utility;
using Shared.Session.Data;

namespace Shared.Session.Utility
{
    public class PacketHandlerTarget
    {
        public Type AssemblyType;
        public Type BaseType;
    }

    public class PacketHandlerData
    {
        public PacketHeader Header;
        public MethodInfo MethodInfo;
        public Type ParameterType;
        public Type RequestType;
        public Type ReturnType;
        public NetServiceTypeInfo[] ServiceTypes;
    }

    public static class PacketHandlerInfoTableMap
    {
        private static readonly Dictionary<Type, PacketHandlerInfoTable[]> _tableMap = new();

        private static readonly object _lock = new();
        
        public static void Build(Type type, PacketHandlerTarget[] packetHandlerTargets)
        {
            lock (_lock)
            {
                if (_tableMap.ContainsKey(type))
                    return;

                var packetTypes = Enum.GetValues(typeof(PacketType)).Cast<PacketType>().Where(p => p != PacketType.None)
                    .ToArray();

                _tableMap.Add(type, new PacketHandlerInfoTable[packetTypes.Length + 1]);
                foreach (var packetType in packetTypes)
                {
                    if (packetType == PacketType.Response)
                        continue;

                    var parameterBaseType = packetType switch
                    {
                        PacketType.Request => typeof(RequestBase),
                        PacketType.Ntf => typeof(NtfBase),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    var returnType = packetType switch
                    {
                        PacketType.Request => typeof(Task<ResponseReply>),
                        PacketType.Ntf => typeof(Task),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    var table = new PacketHandlerInfoTable();
                    table.Build(parameterBaseType, packetHandlerTargets, returnType);
                    _tableMap[type][(int)packetType] = table;
                }
            }
        }

        public static PacketHandlerData Get(Type type, PacketType packetType, PacketHeader packetHeader)
        {
            return _tableMap[type][(int)packetType].Get(packetHeader);
        }

        public static PacketHandlerData Get(Type type, PacketHeaderData headerData)
        {
            if (headerData == null)
                return null;

            return _tableMap[type][(int)headerData.PacketType].Get(headerData.Header);
        }
    }

    public class PacketHandlerInfoTable
    {
        // ReSharper disable once StaticMemberInGenericType
        private Dictionary<PacketHeader, PacketHandlerData> _sessionMethodMap = new();

        public void Build(Type parameterBaseType, PacketHandlerTarget[] packetHandlerTargets, Type returnType)
        {
            if (packetHandlerTargets == null)
                return;
            if (packetHandlerTargets.Length == 0)
                return;

            var packetHandlerDataList = new List<PacketHandlerData>();
            foreach (var packetHandlerTarget in packetHandlerTargets)
            {
                var methodData =
                    Build(packetHandlerTarget.AssemblyType, packetHandlerTarget.BaseType, parameterBaseType, returnType)
                        .Where(p => p != null).ToArray();
                if (methodData.Length > 0)
                {
                    packetHandlerDataList.AddRange(methodData);
                }
            }

            _sessionMethodMap = new Dictionary<PacketHeader, PacketHandlerData>();
            foreach (var data in packetHandlerDataList)
            {
                _sessionMethodMap.TryAdd(data.Header, data);
            }
        }

        private static IEnumerable<PacketHandlerData> Build(Type assemblyType, Type baseType, Type parameterBaseType, Type returnType)
        {
            var types = Assembly.GetAssembly(assemblyType)?.GetTypes();

            var targetTypes = types.Where(p => p.BaseType == baseType).ToArray();

            var errorMessageList = new List<string>();

            var results = targetTypes
                .SelectMany(p => p.GetMethods())
                .Where(p =>
                {
                    var parameters = p.GetParameters();
                    if (parameters.Length != 1)
                        return false;
                    if (parameters[0].ParameterType.BaseType == null)
                        return false;
                    if (parameters[0].ParameterType.BaseType != parameterBaseType)
                        return false;
                    if (p.ReturnType != returnType)
                    {
                        errorMessageList.Add(
                            $"Handler return type error: Type({parameters[0].ParameterType}), ActualReturnType({p.ReturnType}), ExpectedReturnType({returnType})");
                        return false;
                    }

                    return true;
                }).SelectMany(p =>
                {
                    PacketHandlerData reqPacketHandlerData = null;
                    PacketHandlerData resPacketHandlerData = null;
                    PacketHandlerData ntfPacketHandlerData = null;

                    // Request to Response
                    {
                        var requestType = p.GetParameters()[0].ParameterType;
                        var responseTypeName =
                            $"{requestType.Namespace}.{requestType.Name.Substring(0, requestType.Name.Length - 3) + "Res"},{requestType.AssemblyQualifiedName.Split(',')[1]}";
                        var requestAttribute = requestType.GetCustomAttribute<RequestClassAttribute>();
                        var multipleRequestAttribute = requestType.GetCustomAttribute<MultipleRequestClassAttribute>();

                        if (multipleRequestAttribute != null || requestAttribute != null)
                        {
                            byte major;
                            byte minor;
                            NetServiceTypeInfo[] serviceTypes;
                            if (multipleRequestAttribute != null)
                            {
                                major = multipleRequestAttribute.Major;
                                minor = multipleRequestAttribute.Minor;
                                serviceTypes = multipleRequestAttribute.MultipleServiceType.GetServices();
                            }
                            else
                            {
                                major = requestAttribute.Major;
                                minor = requestAttribute.Minor;
                                serviceTypes = new NetServiceTypeInfo[1]
                                {
                                    new NetServiceTypeInfo(requestAttribute.ProtocolType, requestAttribute.ServiceType),
                                };
                            }

                            reqPacketHandlerData = new PacketHandlerData
                            {
                                Header = new PacketHeader(major, minor),
                                MethodInfo = p,
                                ParameterType = p.GetParameters()[0].ParameterType,
                                RequestType = requestType,
                                ReturnType = Type.GetType(responseTypeName),
                                ServiceTypes = serviceTypes,
                            };
                        }
                    }

                    // Response to Request
                    {
                        var responseType = p.GetParameters()[0].ParameterType;
                        var requestTypeName =
                            $"{responseType.Namespace}.{responseType.Name.Substring(0, responseType.Name.Length - 3) + "Req"},{responseType.AssemblyQualifiedName.Split(',')[1]}";
                        var responseAttribute = responseType.GetCustomAttribute<ResponseClassAttribute>();
                        var multipleResponseAttribute = responseType.GetCustomAttribute<MultipleResponseClassAttribute>();

                        if (multipleResponseAttribute != null || responseAttribute != null)
                        {
                            byte major;
                            byte minor;
                            NetServiceTypeInfo[] serviceTypes;
                            if (multipleResponseAttribute != null)
                            {
                                major = multipleResponseAttribute.Major;
                                minor = multipleResponseAttribute.Minor;
                                serviceTypes = multipleResponseAttribute.MultipleServiceType.GetServices();
                            }
                            else
                            {
                                major = responseAttribute.Major;
                                minor = responseAttribute.Minor;
                                serviceTypes = new []
                                {
                                    new NetServiceTypeInfo(responseAttribute.ProtocolType, responseAttribute.ServiceType),
                                };
                            }

                            resPacketHandlerData = new PacketHandlerData
                            {
                                Header = new PacketHeader(major, minor),
                                MethodInfo = p,
                                ParameterType = p.GetParameters()[0].ParameterType,
                                RequestType = Type.GetType(requestTypeName),
                                ReturnType = responseType,
                                ServiceTypes = serviceTypes
                            };
                        }
                    }

                    // Ntf
                    {
                        var ntfType = p.GetParameters()[0].ParameterType;
                        var ntfAttribute = ntfType.GetCustomAttribute<NtfClassAttribute>();
                        var multipleNtfAttribute = ntfType.GetCustomAttribute<MultipleNtfClassAttribute>();

                        if (multipleNtfAttribute != null || ntfAttribute != null)
                        {
                            byte major;
                            byte minor;
                            NetServiceTypeInfo[] serviceTypes;
                            if (multipleNtfAttribute != null)
                            {
                                major = multipleNtfAttribute.Major;
                                minor = multipleNtfAttribute.Minor;
                                serviceTypes = multipleNtfAttribute.MultipleServiceType.GetServices();
                            }
                            else
                            {
                                major = ntfAttribute.Major;
                                minor = ntfAttribute.Minor;
                                serviceTypes = new NetServiceTypeInfo[1]
                                {
                                    new NetServiceTypeInfo(ntfAttribute.ProtocolType, ntfAttribute.ServiceType),
                                };
                            }

                            ntfPacketHandlerData = new PacketHandlerData
                            {
                                Header = new PacketHeader(major, minor),
                                MethodInfo = p,
                                ParameterType = p.GetParameters()[0].ParameterType,
                                RequestType = null,
                                ReturnType = null,
                                ServiceTypes = serviceTypes,
                            };
                        }
                    }

                    if (resPacketHandlerData != null)
                    {
                        return new[]
                        {
                            reqPacketHandlerData,
                            resPacketHandlerData,
                        };
                    }
                    else if (reqPacketHandlerData != null)
                    {
                        return new[]
                        {
                            reqPacketHandlerData,
                        };
                    }
                    else if (ntfPacketHandlerData != null)
                    {
                        return new[]
                        {
                            ntfPacketHandlerData,
                        };
                    }
                    else
                    {
                        return Array.Empty<PacketHandlerData>();
                    }
                });

            if (errorMessageList.Count > 0)
            {
                throw new Exception(string.Join("|\n", errorMessageList));
            }

            return results;
        }

        public PacketHandlerData Get(PacketHeader packetHeader)
        {
            return _sessionMethodMap.TryGetValue(packetHeader, out var methodData) ? methodData : null;
        }
    }
}
