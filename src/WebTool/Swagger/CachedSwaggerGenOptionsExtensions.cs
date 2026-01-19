using System;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Xml.XPath;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CachedSwaggerGenOptionsExtensions
    {
        public static IServiceCollection AddCachedSwaggerGen(
            this IServiceCollection services,
            Action<SwaggerGenOptions> setupAction = null)
        {
            services.Replace(ServiceDescriptor.Transient<ISwaggerProvider, Bluegames.Swagger.SwaggerGenerator>());

            if (setupAction != null)
            {
                services.ConfigureSwaggerGen(setupAction);
            }
            return services;
        }

        public static void AddXmlComments(
             this SwaggerGenOptions swaggerGenOptions,
             string filePath,
             bool includeControllerXmlComments = false)
        {
            swaggerGenOptions.AddXmlComments(() => new XPathDocument(filePath), includeControllerXmlComments);
        }

        public static void AddXmlComments(
            this SwaggerGenOptions swaggerGenOptions,
            Func<XPathDocument> xmlDocFactory,
            bool includeControllerXmlComments = false)
        {
            var xmlDoc = xmlDocFactory();
            //swaggerGenOptions.ParameterFilter<Bluegames.Swagger.XmlCommentsParameterFilter>(xmlDoc);
            //swaggerGenOptions.RequestBodyFilter<Bluegames.Swagger.XmlCommentsRequestBodyFilter>(xmlDoc);
            swaggerGenOptions.OperationFilter<Bluegames.Swagger.XmlCommentsOperationFilter>(xmlDoc);
            //swaggerGenOptions.SchemaFilter<Bluegames.Swagger.XmlCommentsSchemaFilter>(xmlDoc);
            
            //if(includeControllerXmlComments)
            //    swaggerGenOptions.DocumentFilter<Bluegames.Swagger.XmlCommentsDocumentFilter>(xmlDoc);
        }
    }
}
