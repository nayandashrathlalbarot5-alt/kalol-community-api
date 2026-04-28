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
        public string RefreshToken { get; set; } = null!;
        public Guid UserId { get; set; }
        public DateTime Expiry { get; set; }
        
        // Google profile information (populated when signing in with Google)
        public GoogleUserInfoDto GoogleUserInfo { get; set; } = null!;
    }
}
