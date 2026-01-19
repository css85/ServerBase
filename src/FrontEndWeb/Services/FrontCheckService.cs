using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3.Model.Internal.MarshallTransformations;
using Common.Config;
using Elasticsearch.Net;
using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;
using Shared;
using Shared.Clock;
using Shared.CsvData;
using Shared.Entities.Models;
using Shared.Packet.Models;
using Shared.Packet.Server.Extensions;
using Shared.Repository;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Config;
using Shared.ServerApp.Services;
using Shared.ServerApp.Utility;
using Shared.ServerModel;
using Shared.Services.Redis;
using StackExchange.Redis;
using SampleGame.Shared.Common;

namespace FrontEndWeb.Services
{
    public class FrontCheckService : IHostedService, IDisposable
    {
        private readonly ILogger<FrontCheckService> _logger;

        private readonly DatabaseRepositoryService _dbRepo;
        private readonly CsvStoreContext _csvContext;        
        private readonly RedisRepositoryService _redisRepo;
        private readonly ChangeableSettings<GameRuleSettings> _gameRule;
        private readonly PlayerService _playerService;  

        private DatabaseReader _geoIpReader;
        private Timer _timer;

        private List<long> _blockUsers = new List<long>();
        private int _timerCount = 0;
        private NewUserConfig _newUserConfig = new NewUserConfig();


        public FrontCheckService(
            ILogger<FrontCheckService> logger,
            DatabaseRepositoryService dbRepo,
            CsvStoreContext csvStoreContext,            
            ChangeableSettings<GameRuleSettings> gameRule,
            PlayerService playerService,
            RedisRepositoryService redisRepo)
        {
            _logger = logger;
            _dbRepo = dbRepo;            
            _csvContext = csvStoreContext;
            _redisRepo = redisRepo;            
            _gameRule = gameRule;
            _playerService = playerService;
            _geoIpReader = new DatabaseReader("GeoLite2-City.mmdb");
        }

        public void Dispose()
        {
            _timer?.Dispose();
            //            throw new NotImplementedException();
        }

        public string GetCountryCode(string ip)
        {
            var country = "KR";
            try
            {
                var cityData = _geoIpReader.City(ip);
                if (cityData != null)
                {
                    country = cityData.Country.IsoCode;
//                    _logger.LogInformation(string.Format("ip : {0} country : {1} ", ip, country));
                }
            } catch (Exception ex)
            {
                return country;   
            }
            return country;

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;

        }

        public async Task OnStartAsync()
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            await _playerService.SetShoppingmallRankAsync();
            await InitNewUserConfigAsync();
        }

        public void SetNewUserConfig(NewUserConfig info)
        {
            _newUserConfig = info;
        }

        public List<long> BlockUsers => _blockUsers;
        public NewUserConfig NewUserConfig => _newUserConfig;
        

        private async void DoWork(object state)
        {
            _timerCount++;            
            if (_timerCount == 60)
            {
                await SelectBlockUserAsync();
                _timerCount = 0;
            }
        }

        private async Task SelectBlockUserAsync()
        {
            using var userCtx = _dbRepo.GetUserDb();
            var blockUserDbs = await userCtx.UserAccounts.Where(p => p.Block == true).ToListAsync();

            _blockUsers = blockUserDbs.Any() ? blockUserDbs.Where(p=>p.BlockEndDt > AppClock.UtcNow).Select(p => p.UserSeq).ToList() : new List<long>();
        }

        private async Task InitNewUserConfigAsync()
        {
            using var userCtx = _dbRepo.GetUserDb();

            var configDb = await userCtx.NewUserConfigs.Where(p => p.IsActive).FirstOrDefaultAsync();
            if (configDb == null)
                return;

            var configInfo = configDb.ToNewUserConfig();
            SetNewUserConfig(configInfo);

        }


    }
}
 