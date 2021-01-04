using GoofyAlgoTrader.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.WebApi.Filter
{
    public class GlobalActionFilter : ActionFilterAttribute
    {
        private readonly ILogger _log;
        public GlobalActionFilter()
        {
            _log = Log.GetLogger("Operation");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var method = context.HttpContext.Request.Method;
            var url = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString.Value;
            var ip = context.HttpContext.GetIpAddress();
            var ua = context.HttpContext.Request.Headers["User-Agent"];
            var referer = context.HttpContext.Request.Headers["Referer"];

            //var mobile = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "mobile")?.Value;
            var token = "";
            if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues authorization))
            {
                var bearer = authorization.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(bearer) && bearer.Contains("bearer ", StringComparison.InvariantCultureIgnoreCase))
                    token = bearer.Substring(7);
            }


            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"[Url]: {url}");
            sb.AppendLine($"[Method]: {method}");
            sb.AppendLine($"[IP]: {ip}");
            sb.AppendLine($"[UserAgent]: {ua}");
            sb.AppendLine($"[Referer]: {referer}");
            //sb.AppendLine($"[Mobile]: {mobile}");
            sb.AppendLine($"[Token]: {token}");

            _log.Info(sb.ToString());

            base.OnActionExecuting(context);
        }
    }
}
