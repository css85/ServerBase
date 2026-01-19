using System;
using System.Linq;
using System.Threading.Tasks;
using SampleGame.Shared.Common;
using Shared.Services.Redis;
using FrontEndWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Clock;
using Shared.PacketModel;
using Shared.Repository.Services;
using Shared.ServerApp.Mvc;
using Shared.ServerApp.Services;
using Microsoft.AspNetCore.Authorization;
using Shared.Entities.Models;
using Common.Config;
using Shared.ServerApp.Config;
using LitJWT;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace FrontEndWeb.Controllers
{
    //    [ApiVersion("1.0")]
    //    [Route("api/v{version:apiVersion}/auth")]
    [Route("api/auth")]
    public class AuthController : TokenBasedApiController
    {
        private readonly ILogger<AuthController> _logger;
        private readonly DatabaseRepositoryService _dbRepo;        
        private readonly ITokenService _tokenService;
        private readonly ServerInspectionService _serverInspectionService;
        private readonly PlayerService _playerService;
        private readonly FrontCheckService _frontCheckService;
        private readonly ChangeableSettings<TokenSettings> _tokenSettings;

        public AuthController(
            ILogger<AuthController> logger,
            DatabaseRepositoryService dbRepo,
            ChangeableSettings<TokenSettings> tokenSettings,
            ServerInspectionService serverInspectionService,
            PlayerService playerService,
            FrontCheckService frontCheckService,
            ITokenService tokenService
            )
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _tokenService = tokenService;
            _serverInspectionService = serverInspectionService;
            _tokenSettings = tokenSettings;
            _playerService = playerService;
            _frontCheckService = frontCheckService;
        }

        /// <summary>
        /// 해당 계정의 토큰 발급 (계정이 없으면 생성)        
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>        
        /// <response code="1">토큰발급 실패 (다시시도 필요)</response>

        [AllowAnonymous]
        [HttpPost("auth")]
        public async Task<ActionResult<AccountAuthRes>> AccountAuthAsync([FromBody] AccountAuthReq req)        
        {
            using var userCtx = _dbRepo.GetUserDb();

            //_logger.LogInformation("//////////////////auth////////////////////////////////////////////////");
            //_logger.LogInformation("type : " + req.AccountType.ToString() + "  id : " + req.Id);
            //_logger.LogInformation("//////////////////////////////////////////////////////////////////");
            if (req.AccountType != AccountType.Guest && string.IsNullOrEmpty(req.Id))
                return Ok<AccountAuthRes>(ResultCode.InvalidParameter);

            if (Enum.IsDefined(typeof(OSType), req.OsType) == false || req.OsType == OSType.None)
                return Ok<AccountAuthRes>(ResultCode.InvalidParameter);

            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<AccountAuthRes>(ResultCode.ServerInspection);

          
            
            // 게스트 최초 로그인 
            if( req.AccountType == AccountType.Guest && string.IsNullOrEmpty(req.Id))
            {
                string guestId = string.Empty;
                int checkCount = 0;
                while (string.IsNullOrEmpty(guestId))
                {
                    if( checkCount > 100 )
                    {
                        return Ok<AccountAuthRes>(ResultCode.ServerError);
                    }
                    var randId = Guid.NewGuid().ToString();
                    var checkDb = await userCtx.UserAccountLinks.Where(p => p.AccountType == req.AccountType && p.AccountId == randId).FirstOrDefaultAsync();                    
                    if (checkDb == null)
                    {
                        guestId = randId;
                        break;
                    }
                    checkCount++;   
                }
                req.Id = guestId;
            }
            var accountLinkDb = await userCtx.UserAccountLinks.Where(p => p.AccountType == req.AccountType && p.AccountId == req.Id).FirstOrDefaultAsync();
            var accountDb = new AccountModel();

            if(accountLinkDb == null)
            {
                bool newUser = true;
                if(req.AccountType == AccountType.GameCenter)
                {
                    if( !string.IsNullOrEmpty(req.BeforeId) )
                    {
                        accountLinkDb = await userCtx.UserAccountLinks.Where(p => p.AccountType == req.AccountType && p.AccountId == req.BeforeId).FirstOrDefaultAsync();
                        if (accountLinkDb != null)
                        {
                            newUser = false;
                            accountDb = await userCtx.UserAccounts.Where(p => p.UserSeq == accountLinkDb.UserSeq).FirstOrDefaultAsync();
                            if (accountDb == null)
                                return Ok<AccountAuthRes>(ResultCode.NotFound);

                            accountLinkDb.AccountId = req.Id;
                            userCtx.UserAccountLinks.Update(accountLinkDb);

                            await userCtx.AppleLoginTransferHistory.AddAsync(new AppleLoginTransferHistoryModel
                            {
                                UserSeq = accountLinkDb.UserSeq,
                                AccountType = req.AccountType,
                                BeforeAccountId = req.BeforeId,
                                AfterAccountId = req.Id,
                                Comment = "GameCenter Voxel Busters"
                            });
                        }
                    }
                }

                if( newUser)
                {
                    accountDb = new AccountModel
                    {
                        Block = false,
                    };
                    await userCtx.UserAccounts.AddAsync(accountDb);
                    await userCtx.SaveChangesAsync();   // user_seq AutoIncrement 로 일단 저장

                    accountLinkDb = new AccountLinkModel
                    {
                        UserSeq = accountDb.UserSeq,
                        AccountType = req.AccountType,
                        AccountId = req.Id,
                    };
                    await userCtx.UserAccountLinks.AddRangeAsync(accountLinkDb);
                }
            }
            else
            {
                accountDb = await userCtx.UserAccounts.Where(p=>p.UserSeq == accountLinkDb.UserSeq).FirstOrDefaultAsync();  
                if( accountDb == null )
                    return Ok<AccountAuthRes>(ResultCode.NotFound);
            }

            if (_frontCheckService.BlockUsers.Any(p => p == accountDb.UserSeq))
                return Ok<AccountAuthRes>(ResultCode.BlockUser);

            accountDb.ApiToken = await _tokenService.GenerateTokenAsync(accountDb.UserSeq, (byte)req.OsType);
            accountDb.TokenExpireDt = AppClock.UtcNow.Add(_tokenSettings.Value.TokenExpires);

            if (string.IsNullOrEmpty(accountDb.ApiToken))
                return Ok<AccountAuthRes>(1);

            var countryCode = req.AccountType == AccountType.Guest ? "KR" : _frontCheckService.GetCountryCode(GetIP());
            if (countryCode != null)
                accountDb.CountryCode = countryCode;

            if( accountDb.TodayFirstLoginDt.Day != AppClock.UtcNow.Day) 
                accountDb.TodayFirstLoginDt = AppClock.UtcNow;


            accountDb.LoginDt = AppClock.UtcNow;
            accountDb.PushToken = req.PushToken;
            userCtx.Update(accountDb);

            var firstLogin = false;
            var userInfoDb = await userCtx.UserInfos.FindAsync(accountDb.UserSeq);
            if (userInfoDb == null || string.IsNullOrEmpty(userInfoDb.Nick))
                firstLogin = true;

            await userCtx.SaveChangesAsync();


            return Ok(ResultCode.Success, new AccountAuthRes
            {
                HttpToken = accountDb.ApiToken,
                UserSeq = accountDb.UserSeq,
                AccountType = req.AccountType,
                Id = req.Id,                
                IsFirstLogin = firstLogin,
                NewUserConfig = _frontCheckService.NewUserConfig,
            });
        }

        [HttpPost("account-link")]
        public async Task<ActionResult<AccountLinkRes>> AccountLinkAsync([FromBody] AccountLinkReq req)
        {
            //_logger.LogInformation("///////////////account-link///////////////////////////////////////////////////");
            //_logger.LogInformation("type : " + req.AccountType.ToString() + "  id : " + req.Id);
            //_logger.LogInformation("//////////////////////////////////////////////////////////////////");
            using var userCtx = _dbRepo.GetUserDb();
            var userSeq = GetUserSeq();
            if (Enum.IsDefined(typeof(AccountType), req.AccountType) == false || req.AccountType <= AccountType.Guest)            
                return Ok<AccountLinkRes>(ResultCode.InvalidParameter);

            var checkAccountLinkDb = await userCtx.UserAccountLinks.Where(p => p.AccountType == req.AccountType && p.AccountId == req.Id).FirstOrDefaultAsync();
            //            var checkAccountDb = await userCtx.UserAccounts.Where(p => p.AccountType == req.AccountType && p.AccountId == req.Id).FirstOrDefaultAsync();
            if (checkAccountLinkDb != null)
                return Ok<AccountLinkRes>(ResultCode.ExistPlatformLink);

            var accountDb = await userCtx.UserAccounts.FindAsync(userSeq);
            if (accountDb == null)
                return Ok<AccountLinkRes>(ResultCode.NotFound);

            var userAccountLinkDb = await userCtx.UserAccountLinks.Where(p => p.UserSeq == userSeq && p.AccountType == req.AccountType).FirstOrDefaultAsync();
            if (userAccountLinkDb != null)
                return Ok<AccountLinkRes>(ResultCode.ExistPlatformLink);

            await userCtx.UserAccountLinks.AddAsync(new AccountLinkModel
            {
                UserSeq = userSeq,
                AccountType = req.AccountType,
                AccountId = req.Id,
            });

            //accountDb.AccountType = req.AccountType;
            //accountDb.AccountId = req.Id;
            //userCtx.UserAccounts.Update(accountDb);
            await userCtx.SaveChangesAsync();

            var accountLinks = await userCtx.UserAccountLinks.Where(p => p.UserSeq == userSeq && p.AccountType > AccountType.Guest).Select(p => p.AccountType).ToListAsync();

            return Ok(ResultCode.Success, new AccountLinkRes
            {
                AccountType = req.AccountType,
                Id = req.Id,
                AccountLinks = accountLinks,
            });
        }


        [HttpPost("remove-account")]
        public async Task<ActionResult<RemoveAccountRes>> AccountLinkAsync([FromBody] RemoveAccountReq req)
        {  
            using var userCtx = _dbRepo.GetUserDb();
            var userSeq = GetUserSeq();

            var accountLinkDbs = await userCtx.UserAccountLinks.Where(p => p.UserSeq == userSeq).ToListAsync();
            foreach(var db in accountLinkDbs)
            {
                db.AccountId = $"{db.AccountId}_remove";
            }

            userCtx.UserAccountLinks.UpdateRange(accountLinkDbs);
            await userCtx.SaveChangesAsync();


            return Ok(ResultCode.Success, new RemoveAccountRes
            {
                
            });
        }




        [Serializable]
        class JwtGoogle
        {
            public string kid;
        }
        //[NonAction]
        public string DecodeGoogleIdToken(string token)
        {
            var splitToken = token.Split('.');

            var decoder = new JwtDecoder();
            var result = decoder.TryDecode(splitToken[0],
                x =>
                    (JwtGoogle)JsonTextSerializer.Deserialize(x, typeof(JwtGoogle))
                    , out var jwtGoogle);
            if (result == DecodeResult.Success)
            {
                return jwtGoogle.kid;
            }

            return null;
        }
    }
}
