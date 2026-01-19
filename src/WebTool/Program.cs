using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Shared.ServerApp.Extensions;
using WebTool.Config;

namespace WebTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((host, config) =>
                {
                    var envName = host.HostingEnvironment.EnvironmentName;

                    config.AddJsonFile("Settings/dbsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile($"Settings/dbsettings.{envName}.json", optional: true,
                        reloadOnChange: true);

                    var envSubNamePos = envName.IndexOf('-');
                    if(envSubNamePos >0 )
                    {
                        var envSubName = envName.Substring(0, envSubNamePos);
                        config.AddJsonFile($"Settings/dbsettings.{envSubName}.json", optional: true,
                            reloadOnChange: true);
                    }
                })
                .UseSerilog()
                .ConfigureLogging((con, ctx) =>
                {
                    ctx.ClearProviders();
                    Log.Logger = con.CreateServerLogger<WebToolAppSettings>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureKestrel(options =>
                        {
                            options.AddServerHeader = false;
                            options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(30);
                        })
                        .UseKestrel();
                    webBuilder.UseStartup<Startup>();
                });
    }
}