using KalolCommunity.Application.Interfaces;
using KalolCommunity.Contracts.DTO;
using KalolCommunity.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using KalolCommunity.Application.Common;

namespace KalolCommunity.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtService;
        private readonly IGoogleAuthService _googleAuthService;

        public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtService,
        IGoogleAuthService googleAuthService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _googleAuthService = googleAuthService;
        }

        public async Task<ApiResponse<AuthResponseDTO>> RegisterAsync(RegisterDTO dto)
        {
            // 1️ Check if email exists
            if (await _unitOfWork.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return new ApiResponse<AuthResponseDTO>
                {
                    Success = false,
                    Message = ResponseMessages.EmailAlreadyRegistered,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null
                };
            }

            // 2️ Create user
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = _passwordHasher.Hash(dto.Password)
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // 3️ Generate JWT
            var token = _jwtService.GenerateAccessToken(user);

            // 4️ Return wrapped response
            return new ApiResponse<AuthResponseDTO>
            {
                Success = true,
                Message = ResponseMessages.RegistrationSuccessful,
                StatusCode = (int)HttpStatusCode.Created,
                Data = new AuthResponseDTO
                {
                    Token = token,
                    Expiry = DateTime.UtcNow.AddHours(1)
                }
            };
        }

        public async Task<ApiResponse<AuthResponseDTO>> LoginAsync(LoginDTO dto)
        {
            // 1️ Find user
            var user = await _unitOfWork.Users.GetAsync(u => u.Email == dto.Email);

            if (user == null || user.PasswordHash == null)
                return new ApiResponse<AuthResponseDTO>
                {
                    Success = false,
                    Message = ResponseMessages.InvalidCredentials,
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Data = null
                };

            // 2️ Verify password
            var isValidPassword = _passwordHasher.Verify(user.PasswordHash, dto.Password);

            if (!isValidPassword)
                return new ApiResponse<AuthResponseDTO>
                {
                    Success = false,
                    Message = ResponseMessages.InvalidCredentials,
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Data = null
                };

            // 3️ Generate JWT
            var token = _jwtService.GenerateAccessToken(user);

            // 4️ Return wrapped response
            return new ApiResponse<AuthResponseDTO>
            {
                Success = true,
                Message = ResponseMessages.LoginSuccessful,
                StatusCode = (int)HttpStatusCode.OK,
                Data = new AuthResponseDTO
                {
                    Token = token,
                    Expiry = DateTime.UtcNow.AddHours(1)
                }
            };
        }

        public Task<ApiResponse<AuthResponseDTO>> GoogleLoginAsync(string idToken)
        {
            throw new NotImplementedException();
        }
    }
}
