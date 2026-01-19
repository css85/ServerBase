using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Shared.ServerApp.Extensions;

namespace Benchmark
{
    public static class ServiceUtility
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public static void Initialize(Action<ServiceCollection> collections=null)
        {
            var serviceCollections = RegisterServices();
            collections?.Invoke(serviceCollections);
            ServiceProvider = serviceCollections.BuildServiceProvider();
        }

        private static ServiceCollection RegisterServices()
        {
            var configuration = SetupConfiguration();
            var serviceCollection = new ServiceCollection();
            
            serviceCollection.AddLogging(cfg => cfg.AddConsole());
            serviceCollection.AddSingleton(configuration);
            var hostingEnvironment = new HostingEnvironment {EnvironmentName = "Benchmark"};
            serviceCollection.AddSingleton<IHostEnvironment>(hostingEnvironment);

            serviceCollection.AddServerApp(configuration, hostingEnvironment);
            
            return serviceCollection;
        }

        private static IConfiguration SetupConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
        }
    }
}