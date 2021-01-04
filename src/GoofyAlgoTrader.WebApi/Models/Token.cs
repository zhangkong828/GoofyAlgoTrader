using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.WebApi.Models
{
    public class Token
    {
        public string AccessToken { get; set; }
        public long AccessTokenExpires { get; set; }
        public string RefreshToken { get; set; }
        public long RefreshTokenExpires { get; set; }
    }
}
