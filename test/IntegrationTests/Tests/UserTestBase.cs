using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Config;
using Integration.Tests.Base;
using Integration.Tests.Client;
using Integration.Tests.Fixtures;
using Integration.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Clock;
using Shared.CsvData;
using Shared.CsvParser;
using Shared.Entities;
using Shared.Entities.Models;
using Shared.Packet.Models;
using Shared.Packet.Server.Extensions;
using Shared.Packet.Utility;
using Shared.PacketModel;
using Shared.Repository;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Config;
using Shared.ServerApp.Model;
using Shared.ServerApp.Services;
using Shared.ServerModel;
using Xunit;

namespace Integration.Tests.Tests
{
    public class ServerFixtureData
    {
        public readonly Dictionary<int, GateServerFixtureBase> GateFixtures = new();
        public readonly Dictionary<int, FrontendServerFixtureBase> FrontendFixtures = new();

        public void Initialize()
        {
            // 초기화
            foreach (var frontendFixture in FrontendFixtures.Values)
            {
                frontendFixture.CreateClient();
            }
            foreach (var gateFixture in GateFixtures.Values)
            {
                gateFixture.CreateClient();
            }
        }
    }
    public partial class UserTestBase
    {
        protected readonly ILogger<UserTestBase> Logger;
        protected readonly ServerFixtureData ServerFixtures =new();

        private static int _nextTestEventIndex = 90001;

        public const int EmoticonId = 3914;
        public const string EmoticonIdString = "3914";

        public UserTestBase(
            ILogger<UserTestBase> logger,
            FrontendServer01Fixture frontend01Fixture,
            FrontendServer02Fixture frontend02Fixture,            
            GateServer01Fixture gate01Fixture,
            GateServer02Fixture gate02Fixture
            )
        {
            Logger = logger;

            ServerFixtures.FrontendFixtures.Add(1, frontend01Fixture);
            ServerFixtures.FrontendFixtures.Add(2, frontend02Fixture);

            ServerFixtures.GateFixtures.Add(1, gate01Fixture);
            ServerFixtures.GateFixtures.Add(2, gate02Fixture);

            ServerFixtures.Initialize();
        }

        
        public T GetServiceByFront<T>(int index)
        {
            return ServerFixtures.FrontendFixtures[index].Services.GetRequiredService<T>();
        }

        public FrontendServerFixtureBase GetFrontendServer(int index)
        {
            return ServerFixtures.FrontendFixtures[index];
        }
        public GateServerFixtureBase GetGateServer(int index)
        {
            return ServerFixtures.GateFixtures[index];
        }

        private ChangeableSettings<T>[] GetSettingsArray<T>(
            ChangeableSettings<T> frontend,ChangeableSettings<T> gate)
        {
            var settingsServerTypeValues = Enum.GetValues(typeof(SettingsTypes));
            var settingsArray = new ChangeableSettings<T>[settingsServerTypeValues.Length];
            for (var i = 0; i < settingsServerTypeValues.Length; i++)
            {
                var value = (SettingsTypes) settingsServerTypeValues.GetValue(i);
                switch (value)
                {
                    case SettingsTypes.Frontend:
                        settingsArray[(int)value] = frontend;
                        break;
                    case SettingsTypes.Gate:
                        settingsArray[(int)value] = gate;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return settingsArray;
        }
        
        public CsvStoreContextData GetCsvData(int appIndex = 1)
        {
            return GetFrontendServer(appIndex).CsvData;
        }

        
        public SettingsContext<TokenSettings> BeginTokenSettings(int appIndex = 1)
        {
            var settingsArray = GetSettingsArray(
                GetFrontendServer(appIndex).TokenSettings,
                GetGateServer(appIndex).TokenSettings);

            return new (settingsArray);
        }


        
        public async Task<TestUserContext> MakeUcBaseAsync(TestEnv testEnv = null)
        {
            testEnv ??= TestEnv.Default;

            var gateServerHttpClient = await GetGateServer(testEnv.GateAppIndex).GetTestHttpClientAsync();
            var frontendHttpClient = await GetFrontendServer(testEnv.FrontendHttpAppIndex).GetTestHttpClientAsync();
//            var frontendSessionClient = await GetFrontendServer(testEnv.FrontendSessionAppIndex).GetTestSessionClientAsync();
            var uc = new TestUserContext(Logger, testEnv);

            uc.AddTestClient(gateServerHttpClient);
            uc.AddTestClient(frontendHttpClient);
//            uc.AddTestClient(frontendSessionClient);            

            return uc;
        }

        public async Task<TestUserContext> MakeUcAccountAsync(TestEnv testEnv = null, TestUserContext alreadyUc = null)
        {
            testEnv ??= TestEnv.Default;

            if (alreadyUc != null)
                await alreadyUc.DisposeAsync();

            var uc = await MakeUcBaseAsync(testEnv);


            return uc;
        }

        public async Task<TestUserContext> MakeUcAvatarAsync(TestEnv testEnv = null, TestUserContext alreadyUc = null)
        {
            testEnv ??= TestEnv.Default;

            var uc = await MakeUcAccountAsync(testEnv, alreadyUc);

            if (alreadyUc == null || string.IsNullOrEmpty(alreadyUc.LoginId))
            {
                var nick = $"UT{uc.UserSeq}";
              

              
            }

            return uc;
        }

        public async Task<TestUserContext> MakeUcPlayerAsync(TestEnv testEnv = null, TestUserContext alreadyUc = null)
        {
            testEnv ??= TestEnv.Default;

            var uc = await MakeUcAvatarAsync(testEnv, alreadyUc);

            // 유저 데이터 강제로 세팅
            if (alreadyUc == null)
            {
                var dbRepo = ServerFixtures.FrontendFixtures.First().Value.DbRepo;
                using var userCtx = dbRepo.GetUserDb(uc.UserSeq);
                //await userCtx.Users.AddAsync(new User()
                //{
                //    Seq = uc.UserSeq,
                //    NormalGrade = testEnv.NormalGrade,
                //    SpecialGrade = testEnv.SpecialGrade,
                //    Level = testEnv.Level,
                //    Exp = testEnv.Exp,
                //});
                await userCtx.SaveChangesAsync();
            }

            var connectFrontendResult = await uc.SendPacketAsync<ConnectSessionRes>(new ConnectSessionReq
            {
                UserSeq = uc.UserSeq,
                Language = 0,
                OsType = (byte) OSType.Android,
                AppVer = "1",
            });

            AssertEx.EqualResult(ResultCode.Success, connectFrontendResult.ResultCode);

            return uc;
        }

        public async Task<TestUserContext> MakeUcAsync(TestEnv testEnv = null, TestUserContext alreadyUc = null)
        {
            testEnv ??= TestEnv.Default;

            var uc = await MakeUcPlayerAsync(testEnv, alreadyUc);

         
            uc.ClearResponses();

            return uc;
        }

        public async Task<MultipleTestUserContext> MakeMultipleUcAsync(int count, TestEnv testEnv = null)
        {
            var ucArray = new TestUserContext[count];
            for (var i = 0; i < ucArray.Length; i++)
                ucArray[i] = await MakeUcAsync(testEnv).ConfigureAwait(false);

            return new MultipleTestUserContext(ucArray);
        }
     
    }
}

