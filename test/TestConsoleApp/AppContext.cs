using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using SampleGame.Shared.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.CsvData;
using Shared.Packet.Utility;
using Shared.PacketModel;
using Shared.PacketModel.Test;
using Shared.Server.Define;
using Shared.Utility;
using TestConsoleApp.Base;
using TestConsoleApp.Network;
using TestConsoleApp.Network.Base;
using TestConsoleApp.User;

namespace TestConsoleApp
{
    public class AppContext
    {
        private static readonly string[] _sharedPacketAssemblyNames = { "Shared.Packet.Server" };

        public readonly NetworkManager Net;

        private readonly string _gateHost;
        public const byte DefaultLanguage = 0;
        public readonly IServiceProvider AppServices;
        private readonly ILogger _logger;

        public AppContext(string gateHost,IServiceProvider serviceProvider)
        {
            _gateHost = gateHost;
            AppServices = serviceProvider;
            var loggerFactory = AppServices.GetRequiredService<ILoggerFactory>();
            _logger = AppServices.GetRequiredService<ILogger<AppContext>>();
            PacketHeaderTable.Build(_sharedPacketAssemblyNames);

            Net = new NetworkManager(serviceProvider,loggerFactory,new []
            {
                new NetConnectInfo
                {
                    ServiceTypes = new []{ NetServiceType.Gate },
                    ProtocolType = ProtocolType.Http,
                    Host = _gateHost,
                    Port = 0,
                },

                new NetConnectInfo
                {
                    ServiceTypes = new []{ NetServiceType.Auth },
                    ProtocolType = ProtocolType.Http,
                },

                new NetConnectInfo
                {
                    ServiceTypes = new []{ NetServiceType.Api },
                    ProtocolType = ProtocolType.Http,
                },

                new NetConnectInfo
                {
                    ServiceTypes = new []{ NetServiceType.FrontEnd },
                    ProtocolType = ProtocolType.Tcp,
                },
                
            });
        }

        public Task<TestUserContext> CreateUserAsync()
        {
            return Task.FromResult(new TestUserContext(this));
        }

        public async Task<TestUserContext> CreateAvatarUserAsync(bool isCreateUser = true, int existingUserIdIndex = -1)
        {
            var user = new TestUserContext(this);

            #region Gate 연결
            await user.ConnectAsync(new[] { NetServiceType.Gate}, _gateHost,null);
            #endregion

            #region 서버 리스트 가져오기
            await user.SendAsync<GetServerInfosRes>(new GetServerInfosReq
            {
                OsType = OSType.Android,
                AppVer = 1
            });
            #endregion

     

            return user;
        }

  


        public async Task<TestUserContext> CreateChatUserAsync(string chatHostAndPort)
        {
            var user = await CreateAvatarUserAsync();

            await Task.Delay(1000);

            return user;
        }

        #region 서버 시간 요청
      
        #endregion

     
    }
}