using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.WebApi
{
    public static class HttpContextExtension
    {
        public static string GetIpAddress(this HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress.ToString();
            }

            if (ip == "::1")
            {
                ip = "localhost";
            }
            return ip;
        }
    }
}
