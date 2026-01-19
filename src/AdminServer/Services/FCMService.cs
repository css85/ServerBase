using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;
using Shared.Clock;
using Shared.Entities.Models;
using Shared.Repository.Services;
using Shared.ServerApp.Services;
using Shared.Services.Redis;

namespace AdminServer.Services
{
    public class FCMService : IHostedService, IDisposable
    {
        private readonly ILogger<FCMService> _logger;

        private readonly DatabaseRepositoryService _dbRepo;
        private readonly CsvStoreContext _csvContext;

        private Timer _timer;
        private Dictionary<string, NightPushLimit> _nightLimitDic = new();


        public FCMService(
            ILogger<FCMService> logger,
            DatabaseRepositoryService dbRepo,
            CsvStoreContext csvStoreContext
            )
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _csvContext = csvStoreContext;

            InitNightLimit();
        }

        private class UserPushInfo
        {
            public string PushToken { get; set; }
            public byte Language { get; set; }
        }

        private class PushMessage
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public List<string> PushTokens { get; set; } = new();
        }

        private class NightPushLimit
        {
            public string Country { get; set; }
            public int StartHour { get; set; }
            public int EndHour { get; set; }

            public bool IsLimit()
            {
                if( StartHour <= EndHour )
                    return StartHour <= AppClock.UtcNow.Hour && EndHour > AppClock.UtcNow.Hour;
                else
                {
                    if( StartHour <= AppClock.UtcNow.Hour || EndHour > AppClock.UtcNow.Hour)
                        return true;
                }
                return false;
            }
        }

        private void InitNightLimit()
        {
            _nightLimitDic.Add("KR", new NightPushLimit
            {
                Country = "KR",
                StartHour = 12,
                EndHour = 23,
            });
            
        }

      
        private bool InitConfig()
        {
            var result = false;
            if (FirebaseApp.DefaultInstance == null)
            {
                result = true;
                try
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential =
                            GoogleCredential.FromFile("firebase.json"),
                    });
                }
                catch (ArgumentException) // 이미 DefaultInstance가 있는경우
                {
                    // ignored
                }
            }
            return result;
        }


        private async void DoWork(object state)
        {
            await CheckPushAsync();
        }

       
        public async Task OnStartAsync()
        {   
            if( InitConfig())
            {
                _logger.LogInformation("FCMService running.");
                _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            }
                
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

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private List<string> CheckNightPushLimit(List<string> countrys)
        {
            var checkCountrys = countrys.ToList();

            var limitCountrys = checkCountrys.Where(p => _nightLimitDic.Keys.Contains(p)).ToList();
            foreach(var country in limitCountrys)
            {
                if( _nightLimitDic[country].IsLimit() == true )
                {
                    checkCountrys.Remove(country);
                }
            }

            return checkCountrys;
        }
        private async Task CheckPushAsync()
        {
            try
            {
                var csvData = _csvContext.GetData();
                using var userCtx = _dbRepo.GetUserDb();
                var availablePushMessages = await LoadAvailablePushAsync();

                if (availablePushMessages == null || availablePushMessages.Count <= 0)
                    return;

                foreach(var message in availablePushMessages) 
                {
                    if (string.IsNullOrEmpty(message.Message))
                        continue;

                    //if (csvData.ServerPushCountryDicData.TryGetValue(message.Language, out var data) == false)
                    //    continue;

                    //if( data.Countrys.Count <= 0 )
                    //    continue;

                    //                    var countrys = CheckNightPushLimit(data.Countrys);
                    //                    var countrys = new List<string>();  

                    //if (countrys.Count <= 0)
                    //    continue;

                    //var tokens = await userCtx.Accounts
                    //    .Where(p => countrys.Contains(p.CountryCode) && string.IsNullOrEmpty(p.PushToken))
                    //    .Select(p => p.PushToken).ToListAsync();

                    var tokens = await userCtx.UserAccounts.Where(p => !string.IsNullOrEmpty(p.PushToken)).Select(p => p.PushToken).ToListAsync();

                    if (tokens.Any() == false)
                        continue;

                    var pushMessage = new PushMessage
                    {
                        Title = message.Title,
                        Message = message.Message,
                        PushTokens = tokens,
                    };

                    await SendPushAsync(pushMessage);

                    message.SendYn = true;
                    message.SendTime = AppClock.UtcNow;
                    userCtx.MessagePushes.Update(message);

                    await userCtx.SaveChangesAsync();
                }
                





                //foreach (var availablePushMessage in availablePushMessages)
                //{
                //    if (string.IsNullOrEmpty(availablePushMessage.Message))
                //        continue;

                //    var titleLanguageText = LanguageTextUtility.Deserialize(availablePushMessage.Title);
                //    if (_languageService.ValidateLanguageTextNoEmpty(titleLanguageText) == false)
                //        continue;

                //    var messageLanguageText = LanguageTextUtility.Deserialize(availablePushMessage.Message);
                //    if (_languageService.ValidateLanguageTextNoEmpty(messageLanguageText) == false)
                //        continue;

                //    var userPushInfos = await _userService.GetTargetUsersAsync(new UserTargetData
                //    {
                //        Type = (UserTargetType)availablePushMessage.TargetType,
                //        Users = availablePushMessage.Targets,
                //    }, p => !string.IsNullOrWhiteSpace(p.PushToken) && p.NoticePush == 1, p => new UserPushInfo
                //    {
                //        PushToken = p.PushToken,
                //        Language = p.Language
                //    });

                //    if (userPushInfos.Count <= 0)
                //    {
                //        _logger.LogInformation("FcmService: SendPush (NoUser) [{Title}, {Message}]", availablePushMessage.Title,
                //            availablePushMessage.Message);

                //        availablePushMessage.SendYn = "y";
                //        availablePushMessage.SendTime = AppClock.UtcNow;
                //        appCtx.Update(availablePushMessage);
                //        await appCtx.SaveChangesAsync();
                //        continue;
                //    }

                //    var messageMap = new Dictionary<byte, PushMessage>();
                //    foreach (var item in titleLanguageText.Items)
                //    {
                //        if (item.IsEnabled)
                //        {
                //            messageMap.Add((byte)item.Type, new PushMessage
                //            {
                //                Title = item.Text,
                //            });
                //        }
                //    }

                //    foreach (var item in messageLanguageText.Items)
                //    {
                //        if (item.IsEnabled)
                //        {
                //            if (messageMap.TryGetValue((byte)item.Type, out var pushMessage) == false)
                //            {
                //                pushMessage = new PushMessage();
                //                messageMap.Add((byte)item.Type, pushMessage);
                //            }

                //            pushMessage.Message = item.Text;
                //        }
                //    }

                //    var defaultPushMessage = messageMap[(byte)SupportLanguageInfo.DefaultLanguageType];
                //    foreach (var info in userPushInfos)
                //    {
                //        if (messageMap.TryGetValue(info.Language, out var pushMessage))
                //        {
                //            pushMessage.PushTokens.Add(info.PushToken);
                //        }
                //        else
                //        {
                //            defaultPushMessage.PushTokens.Add(info.PushToken);
                //        }
                //    }

                //    _logger.LogInformation("FcmService: SendPush [{Title}, {Message}]", availablePushMessage.Title,
                //        availablePushMessage.Message);

                //    foreach (var pushMessage in messageMap.Values)
                //        await SendPushAsync(pushMessage);

                //    availablePushMessage.SendYn = "y";
                //    availablePushMessage.SendTime = AppClock.UtcNow;
                //    userCtx.Update(availablePushMessage);
                //    await userCtx.SaveChangesAsync();
                //}
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "CheckPush failed");
            }
        }

        private async Task<List<MessagePushModel>> LoadAvailablePushAsync()
        {
            using var userCtx = _dbRepo.GetUserDb();

            return await userCtx.MessagePushes.Where(p => p.ReservationTime <= AppClock.UtcNow && p.SendYn == false)
                .OrderBy(p => p.ReservationTime).ToListAsync();
        }

        private async Task SendPushAsync(PushMessage message)
        {            
            while (message.PushTokens.Count > 0)
            {
                int rangeCnt = message.PushTokens.Count >= 500 ? 500 : message.PushTokens.Count;
                var tokens = message.PushTokens.GetRange(0, rangeCnt);
                var multiMessage = new MulticastMessage()
                {
                    Notification = new Notification
                    {
                        Title = message.Title,
                        Body = message.Message,
                    },
                    Tokens = tokens
                };

                await FirebaseMessaging.DefaultInstance.SendMulticastAsync(multiMessage)
                    .ConfigureAwait(false);
                message.PushTokens.RemoveRange(0, rangeCnt);
            }
        }



    }
}
