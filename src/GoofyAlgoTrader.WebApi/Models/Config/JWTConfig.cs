using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.WebApi.Models.Config
{
    public class JWTConfig
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string IssuerSigningKey { get; set; }
        public int AccessTokenExpiresMinutes { get; set; }
        public int RefreshTokenExpiresDays { get; set; }
    }
}
