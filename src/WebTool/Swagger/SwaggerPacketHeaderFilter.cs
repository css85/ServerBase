using Microsoft.OpenApi.Models;
using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Shared.Packet;
using System.Collections;
using System.Collections.Generic;

namespace Bluegames.Swagger
{
    public class SwaggerPacketHeaderExtendFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attr  = context.MethodInfo?.GetCustomAttribute<SwaggerPacketHeaderAttribute>(false);
            if (attr == null) 
                return;
         
            operation.Description = $"{attr.ServiceTypeInfo.ProtocolType}://{attr.Header} / {attr.ServiceTypeInfo.NetServiceType}";
            operation.Tags[0].Name = ((MAJOR)attr.Header.Major).ToString();
        }
    }
}
