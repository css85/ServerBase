using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace Bluegames.Swagger
{
    public class SwaggerGeneratorOptions : IConfigureOptions<SwaggerGenOptions>
    {
        public void Configure(SwaggerGenOptions options)
        {
            options.SwaggerDoc("doc", CreateInfoForApiVersion(new ApiVersionDescription(new ApiVersion(1,0),"v1",false)));
        }
      
        static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Title = "PacketDocs",
                Version = description.ApiVersion.ToString(),
                Description = "AuditionM Project",
                TermsOfService = new Uri("https://tigerclaw.modoo.at/"),
                Contact = new OpenApiContact() { Name = "tigerclaw", Email = null, Url = new Uri("https://tigerclaw.modoo.at"), },
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
    }
}
