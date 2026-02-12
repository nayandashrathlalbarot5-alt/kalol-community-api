using KalolCommunity.Application.Interfaces;
using KalolCommunity.Application.Services;
using KalolCommunity.Contracts.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace KalolCommunity.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            var result = await _authService.RegisterAsync(registerDTO);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var result = await _authService.LoginAsync(loginDTO);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("refreshtoken")]
        public async Task<IActionResult> RefreshToekn(RefreshTokenRequestDTO refreshTokenDTO)
        {
            var result = await _authService.RefreshTokenAsync(refreshTokenDTO);
            return StatusCode(result.StatusCode, result);
        }

        //[HttpPost("google-login")]
        //public async Task<IActionResult> GoogleLogin(GoogleLoginRequestDTO request)
        //{
        //    var googleUser = await _googleAuthService.ValidateTokenAsync(request.IdToken);

        //    // 1. Check if user exists
        //    var user = await _unitOfWork.Users
        //        .GetByEmailAsync(googleUser.Email);

        //    // 2. If not exists → auto-register
        //    if (user == null)
        //    {
        //        user = new User
        //        {
        //            Email = googleUser.Email,
        //            //FullName = googleUser.FullName,
        //            GoogleId = googleUser.GoogleId,
        //            IsGoogleUser = true,
        //            CreatedAt = DateTime.UtcNow
        //        };

        //        await _unitOfWork.Users.AddAsync(user);
        //        await _unitOfWork.SaveChangesAsync();
        //    }

        //    // 3. Generate JWT (same as normal login)
        //    var token = _jwtTokenService.GenerateAccessToken(user);

        //    return Ok(new
        //    {
        //        token,
        //        user.Email
        //        //user.FullName
        //    });
        //}
    }
}
