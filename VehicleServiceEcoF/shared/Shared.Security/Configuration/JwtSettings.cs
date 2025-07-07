using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Security.Configuration
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public int ExpiryInHours { get; set; } = 24;
        public int ExpirationInMinutes { get; set; } = 1440; // 24 horas por defecto
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}
