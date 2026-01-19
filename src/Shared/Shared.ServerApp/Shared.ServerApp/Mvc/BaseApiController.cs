using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Shared.Model;
using Shared.Packet;

namespace Shared.ServerApp.Mvc
{
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        [NonAction]
        protected OkObjectResult Ok<T>(int retCode, [ActionResultObjectValue] T value) where T : ResponseBase, new()
        {
            if (Response.StatusCode == 200)
            {
                if (retCode == (int) ResultCode.Success)
                {
                    Response.Headers["ret"] = "0";
                    value ??= new T();
                }
                else
                {
                    Response.Headers["ret"] = retCode.ToString();
                }
            }
            else
            {
                Response.Headers["ret"] = "-102";
            }

            return Ok(value);
        }
        
        [NonAction]
        protected OkObjectResult Ok<T>(ResultCode retCode, T value) where T : ResponseBase, new()
        {
            return Ok((int) retCode, value);
        }

        [NonAction]
        protected OkObjectResult Ok<T>(ResultCode retCode) where T : ResponseBase, new()
        {
            return Ok((int) retCode, new T());
        }

        [NonAction]
        protected OkObjectResult Ok<T>(int retCode) where T : ResponseBase, new()
        {
            return Ok(retCode, new T());
        }

        [NonAction]
        protected string GetIP()
        {   
            var ip = Request.Headers["x-forwarded-for"].ToString();

            if (string.IsNullOrEmpty(ip))
            {
                ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
            }

            return ip;
        }
    }
}
