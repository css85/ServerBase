using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TestConsoleApp.CommandLine;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared;
using TestConsoleApp.User;

namespace TestConsoleApp.ScenarioTest
{
    
    [SuppressMessage("ReSharper", "VSTHRD200")]
    class TestLogin : ScenarioTestBase
    {
        private readonly TestLoginOptions _options;

        private readonly AppContext _appContext;

        public TestLogin(TestLoginOptions options):base(options)
        {
            _options = options;
            _appContext = new AppContext(_options.ApiHost, Services);
            _logger = Services.GetRequiredService<ILogger<TestLogin>>();
        }

        public override async Task RunAsync()
        {
            var sw = new Stopwatch();

            #region 생성
            for (var i = 0; i < UserTaskList.Length; i++)
                UserTaskList[i] = _appContext.CreateUserAsync();
            UserList= await Task.WhenAll(UserTaskList).ConfigureAwait(false);
            #endregion

            while(true)
            {
        
                EndCycle(UserList);
                await Task.Delay(Options.CycleDelayMilliseconds);
            }
        }
    }
}
