using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using Common.Config;
using FrontEndWeb.Connection;
using Shared.CdnStore;
using FrontEndWeb.Config;
using Microsoft.Extensions.FileProviders;
using FrontEndWeb.Services;
using Shared.ServerApp.Extensions;
using FrontEndWeb.SignalR;
using Microsoft.AspNetCore.Http;
using Shared.ServerApp.Middleware;

namespace FrontEndWeb
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            IocConfig.AddAllServices(services, Configuration, Environment);

            services.AddFrontendServer();
            services.AddHostedService<FrontEndAppHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
//            app.UseAllElasticApm<FrontEndAppSettings>(Configuration);

            app.UseDeveloperExceptionPage();

            // LocalCdnStoreService
            var appSettings = app.ApplicationServices.GetRequiredService<ChangeableSettings<FrontEndAppSettings>>();
            if (appSettings.Value.CdnStoreType == CdnStoreType.Local)
            {
                var cdnFilesPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, "cdnfiles/profiles/"));
                Directory.CreateDirectory(cdnFilesPath);
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(cdnFilesPath),
                    RequestPath = "/cdnfiles/profiles",
                    ServeUnknownFileTypes = true,
                    DefaultContentType = "text/plain",
                });
            }
            //app.Use(async (context, next) => {
            //    context.Request.EnableBuffering();
            //    await next();
            //});
            app.UseRouting();
            app.UseForwardedHeaders();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<FrontInfoHub>("/FrontInfoHub");
            });
            app.UseAppHealthCheck();
        }
    }
}
