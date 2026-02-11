using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Contracts.DTO
{
    public class AuthResponseDTO
    {
        public string Token { get; set; } = null!;
        public DateTime Expiry { get; set; }
    }
}
