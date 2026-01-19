using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Elastic.Apm.Api;
using Elastic.Apm.StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Server.Define;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using SampleGame.Shared.Common;

namespace Shared.Services.Redis
{
    public class RedisRepositoryService 
    {
        private readonly ILogger<RedisRepositoryService> _logger;
        private readonly IRedisCacheConnectionPoolManager _redisPool;

        private IDatabase[] _databases;
        private readonly IConfiguration _config;

        public IDatabase Session { get; private set; }
        public IDatabase User { get; private set; }
        public IDatabase Pve { get; private set; }
        public IDatabase Ranking { get; private set; }
        public IDatabase WebTool { get; private set; }
        public IDatabase Account { get; private set; }
        public IDatabase Internal { get; private set; }
        public IDatabase App { get; private set; }
        public IDatabase Etc { get; private set; }


        private IServer[] _servers;
        private readonly Dictionary<string, LoadedLuaScript> _luaScriptMap = new();

        public RedisRepositoryService(
            ILogger<RedisRepositoryService> logger,
            IRedisCacheConnectionPoolManager redisPool,
            IConfiguration config)
        {
            _logger = logger;
            _redisPool = redisPool;
            _config = config;
        }

        public async Task OnStartedAsync()
        {
            var connection = _redisPool.GetConnection();
            _databases = new IDatabase[Enum.GetValues<RedisDatabase>().Length];
            foreach (var type in Enum.GetValues<RedisDatabase>())
            {
                if (type == RedisDatabase.None)
                    continue;

                var redisDatabase = connection.GetDatabase((int) type);                
                _databases[(int) type] = redisDatabase;

                var endPoint = connection.GetEndPoints();
                switch (type)
                {
                    case RedisDatabase.Session:
                        Session = redisDatabase;
                        break;
                    case RedisDatabase.User:
                        User = redisDatabase;
                        break;                    
                    case RedisDatabase.Ingame:
                        Pve = redisDatabase;
                        break;
                    case RedisDatabase.Ranking:
                        Ranking = redisDatabase;
                        break;
                    case RedisDatabase.WebTool:
                        WebTool = redisDatabase;
                        break;
                    case RedisDatabase.Account:
                        Account = redisDatabase;
                        break;
                    case RedisDatabase.Internal:
                        Internal = redisDatabase;
                        break;
                    
                    case RedisDatabase.App:
                        App = redisDatabase;
                        break;
                    case RedisDatabase.Etc:
                        Etc = redisDatabase;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            var endPoints = _redisPool.GetConnection().GetEndPoints();
            _servers = new IServer[endPoints.Length];
            for (var i = 0; i < endPoints.Length; i++)
            {
                _servers[i] = _redisPool.GetConnection().GetServer(endPoints[i]);
            }

            await AddLuaScriptAsync(nameof(RedisScripts.SortedSetModify), RedisScripts.SortedSetModify);
            await AddLuaScriptAsync(nameof(RedisScripts.ListLeftPushTrimWhenOverCount), RedisScripts.ListLeftPushTrimWhenOverCount);
        }

        public IDatabase GetDb(RedisDatabase db)
        {
            return _databases[(int) db];
        }

        public List<RedisKey> GetKeys(RedisDatabase db, string pattern)
        {
            var keyList = new List<RedisKey>();    
            foreach( var server in _servers)
            {
                var keys = server.Keys(database: (int)db,  pattern: pattern).ToList();
                keyList.AddRange(keys);
            }
            return keyList;
        }

        public int GetKeysCount(RedisDatabase db, string pattern)
        {
            var keyList = GetKeys(db, pattern); 
            return keyList.Count;   
        }

        public List<T> JsonGet<T>(IDatabase db, RedisKey[] keys)
        {
            RedisValue[] redisValues = db.StringGet(keys);
            return redisValues.Select(v => JsonTextSerializer.Deserialize<T>(v)).ToList();
            
//            return redisValues.Select(v => JsonSerializer.Deserialize<T>(v, _jsonSerializerOptions)).ToList();
        }

        private async Task AddLuaScriptAsync(string name, string script)
        {
            foreach (var server in _servers)
            {
                if (await server.ScriptExistsAsync(script) == false)
                {
                    var preparedScript = LuaScript.Prepare(script);
                    var loadedScript = await server.ScriptLoadAsync(preparedScript);
                    _luaScriptMap.Add(name, loadedScript);
                }
            }
        }

        public LoadedLuaScript GetLuaScript(string name)
        {
            return _luaScriptMap.TryGetValue(name, out var luaScript) ? luaScript : null;
        }
    }
}