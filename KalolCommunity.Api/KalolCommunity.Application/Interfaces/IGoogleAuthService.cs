using KalolCommunity.Contracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserInfoDto> ValidateTokenAsync(string idToken);
    }
}
