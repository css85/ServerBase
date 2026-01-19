using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Shared.Server.Define;
using Shared.Session.Models;

namespace Shared.ServerApp.SignalR
{
    [Authorize(AuthenticationSchemes = DefaultSchemes)]
    public class TokenBasedHub : Hub
    {
        private const string DefaultSchemes = DefineConsts.JwtAuthenticationScheme;

        [NonAction]
        protected long GetUserSeq()
        {
            if (long.TryParse(Context.User.FindFirstValue(nameof(JwtPayload.Seq)), out var userSeq) == false)
                throw new Exception("Invalid Token Seq.");

            if (userSeq == 0)
                throw new Exception("Invalid Token Seq.");

            return userSeq;
        }
        

        [NonAction]
        protected byte GetUserOsType()
        {
            if (byte.TryParse(Context.User.FindFirstValue(nameof(JwtPayload.OsType)), out var osType) == false)
                throw new Exception("Invalid Token OsType.");

            return osType;
        }

    }
}