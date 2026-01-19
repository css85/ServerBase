using System;
using System.Threading.Tasks;
using Shared.Services.Redis;
using Microsoft.Extensions.Logging;
using Shared.Clock;
using Shared.Server.Define;
using Shared.ServerApp.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using MaxMind.GeoIP2;

namespace ChatServer.Services
{
    public class ChatCheckService : IHostedService, IDisposable
    {
        private readonly ILogger<ChatCheckService> _logger;       
        private readonly RedisRepositoryService _redisRepo;
        private readonly PlayerService _playerService;

        private DatabaseReader _geoIpReader;
        private Timer _timer;

        private DateTime _likeBestDate = DateTime.MinValue.Date;

        public ChatCheckService(
            ILogger<ChatCheckService> logger,
            RedisRepositoryService redisRepo,
            PlayerService playerService
        )
        {
            _logger = logger;
            _redisRepo = redisRepo;
            _playerService = playerService;
            _geoIpReader = new DatabaseReader("GeoLite2-City.mmdb");
        }


        public void Dispose()
        {
            _timer?.Dispose();
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
                    _logger.LogInformation(string.Format("ip : {0} country : {1} ", ip, country));
                }
            }
            catch (Exception ex)
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
        }

        private async void DoWork(object state)
        {   
            await CheckLikeBestInfoAsync();
        }

        private async Task CheckLikeBestInfoAsync()
        {
            if (_likeBestDate.Date != AppClock.UtcNow.Date)
            {
                var redisRank = _redisRepo.GetDb(RedisDatabase.Ranking);

                await SetLikeBestInfoAsync();
                await redisRank.KeyExpireAsync(string.Format(RedisKeys.ss_LikeBestRank, AppClock.UtcNow.AddDays(-1).ToShortDateString()), TimeSpan.FromDays(2));
            }
        }
        private async Task SetLikeBestInfoAsync()
        {
//            await _playerService.SetLikeBestInfoAsync();
            _likeBestDate = AppClock.UtcNow.Date;
        }

    }
}