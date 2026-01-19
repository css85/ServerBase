using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Common.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Clock;
using Shared.CsvData;
using Shared.Entities.Models;
using Shared.Packet;
using Shared.Packet.Models;
using Shared.Repository;
using Shared.Repository.Database;
using Shared.Server.Define;
using Shared.ServerApp.Config;
using Shared.ServerApp.Model;
using Shared.ServerApp.Connection;
using static Shared.Session.Extensions.ReplyExtensions;
using Shared.Server.Packet.Internal;


namespace Shared.ServerApp.Services
{
    public class UserContextDataService
    {
        private readonly ILogger<UserContextDataService> _logger;
        private readonly ChangeableSettings<GameRuleSettings> _gameRule;
        private readonly CsvStoreContext _csvStoreContext;
        private readonly AppServerSessionServiceBase _sessionService;
        private readonly ConcurrentDictionary<long, UserContextData> _contextDataMap = new();

        public UserContextDataService(
            ILogger<UserContextDataService> logger,
            ChangeableSettings<GameRuleSettings> gameRule,
            CsvStoreContext csvStore,
            AppServerSessionServiceBase sessionService
            )
        {
            _logger = logger;
            _gameRule = gameRule;
            _csvStoreContext = csvStore;
            _sessionService = sessionService;
        }

        public void SetContextData(UserContextData data)
        {
            _contextDataMap[data.UserSeq] = data;
        }

        public void ReleaseContextData(long userSeq, int sessionId)
        {
            if( _contextDataMap.TryGetValue(userSeq, out var data ) )
            {
                if( data.SessionId == sessionId)
                    _contextDataMap.TryRemove(userSeq, out _);
            }
        }

        /// <summary>
        /// FrontEndSession에 접속중인 유저의 UserContextData 가져오기
        /// </summary>
        public UserContextData Get(long userSeq)
        {
            return _contextDataMap.TryGetValue(userSeq, out var userContextData) ? userContextData : null;
        }

        /// <summary>
        /// FrontEndSession에 접속중인 유저의 UserContextData 가져오기
        /// </summary>
        public UserContextData[] Get(long[] users)
        {
            if (users == null || users.Length == 0)
                return Array.Empty<UserContextData>();

            var results = new UserContextData[users.Length];
            for (var i = 0; i < users.Length; i++)
                results[i] = Get(users[i]);

            return results;
        }

      

    }
}