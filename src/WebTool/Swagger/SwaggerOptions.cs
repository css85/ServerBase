using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace Bluegames.Swagger
{
    public class SwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        public void Configure(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", CreateInfoForApiVersion());
        }

        static OpenApiInfo CreateInfoForApiVersion()
        {
            var info = new OpenApiInfo
            {
                Title = "Docs",
                Version = "v1",
                Description = "AuditionM Project",
                TermsOfService = new Uri("https://bluegames.modoo.at/"),
                Contact = new OpenApiContact() { Name = "bluegames", Email = null, Url = new Uri("https://bluegames.modoo.at"), },
            };

            return info;
        }
    }
}
