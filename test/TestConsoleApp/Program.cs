using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Serilog.Core;
using TestConsoleApp.CommandLine;
using TestConsoleApp.ScenarioTest;

namespace TestConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var optionTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(p => p.IsClass && p.BaseType == typeof(TestOptionsBase)).ToArray();

#pragma warning disable VSTHRD101
            Parser.Default
                .ParseArguments(args, optionTypes)
                .WithParsed(async (TestLoginOptions o) =>
                {
                    await RunTestAsync(new TestLogin(o));
                });
#pragma warning restore VSTHRD101

            while (true)
            {
                await Task.Delay(1000);
            }
        }

        private static async Task RunTestAsync<T>(T test) where T : IScenarioTest
        {
            var sw = new Stopwatch();
            sw.Start();
            await test.RunAsync();
            sw.Stop();
            Logger.None.Information($"{typeof(T)}: {sw.Elapsed}");
        }
    }
}
