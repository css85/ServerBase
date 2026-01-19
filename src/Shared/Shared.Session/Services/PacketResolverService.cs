using System;
using SampleGame.Shared.Common;
using Shared.Services.Redis;
using Microsoft.Extensions.Logging;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Data;
using Shared.Packet.Extension;
using Shared.Session.Base;
using Shared.Utility;

namespace Shared.Session.Services
{
    public class PacketResolverService
    {
        private readonly SessionManagementService _sessionManagementService;
        private readonly ILogger<PacketResolverService> _logger;
        private readonly RedisRepositoryService _redisRepo;
        private readonly AppContextServiceBase _appContext;

        public PacketResolverService(
            ILogger<PacketResolverService> logger,
            SessionManagementService sessionManagementService,
            RedisRepositoryService redisRepo,
            AppContextServiceBase appContext)
        {
            _logger = logger;
            _sessionManagementService = sessionManagementService;
            _redisRepo = redisRepo;
            _appContext = appContext;
        }

        public IPacketItem ResolverPacketItem(SessionBase session, object packet) 
        {
            var packetItem = (IPacketItem) packet;
            var headerType = packetItem.GetHeaderData();
            var dataType = headerType.DataType;
            var dataBytes = packetItem.GetDataBytes();

            var secretKey = headerType.PacketType switch
            {
                PacketType.Ntf => _sessionManagementService.GatewayEncryptKey,
                _=>session.Connection.IsAuthenticated ? session.Connection.SecretKey : _sessionManagementService.GatewayEncryptKey
            };
            
            if (_appContext.IsUnitTest)
                secretKey = null;
            
            switch (headerType.PacketType)
            {
                case PacketType.Request:
                    return new RequestPacketItem(packetItem, GetPacketData(secretKey, dataType, dataBytes));
                case PacketType.Response:
                    return new ResponsePacketItem(packetItem, GetPacketData(secretKey, dataType ,dataBytes));
                case PacketType.Ntf:
                    return new NtfPacketItem(packetItem, GetPacketData(secretKey, dataType, dataBytes));
                default:
                    throw new NotSupportedException($"not supported type : {headerType.PacketType}");
            }
        }
        private IPacketData GetPacketData(string key, Type dataType, byte[] bodyBytes)
        {
            if (key==null|| bodyBytes==null || bodyBytes.Length<1)
                return null;
            
            IPacketData data = null;
            
            try
            {
                var decryptedBytes = EncryptProvider.DecryptAes256(key, bodyBytes);
                data = JsonTextSerializer.Deserialize(decryptedBytes, dataType) as IPacketData;
            }
            catch (Exception e)
            {
                _logger.LogError(e,"{Type} Decrypt fail : {SecretKey}",dataType.Name,key);
            }

            return data;
        }
    }
}