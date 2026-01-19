using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Connection;
using Shared.Session.Base;

namespace Shared.ServerApp.Services
{
    public class GateService
    {
        private const int DefaultAppGroupId = 1;
        private const string InvalidAppVersionKey = "";

        private int _recommendServerGroupId = DefaultAppGroupId;

        private readonly ILogger<GateService> _logger;
        private readonly AppContextServiceBase _appContextBase;
        private readonly AppServerSessionServiceBase _serverSessionService;
        private readonly DatabaseRepositoryService _dbRepo;
        
      
        private readonly Random _random = new();

        public UserAccessGrade AccessGrade { get; set; }
        
        public GateService(
            ILoggerFactory loggerFactory,
            AppContextServiceBase appContextBase,
            AppServerSessionServiceBase serverSessionService,
            DatabaseRepositoryService dbRepo
            )
        {
            _logger = loggerFactory.CreateLogger<GateService>();
            _dbRepo = dbRepo;
            _appContextBase = appContextBase;
            _serverSessionService = serverSessionService;            
        }

        public int RecommendServerGroupId => _recommendServerGroupId;

        public async Task StartAsync()
        {
            await LoadAllAsync();
        }

        public async Task RefreshAsync()
        {
         
        }

        public async Task LoadAllAsync()
        {
            using var gateCtx = _dbRepo.GetGateDb();

         
        }
       


        //public IReadOnlyList<AppServerDataModel> GetServers(int appGroupId = DefaultAppGroupId)
        //{
        //    return Array.Empty<AppServerDataModel>();
        //}

        

        public string GetIP(HttpRequest request)
        {
            var ip = request.Headers["x-forwarded-for"].ToString();

            if (string.IsNullOrEmpty(ip))
            {
                ip = request.HttpContext.Connection.RemoteIpAddress?.ToString();
            }

            return ip;
        }

        //public async Task<bool> CheckIpFromBlockCountry(byte[] ipAddr)
        //{
        //    var gateCtx = _dbRepo.GetGateDb();
        //    var ipTable = await gateCtx.NationIpTable.ToArrayAsync();

        //    if (ipTable == null || ipTable.Length < 1)
        //    {
        //        return false;
        //    }

        //    bool lowerBoundary = true, upperBoundary = false;
        //    foreach (var range in ipTable)
        //    {
        //        var lowerIP = range.LowerIPBytes;
        //        var upperIP = range.UpperIPBytes;

        //        //처음과 끝 ip가 동일할 경우
        //        if (lowerIP.SequenceEqual(upperIP) == true)
        //        {
        //            return (lowerIP.SequenceEqual(ipAddr));
        //        }

        //        for (var i = 0; i < lowerIP.Length; i++)
        //        {
        //            lowerBoundary = (ipAddr[i] == lowerIP[i]);
        //            upperBoundary = (ipAddr[i] == upperIP[i]);

        //            // 시작 ip와 끝 ip 양쪽 대역폭이 동일하면 다음 클래스에서 비교
        //            if (lowerBoundary && upperBoundary)
        //            {
        //                continue;
        //            }

        //            if (ipAddr[i] >= lowerIP[i] &&
        //                ipAddr[i] <= upperIP[i])
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    return false;
        //}
    }
}