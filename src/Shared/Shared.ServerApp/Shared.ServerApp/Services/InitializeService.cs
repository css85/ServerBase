using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Services.Redis;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.Session.Base;
using Shared.Session.Services;

namespace Shared.ServerApp.Services
{
    public class InitializeService
    {
        private readonly ILogger<InitializeService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppContextServiceBase _appContext;
        private readonly RedisRepositoryService _redisRepo;
        private readonly DatabaseRepositoryService _dbRepo;
        private readonly SessionManagementService _sessionManagementService;
        private readonly SessionServiceBase[] _sessionServices;
        private readonly BaseConsoleCommandService[] _consoleCommandServices;
        private readonly RedLockFactory _redLockFactory;
        private readonly AppClockService _appClockService;

        public InitializeService(
            ILogger<InitializeService>  logger,
            IServiceProvider serviceProvider,
            AppContextServiceBase appContext,
            RedisRepositoryService  redisRepo,
            DatabaseRepositoryService dbRepo,
            SessionManagementService sessionManagementService,
            IEnumerable<SessionServiceBase> sessionServices,
            IEnumerable<BaseConsoleCommandService> consoleCommandServices,
            RedLockFactory redLockFactory,
            AppClockService appClockService
            )
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _appContext = appContext;
            _redisRepo = redisRepo;
            _dbRepo = dbRepo;
            _sessionManagementService = sessionManagementService;
            _sessionServices = sessionServices.ToArray();
            _consoleCommandServices = consoleCommandServices.ToArray();
            _redLockFactory = redLockFactory;
            _appClockService = appClockService;
        }

        public async Task StartAsync()
        {
            _appClockService.InitOffset();
            await _redisRepo.OnStartedAsync();
        }

        public async Task OnStartedAsync()
        {
            if (_appContext.IsUnitTest == false)
            {
                // Redis warm up
                {
                    var redisWarmUpCount = 10;
                    var redisWarmUpTasks = new Task[redisWarmUpCount];
                    foreach (var type in Enum.GetValues<RedisDatabase>())
                    {
                        if (type != RedisDatabase.None)
                        {
                            for (var i = 0; i < redisWarmUpCount; i++)
                                redisWarmUpTasks[i] = _redisRepo.GetDb(type).PingAsync();
                        }
                    }

                    await Task.WhenAll(redisWarmUpTasks);
                }
            }

            await _sessionManagementService.StartAsync();

            //foreach (var sessionService in _sessionServices)
            //{
            //    await sessionService.StartAsync();
            //}

            //await _gateService.StartAsync();

            //foreach (var scheduleService in _scheduleServices)
            //{
            //    await scheduleService.OnStartAsync();
            //}

            //foreach (var sessionService in _sessionServices)
            //{
            //    await sessionService.OnStartedAsync();
            //}

        }

        public async Task StoppingAsync()
        {
            foreach (var consoleCommandService in _consoleCommandServices)
            {
                await consoleCommandService.StopAsync();
            }

            await _sessionManagementService.StopAsync();
            
            foreach (var sessionService in _sessionServices)
            {
                await sessionService.OnStopAsync();
            }
        }

        public Task StoppedAsync()
        {
            _redLockFactory.Dispose();

            return Task.CompletedTask;
        }
    }
}
