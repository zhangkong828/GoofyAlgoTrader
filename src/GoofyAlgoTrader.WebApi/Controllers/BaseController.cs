using GoofyAlgoTrader.WebApi.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    [TypeFilter(typeof(GlobalExceptionFilter))]
    [TypeFilter(typeof(GlobalActionFilter))]
    public class BaseController : ControllerBase
    {
        [NonAction]
        protected long GetUserId()
        {
            var uId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value;
            long.TryParse(uId, out long userId);
            return userId;
        }

        [NonAction]
        protected string GetUserName()
        {
            return User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        }
    }
}
