using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Google.Apis.Auth;
using KalolCommunity.Application.Common;
using KalolCommunity.Application.Interfaces;
using KalolCommunity.Application.Exceptions;
using KalolCommunity.Contracts.DTO;
using KalolCommunity.Domain.Entities;


namespace KalolCommunity.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponseDTO>> RegisterAsync(RegisterDTO dto)
        {
            _logger.LogInformation("Register attempt for email: {Email}", dto.Email);

            // 1️ Check if email exists
            if (await _unitOfWork.Users.AnyAsync(u => u.Email == dto.Email))
            {
                throw new ConflictException(ResponseMessages.EmailAlreadyRegistered);
            }

            // 2️ Create user
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = _passwordHasher.Hash(dto.Password)
            };

            // 3 Save User to Databse
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // 4 Generate JWT
            var token = _jwtService.GenerateAccessToken(user);

            // 5 Generate Refresh Token
            var refreshToken = _jwtService.GenerateRefreshToken();

            // 6 Hash the refresh token before storing
            var hashedRefreshToken = _jwtService.HashToken(refreshToken);

            // 7 Create RefreshToken entity
            var refreshTokenEntity = new RefreshToken
            {
                TokenHash = hashedRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = user.UserId,
                CreatedAt = DateTime.UtcNow,
            };

            // 8 Save RefreshToken to Databse
            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User registered successfully. UserId: {UserId}", user.UserId);

            // 9 Return wrapped response
            return new ApiResponse<AuthResponseDTO>
            {
                Success = true,
                Message = ResponseMessages.RegistrationSuccessful,
                StatusCode = (int)HttpStatusCode.Created,
                Data = new AuthResponseDTO
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    UserId = user.UserId,
                    Expiry = DateTime.UtcNow.AddHours(1)
                }
            };
        }

        public async Task<ApiResponse<AuthResponseDTO>> LoginAsync(LoginDTO dto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            // 1️ Find user
            var user = await _unitOfWork.Users.GetAsync(u => u.Email == dto.Email);

            if (user == null || user.PasswordHash == null)
                throw new UnauthorizedException(ResponseMessages.InvalidCredentials);

            // 2️ Verify password
            var isValidPassword = _passwordHasher.Verify(user.PasswordHash, dto.Password);

            if (!isValidPassword)
                throw new UnauthorizedException(ResponseMessages.InvalidCredentials);

            // 3️ Generate JWT
            var token = _jwtService.GenerateAccessToken(user);

            // 4 Generate Refresh Token
            var refreshToken = _jwtService.GenerateRefreshToken();

            _logger.LogInformation("Login successful. UserId: {UserId}", user.UserId);

            // 5 Return wrapped response
            return new ApiResponse<AuthResponseDTO>
            {
                Success = true,
                Message = ResponseMessages.LoginSuccessful,
                StatusCode = (int)HttpStatusCode.OK,
                Data = new AuthResponseDTO
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    UserId = user.UserId,
                    Expiry = DateTime.UtcNow.AddHours(1)                    
                }
            };
        }

        public async Task<ApiResponse<AuthResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO refreshTokenRequestDTO)
        {
            _logger.LogInformation("Refresh token attempt for user: {UserId}", refreshTokenRequestDTO.UserId);

            // 1️ Hash the incoming refresh token to compare with stored hash
            var hashedToken = _jwtService.HashToken(refreshTokenRequestDTO.RefreshToken);

            // 2️ Parse user id from DTO (DTO uses string for user id)
            if (!Guid.TryParse(refreshTokenRequestDTO.UserId, out var userGuid))
            {
                throw new BadRequestException(ResponseMessages.InvalidUserId);
            }

            // 3️ Retrieve the refresh token from the database matching the hashed token and user id
            var storedRefreshToken = await _unitOfWork.RefreshTokens.GetAsync(
                 rt => rt.TokenHash == hashedToken && rt.UserId == userGuid);

            if (storedRefreshToken == null)
            {
                throw new UnauthorizedException(ResponseMessages.RefreshTokenNotFound);
            }

            // 4️ Load the user and attach to the refresh token
            var user = await _unitOfWork.Users.GetAsync(u => u.UserId == storedRefreshToken.UserId);
            if (user == null)
            {
                throw new NotFoundException(ResponseMessages.UserNotFound);
            }

            storedRefreshToken.User = user;

            // 5️ Check token validity
            if (storedRefreshToken.IsRevoked || storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                throw new UnauthorizedException(ResponseMessages.RefreshTokenExpiredOrRevoked);
            }

            // 6️ Generate new tokens and update stored refresh token
            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var newHashedRefreshToken = _jwtService.HashToken(newRefreshToken);

            // 7 Revoke the old refresh token and create a new one
            storedRefreshToken.IsRevoked = true;
            storedRefreshToken.RevokedAt = DateTime.UtcNow;
            await _unitOfWork.RefreshTokens.UpdateAsync(storedRefreshToken);

            // 8 Create new token row
            var newRefreshTokenEntity = new RefreshToken
            {
                TokenHash = newHashedRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = storedRefreshToken.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Refresh token successful for UserId: {UserId}", storedRefreshToken.UserId);

            return new ApiResponse<AuthResponseDTO>
            {
                Success = true,
                Message = ResponseMessages.TokenRefreshed,
                StatusCode = (int)HttpStatusCode.OK,
                Data = new AuthResponseDTO
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                    UserId = storedRefreshToken.UserId,
                    Expiry = DateTime.UtcNow.AddHours(1)
                }
            };
        }

        public async Task<ApiResponse<AuthResponseDTO>> GoogleLoginAsync(string idToken)
        {
            _logger.LogInformation("Google login attempt started");

            //1 Validate token with Google
            var backendClientId = _configuration["GoogleAuth:ClientId"];
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { backendClientId }
            };

            //2 Validate Google token here and handle validation errors locally
            GoogleUserInfoDto googleInfo;
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                googleInfo = new GoogleUserInfoDto
                {
                    GoogleId = payload.Subject,
                    Email = payload.Email,
                    FullName = payload.Name,
                    PictureUrl = payload.Picture
                };
            }
            catch (Exception ex)
            {
                throw new UnauthorizedException(ResponseMessages.InvalidGoogleToken, ex);
            }

            //3 Find or create local user
            var user = await _unitOfWork.Users.GetAsync(u => u.Email == googleInfo.Email);
            if (user == null)
            {
                var names = (googleInfo.FullName ?? string.Empty).Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                user = new User
                {
                    FirstName = names.Length > 0 ? names[0] : string.Empty,
                    LastName = names.Length > 1 ? names[1] : string.Empty,
                    Email = googleInfo.Email,
                    PasswordHash = null,
                    GoogleId = googleInfo.GoogleId,
                    IsGoogleUser = true
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }

            //4 Generate tokens
            var token = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var hashedRefreshToken = _jwtService.HashToken(refreshToken);

            var refreshTokenEntity = new RefreshToken
            {
                TokenHash = hashedRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = user.UserId,
                CreatedAt = DateTime.UtcNow,
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Google login successful. UserId: {UserId}", user.UserId);

            //5 Return ApiResponse<AuthResponseDTO> with Google info included
            return new ApiResponse<AuthResponseDTO>
            {
                Success = true,
                Message = ResponseMessages.LoginSuccessful,
                StatusCode = (int)HttpStatusCode.OK,
                Data = new AuthResponseDTO
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    UserId = user.UserId,
                    Expiry = DateTime.UtcNow.AddHours(1),
                    GoogleUserInfo = googleInfo
                }
            };
        }
    }
}
