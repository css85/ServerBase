using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Shared.ServerApp.Extensions;
using WebTool.Config;
using WebTool.Connection;

namespace WebTool
{
    public class Startup
    {
        public static string GetXmlCommentsFilePath(string fileName)
        {
            return Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, fileName);
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IocConfig.Configure(services, Configuration, Environment);
            services.AddWebTool();
            services.AddHostedService<WebToolAppHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();         //Asp.Net core�� SDK�� ��ġ�Ͽ����� Ȯ��. ��ġ�Ǿ����� �ʴٸ� ��ġ �� �籸��
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
            app.UseAppHealthCheck();
        }
    }
}