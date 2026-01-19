using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Shared.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shared.Packet.Utility;

namespace Bluegames.Swagger
{
    public class ApiDescriptionFactory
    {
        public static ApiDescription CreateFromPacketType(
               PacketHeaderData headerData,
               string groupName = "doc",
               string httpMethod = "POST",
               string relativePath = "resoure",
               IEnumerable<ApiParameterDescription> parameterDescriptions = null,
               IEnumerable<ApiRequestFormat> supportedRequestFormats = null,
               IEnumerable<ApiResponseType> supportedResponseTypes = null
            )
        {
            var routeValues = new Dictionary<string, string>
            {
                ["controller"] = ((MAJOR)headerData.Header.Major).ToString()
            };

            var apiDescription = new ApiDescription
            {
                ActionDescriptor = new ActionDescriptor
                { 
                    DisplayName = $"{headerData.Header}",
                    RouteValues = routeValues,
                },
                GroupName = groupName,
                HttpMethod = httpMethod,
                RelativePath = $"{headerData.Header.Major:D2}.{headerData.Header.Minor:D2} {headerData.Header}",
            };

            if (parameterDescriptions != null)
            { 
                foreach (var parameter in parameterDescriptions)
                {
                    apiDescription.ParameterDescriptions.Add(parameter);
                }
            }

            if (supportedRequestFormats != null)
            {
                foreach (var requestFormat in supportedRequestFormats)
                {
                    apiDescription.SupportedRequestFormats.Add(requestFormat);
                }
            }

            if (supportedResponseTypes != null)
            {
                foreach (var responseType in supportedResponseTypes)
                {
                    apiDescription.SupportedResponseTypes.Add(responseType);
                }
            }

            return apiDescription;
        }

         public static ApiDescription Create(
                MethodInfo methodInfo,
                string groupName = "v1",
                string httpMethod = "POST",
                string relativePath = "resoure",
                IEnumerable<ApiParameterDescription> parameterDescriptions = null,
                IEnumerable<ApiRequestFormat> supportedRequestFormats = null,
                IEnumerable<ApiResponseType> supportedResponseTypes = null)
            {
                var actionDescriptor = CreateActionDescriptor(methodInfo);

                var apiDescription = new ApiDescription
                {
                    ActionDescriptor = actionDescriptor,
                    GroupName = groupName,
                    HttpMethod = httpMethod,
                    RelativePath = relativePath,
                };

                if (parameterDescriptions != null)
                {
                    foreach (var parameter in parameterDescriptions)
                    {
                        //var controllerParameterDescriptor = actionDescriptor.Parameters
                        //    .OfType<ControllerParameterDescriptor>()
                        //    .FirstOrDefault(parameterDescriptor => parameterDescriptor.Name == parameter.Name);
                        //if (controllerParameterDescriptor != null)
                        //{
                        //    parameter.ParameterDescriptor = controllerParameterDescriptor;
                        //}
                        apiDescription.ParameterDescriptions.Add(parameter);
                    }
                }

                if (supportedRequestFormats != null)
                {
                    foreach (var requestFormat in supportedRequestFormats)
                    {
                        apiDescription.SupportedRequestFormats.Add(requestFormat);
                    }
                }

                if (supportedResponseTypes != null)
                {
                    foreach (var responseType in supportedResponseTypes)
                    {
                        if (methodInfo.ReturnType != null)
                        {

                        }

                        apiDescription.SupportedResponseTypes.Add(responseType);
                    }
                }

                return apiDescription;
            }

            public static ApiDescription Create<TController>(
                Func<TController, string> actionNameSelector,
                string groupName = "v1",
                string httpMethod = "POST",
                string relativePath = "resoure",
                IEnumerable<ApiParameterDescription> parameterDescriptions = null,
                IEnumerable<ApiRequestFormat> supportedRequestFormats = null,
                IEnumerable<ApiResponseType> supportedResponseTypes = null)
                where TController : new()
            {
                var methodInfo = typeof(TController).GetMethod(actionNameSelector(new TController()));

                return Create(
                    methodInfo,
                    groupName,
                    httpMethod,
                    relativePath,
                    parameterDescriptions,
                    supportedRequestFormats,
                    supportedResponseTypes
                );
            }
            
            private static ActionDescriptor CreateActionDescriptor(MethodInfo methodInfo)
            {
                var httpMethodAttribute = methodInfo.GetCustomAttribute<HttpMethodAttribute>();
                var attributeRouteInfo = (httpMethodAttribute != null)
                    ? new AttributeRouteInfo { Template = httpMethodAttribute.Template, Name = httpMethodAttribute.Name }
                    : null;

                var parameterDescriptors = methodInfo.GetParameters()
                    .Select(CreateParameterDescriptor)
                    .ToList();

                var routeValues = new Dictionary<string, string>
                {
                    ["controller"] = methodInfo.DeclaringType.Name
                };

                return new ControllerActionDescriptor
                {
                    AttributeRouteInfo = attributeRouteInfo,
                    ControllerTypeInfo = methodInfo.DeclaringType.GetTypeInfo(),
                    ControllerName = methodInfo.DeclaringType.Name,
                    MethodInfo = methodInfo,
                    Parameters = parameterDescriptors,
                    RouteValues = routeValues

                };
            }

            private static ParameterDescriptor CreateParameterDescriptor(ParameterInfo parameterInfo)
            {
                return new ControllerParameterDescriptor
                {
                    Name = parameterInfo.Name,
                    ParameterInfo = parameterInfo,
                    ParameterType = parameterInfo.ParameterType,
                };
            }
        }
}
