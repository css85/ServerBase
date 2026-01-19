using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Shared;
using Shared.PacketModel;
using Shared.Server.Extensions;
using Shared.ServerApp.Log.Formatter;
using Shared.Session.Serializer;
using Shared.TcpNetwork.Transport;
using TestConsoleApp.CommandLine;
using TestConsoleApp.User;

namespace TestConsoleApp.ScenarioTest
{
    public class ReportInfo
    {
        private readonly ILogger _logger;
        public string Name { get; } 
        public long Ticks { get; }
        public double Min { get; set; } = double.MaxValue;
        public double Max { get; set; } = double.MinValue;

        public ReportInfo(ILogger logger, string name,long sec)
        {
            Name = name;
            Ticks = sec;
            UpdateTask(sec);
            _logger = logger;
        }
        public void UpdateTask(double sec)
        {
            if (Min > sec)
                Min = sec;
            
            if (Max < sec)
                Max = sec;
        }
        
        public void Print()
        {
            _logger.LogInformation($"Min:{Min:F2}, Max:{Max:F2} \t - {Name} ");
        }
    }
    public class TestReport
    {
        public Dictionary<string, ReportInfo> ResultReports = new();
        private readonly ILogger<TestReport> _logger;

        public TestReport(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TestReport>();
        }
        public void RegisterTask(ReportInfo report)
        {
            if(!ResultReports.ContainsKey(report.Name))
            {
                ResultReports.Add(report.Name,report);
            }
        }

        public void UpdateTask(string name, TimeSpan elaspedTime,TestUserContext[] result)
        {
            var sendCount = result.Sum(p => p.SendCount);
            var receiveCount = result.Sum(p => p.ReceiveCount);
            
            var seconds = elaspedTime.TotalMilliseconds/1000;
            
            _logger.LogInformation($" {seconds:000.##} s | {(sendCount!=0?sendCount/seconds:0):000.##}/s | {(receiveCount!=0?receiveCount/seconds:0):000.##}/s | \t{name} ( S:{sendCount} R:{receiveCount} )");
            
            ResultReports[name].UpdateTask(seconds);
        }

        public void PrintResult()
        {
            foreach (var report in ResultReports)
            {
                report.Value.Print();
            }
        }
    }

    public abstract class ScenarioTestBase : IScenarioTest
    {
        private readonly Stopwatch _stopWatch = new();

        private int _currentCycleCount; 
        protected IServiceProvider Services { get; private set; }
        public Stopwatch StopWatch => _stopWatch;
        public readonly Stopwatch Stopwatch2 = new();
        public ILoggerFactory LoggerFactory { get; private set; }
        protected ILogger _logger;
        public TestUserContext[] UserList { get; set; }
        public Task<TestUserContext>[] UserTaskList { get; set; }
        
        protected readonly TestReport Report;
        protected readonly TestOptionsBase Options;

        protected readonly AppContext AppContext;
        private ParallelOptions _defaultScheduler = new()
        {
            MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 2.0))
        };
        public ITcpSetting TcpSettings { get; private set; }

        protected ScenarioTestBase(TestOptionsBase options) :this()
        {
            Report = new TestReport(LoggerFactory);
            Options = options;
            UserTaskList = new Task<TestUserContext>[options.UserCount];

            AppContext = new AppContext(Options.ApiHost, Services);
        }
        private ScenarioTestBase()
        {
            Build();
        }
        private void Build()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();
            LoggerFactory = Services.GetRequiredService<ILoggerFactory>();
            TcpSettings = Services.GetRequiredService<ITcpSetting>();
        }
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogging(options =>
                {
                    options.ClearProviders();
                    options
                        .AddConsoleFormatter<CustomPrefixFormatter, CustomPrefixFormatterOptions>(p =>
                        {
                            p.ColorBehavior = LoggerColorBehavior.Disabled;
                            p.SingleLine = true;
                            p.IncludeScopes = false;
                            p.TimestampFormat = "hh:mm:ss.ffff ";
                        }).AddConsole(p=>p.FormatterName = nameof(CustomPrefixFormatter));

                })
                .AddSingleton<ITcpSetting,TcpEncryptConnectionSettings>(p=>new TcpEncryptConnectionSettings
                {
                    PacketSerializer = new JsonPacketSerializerEncrypt(JsonPacketSerializerEncryptOptions.Default) 
                })
                .AddHttpClient();
        }

        private void LogText(string key,params object[] messages)
        {
            _logger.LogInformation(key,messages);    
        }


        protected Task ExecuteBatchingAsync<T>(
            string batchingName,
            [NotNull] TestUserContext[] userContexts,
            Func<TestUserContext, T, Task> func) where T : new()
        {
            return ExecuteBatchingAsync(batchingName, userContexts, async p =>
            {
                var userData = p.GetData<T>();
                if (userData == null)
                {
                    userData = new T();
                    p.SetData(userData);
                }
                await func(p, userData);
                return p;
            });
        }

        protected async Task ExecuteBatchingAsync(
            string batchingName,
            [NotNull]TestUserContext[] userContexts,
            Func<TestUserContext, Task<TestUserContext>> func,
            int batchIntervalValue = 0)
        {
            var loop =(userContexts.Length / Options.BatchProcessingCount);
            var remain = (userContexts.Length % Options.BatchProcessingCount);

            if(loop == 0)
            {
                DiagnosisStart();
                await ParallelBatchingAsync(userContexts[..], func).ConfigureAwait(false);
                DiagnosisEnd();

                LogSummaryResults($"{batchingName} [{0}-{userContexts.Length - 1}]", userContexts);
                ClearLogs(userContexts);
            }
            else
            {
                for (int i = 0; i < loop; i++)
                {
                    int start = i * Options.BatchProcessingCount;
                    int end = start + Options.BatchProcessingCount;

                    if (i == loop - 1)
                        end = remain == 0 ? end : end + remain;

                    DiagnosisStart();
                    await ParallelBatchingAsync(userContexts[start..end], func).ConfigureAwait(false);
                    DiagnosisEnd();

                    if (batchIntervalValue > 0)
                    {
                        await DelayStepAsync(batchIntervalValue).ConfigureAwait(false);
                    }
                    else
                    {
                        await DelayStepAsync(Options.BatchIntervalMilliseconds).ConfigureAwait(false);
                    }

                    LogSummaryResults($"{batchingName} [{start}-{end - 1}]", userContexts);
                    ClearLogs(userContexts);
                }
            }
        }
        
        private async Task ParallelBatchingAsync(TestUserContext[] source, Func<TestUserContext,Task<TestUserContext>> executorAsync)
        {
            var requests = new List<Task<TestUserContext>>(source.Length);
            
            Parallel.ForEach(source, new ParallelOptions
                {
                    MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 2.0))
                },
                task =>
                {
                    requests.Add(executorAsync(task));
                });
             
            await Task.WhenAll(requests);
        }

        private Task DelayStepAsync(int delayMs = 1000)
        {
            return Task.Delay(delayMs);
        }
        public void LogTitle(string key, double elapsedSec)
        {
            LogText(key, elapsedSec);
        }

        public void LogSummaryResults(string title ,TestUserContext[] users)
        {
            var elapsedSec = StopWatch.Elapsed.TotalSeconds;
            var sendCount = users.Sum(x => x.SendCount);
            var recvCount = users.Sum(x => x.ReceiveCount);
            
            LogText("{ElapsedSec:F3} | S:{Send}, R:{Recv} AVG:{Avg:F3} / sec\t\t| {Title} ",
                elapsedSec, sendCount,recvCount,recvCount / elapsedSec,title);
            
        }

        protected void DiagnosisStart()
        {
            StopWatch.Restart();
        }

        protected void DiagnosisEnd()
        {
            StopWatch.Stop();
        }


        protected void DiagnosisStart2()
        {
            Stopwatch2.Restart();
        }

        protected void DiagnosisEnd2()
        {
            Stopwatch2.Stop();
        }
        protected void EndCycle(TestUserContext[] users)
        {
            foreach (var userContext in users)
            {
                userContext.CloseAllConnections();
            }
            
            _logger.LogInformation("== CycleCount == {CycleCount}", ++_currentCycleCount);
        }

        public void ClearResults()
        {
            ClearLogs(UserList);
        }
        private static void ClearLogs(TestUserContext[] users)
        {
            if (users== null || users.Length < 1)
                return;
            users.AsParallel().ForAll(u => u.ClearLogs());
        }

        protected async Task StartCycleAsync(Func<Task> task)
        {
            while (true)
            {
                await task();

                _logger.LogInformation("== CycleCount == {CycleCount}", ++_currentCycleCount);
                await Task.Delay(Options.CycleDelayMilliseconds);
            }
        }

        public abstract Task RunAsync();

        public async Task<TestUserContext[]> CreateUsersAsync()
        {
            var userTasks = new Task<TestUserContext>[Options.UserCount];
            for (var i = 0; i < userTasks.Length; i++)
                userTasks[i] = AppContext.CreateUserAsync();
            return await Task.WhenAll(userTasks).ConfigureAwait(false);
        }

        public void CloseUsers(TestUserContext[] users)
        {
            foreach (var user in users)
                user.CloseAllConnections();
        }

        public async Task<TestUserContext[]> LoginProcess(TestUserContext[] users)
        {

            #region Gate 연결
            {
                await ExecuteBatchingAsync("Gate 연결", users, TestHttpConnectAsync);
            }
            #endregion

            #region 서버 리스트 가져오기
            {
                await ExecuteBatchingAsync("서버 리스트 가져오기", users, TestGetServerAsync);
            }
            #endregion

            return users;
        }

        private void LogSecretKey(string msg, NetServiceType serviceType, TestUserContext userContext)
        {
         
            _logger.LogInformation("{Title}|{ServiceType}|{UserSeq} |{GateKey}",
                msg,
                serviceType,
                userContext.UserSeq,
                userContext.GateSecretKey);
        }


        protected async Task<TestUserContext> TestHttpConnectAsync(TestUserContext testUser)
        {
            var ret = await testUser.ConnectAsync(new[] { NetServiceType.Gate }, Options.ApiHost,null);
            return ret ? testUser : null;
        }

        protected async Task<TestUserContext> TestGetServerAsync(TestUserContext testUser)
        {
            await testUser.SendAsync<GetServerInfosRes>(new GetServerInfosReq
            {
                OsType = OSType.Android,
                AppVer = 1
            });
            return testUser;
        }


        protected void RegisterReport(string name)
        {
            Report.RegisterTask(new ReportInfo(_logger,name,0));
        }

        protected void UpdateReport(string name)
        {
            Report.UpdateTask(name, Stopwatch2.Elapsed,UserList);
        }
    }
}