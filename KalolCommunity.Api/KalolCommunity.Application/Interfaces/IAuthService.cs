using KalolCommunity.Contracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDTO>> RegisterAsync(RegisterDTO dto);
        Task<ApiResponse<AuthResponseDTO>> LoginAsync(LoginDTO dto);
        Task<ApiResponse<AuthResponseDTO>> GoogleLoginAsync(string idToken);
        Task<ApiResponse<AuthResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO refreshTokenRequestDTO);
        Task<ApiResponse<AuthResponseDTO>> SendOtpAsync(string email, string flag);
        Task<ApiResponse<AuthResponseDTO>> VerifyOtpAsync(RegisterDTO request);
        Task<string?> GetOtpAsync(string email);
        Task DeleteOtpAsync(string email);
    }
}
