using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Shared.Packet;
using Shared.Session.Models;

namespace Shared.ServerApp.Mvc
{
    public class ErrorLogFilterAttribute : ActionFilterAttribute
    {
        private readonly ILogger<ErrorLogFilterAttribute>  _logger;

        public ErrorLogFilterAttribute(ILogger<ErrorLogFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            await next();
            return;
            //var result = context.HttpContext.Response.Headers["ret"].ToString();
            //if ((int.TryParse(result, out var retCode) ? retCode : -102) >= 0)
            //    return;
            
            //var userSeq = context.HttpContext.User.FindFirstValue(nameof(JwtPayload.Seq));

            //_logger.LogWarning("{ResultCode}: {Action} :{UserSeq}",
            //    context.ActionDescriptor.DisplayName,
            //    (ResultCode)retCode,
            //    userSeq
            //    );
            
        }
    }
}