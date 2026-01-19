using System;
using System.Text;
using System.Threading.Tasks;
using Common.Config;
using SampleGame.Shared.Common;
using Shared.Services.Redis;
using Microsoft.Extensions.Logging;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Config;
using JwtPayload = Shared.Session.Models.JwtPayload;
using Shared.Clock;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Shared.ServerApp.Services
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(long userSeq, byte osType);
        ResultCode ExtractToken(string token, out JwtPayload payload);
        Task<ResultCode> ValidateTokenAsync(JwtPayload payload);
    }

    public class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> _logger;
        private readonly ChangeableSettings<TokenSettings> _tokenSettings;
        private readonly RedisRepositoryService _redisRepo;
        private readonly DatabaseRepositoryService _dbRepo;
        
        public TokenService(
            ILogger<TokenService> logger,
            ChangeableSettings<TokenSettings> tokenSettings,
            RedisRepositoryService redisRepo,
            DatabaseRepositoryService dbRepo
            )
        {
            _logger = logger;
            _tokenSettings = tokenSettings;
            _redisRepo = redisRepo;
            _dbRepo = dbRepo;
        }

        public async Task<string> GenerateTokenAsync(long userSeq, byte osType)
        {
            try
            {
                var accountRedis = _redisRepo.GetDb(RedisDatabase.Account);
                var redisTokenKey = string.Format(RedisKeys.UserToTokenMap, userSeq);
                var tokenAgeValue = await accountRedis.StringGetAsync(redisTokenKey).ConfigureAwait(false);
                var tokenAge = tokenAgeValue.HasValue && tokenAgeValue.TryParse(out int age) ? age + 1 : 0;
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Value.Secret));


                var claims = new List<Claim>
                {
                    new Claim(nameof(JwtPayload.Seq),userSeq.ToString()),
                    new Claim(nameof(JwtPayload.OsType),osType.ToString()),
                    new Claim(nameof(JwtPayload.Age), tokenAge.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                var jwtSecurityToken = new JwtSecurityToken
                (                    
                    claims: claims,
                    expires: AppClock.UtcNow.Add(_tokenSettings.Value.TokenExpires),
                    signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)

                );
                await accountRedis.StringSetAsync(redisTokenKey, tokenAge, _tokenSettings.Value.TokenExpires).ConfigureAwait(false);
                var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                return token;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e.ToString());
                return null;
            }
        }

        public ResultCode ExtractToken(string token, out JwtPayload payload)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    payload = null;
                    return ResultCode.InvalidToken;
                }
                payload = new JwtPayload();
                return ResultCode.Success;
                //var result = _jwtDecoder.TryDecode(token,
                //    x =>
                //    (JwtPayload)JsonTextSerializer.Deserialize(x,typeof(JwtPayload))
                //    ,out payload);
                
                //switch (result)
                //{
                //    case DecodeResult.Success:
                //        return ResultCode.Success;
                //    case DecodeResult.FailedVerifyExpire:
                //        payload = null;
                //        return ResultCode.ExpiredToken;
                //    default:
                //        payload = null;
                //        return ResultCode.InvalidToken;
                //}
            }
            catch (Exception e)
            {
                _logger.LogWarning(e.ToString());

                payload = null;
                return ResultCode.InvalidToken;
            }
        }

        
        //public async Task<ResultCode> ValidateTokenAsync(JwtPayload payload)
        //{
        //    if (payload.Seq == 0)
        //    {
        //        return ResultCode.InvalidToken;
        //    }

        //    if (_languageService.IsSupport(payload.Lang) == false)
        //    {
        //        return ResultCode.InvalidToken;
        //    }

        //    //EasyCaching
        //    //var redisKey = RedisKeys.UserAccessToken(payload.Seq);
        //    //var inMemoryData = await _localCache.GetAsync<string>(redisKey);
        //    //string token = "";
        //    //if (inMemoryData.HasValue == false)
        //    //{
        //    //    var tokenValue = await _redisRepo.Account.StringGetAsync(redisKey).ConfigureAwait(false);
        //    //    if (tokenValue.HasValue == false)
        //    //    {
        //    //        return ResultCode.InvalidToken;
        //    //    }
        //    //    token = tokenValue.ToString();
        //    //}
        //    //else
        //    //{
        //    //    token = inMemoryData.Value;
        //    //}

        //    //without EasyCaching
        //    var redisKey = RedisKeys.UserAccessToken(payload.Seq);
        //    var tokenValue = await _redisRepo.Account.StringGetAsync(redisKey).ConfigureAwait(false);
        //    string token = "";
        //    if (tokenValue.HasValue == false)
        //    {
        //        return ResultCode.InvalidToken;
        //    }
        //    token = tokenValue.ToString();
            
        //    var tokenResult = ExtractToken(token, out var decodePayload);
            
        //    return tokenResult;
        //}

        public async Task<ResultCode> ValidateTokenAsync(JwtPayload payload)
        {
            if (payload.Seq.Equals(0))
            {
                return ResultCode.InvalidToken;
            }


            var accountRedis = _redisRepo.Account;
            var key = string.Format(RedisKeys.UserToTokenMap, payload.Seq);
            var tokenAgeValue = await accountRedis.StringGetAsync(key).ConfigureAwait(false);
            if (!tokenAgeValue.HasValue)
            {
                using var userCtx = _dbRepo.GetUserDb(payload.Seq);
                var userAccount = await userCtx.UserAccounts.FindAsync(payload.Seq);
                if( userAccount == null)
                {
                    return ResultCode.InvalidToken;
                }

                if (userAccount.TokenExpireDt != null && userAccount.TokenExpireDt > AppClock.UtcNow)
                {   
                    await accountRedis.StringSetAsync(key, payload.Age, userAccount.TokenExpireDt.Value.TimeOfDay).ConfigureAwait(false);
                    tokenAgeValue = payload.Age;
                }
                else
                {
                    return ResultCode.InvalidToken;
                }
            }

            if (payload.Age != tokenAgeValue)
            {
                return ResultCode.PrevToken;
            }

            return ResultCode.Success;
        }
    }
}