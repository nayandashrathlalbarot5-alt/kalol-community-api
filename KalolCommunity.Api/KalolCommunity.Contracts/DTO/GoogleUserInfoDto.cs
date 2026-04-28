using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Contracts.DTO
{
    public class GoogleUserInfoDto
    {
        public string GoogleId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PictureUrl { get; set; } = string.Empty;
    }
}
