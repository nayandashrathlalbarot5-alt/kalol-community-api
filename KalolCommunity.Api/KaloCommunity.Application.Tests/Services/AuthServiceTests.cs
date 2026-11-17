using KalolCommunity.Application.Common;
using KalolCommunity.Application.Exceptions;
using KalolCommunity.Application.Interfaces.Infrastructure;
using KalolCommunity.Application.Interfaces.Repositories;
using KalolCommunity.Application.Interfaces.Services;
using KalolCommunity.Application.Services;
using KalolCommunity.Contracts.DTO;
using KalolCommunity.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KaloCommunity.Application.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
        private readonly Mock<ICachingService> _cachingServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _jwtTokenServiceMock = new Mock<IJwtTokenService>();
            _cachingServiceMock = new Mock<ICachingService>();
            _emailServiceMock = new Mock<IEmailService>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<AuthService>>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

            // Mock repositories returned by UnitOfWork
            _unitOfWorkMock
            .Setup(x => x.Users)
            .Returns(_userRepositoryMock.Object);


            _unitOfWorkMock
            .Setup(x => x.RefreshTokens)
            .Returns(_refreshTokenRepositoryMock.Object);

            // Create AuthService with mocked dependencies
            _authService = new AuthService(
                _unitOfWorkMock.Object,
                _passwordHasherMock.Object,
                _jwtTokenServiceMock.Object,
                _cachingServiceMock.Object,
                _emailServiceMock.Object,
                _configurationMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task Should_Send_Otp_And_Return_Success_Response()
        {
            // Arrange
            var email = "test@gmail.com";
            var flag = "R";

            // Act
            var result = await _authService.SendOtpAsync(email, flag);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(ResponseMessages.OtpSentSuccessfully, result.Message);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async Task Should_Store_Otp_In_Cache()
        {
            // Arrange
            var email = "test@gmail.com";
            var flag = "R";

            // Act
            await _authService.SendOtpAsync(email, flag);

            // Assert
            _cachingServiceMock.Verify(
                x => x.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<TimeSpan>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Send_Otp_Email()
        {
            // Arrange
            var email = "test@gmail.com";
            var flag = "R";

            // Act
            await _authService.SendOtpAsync(email, flag);

            // Assert
            _emailServiceMock.Verify(
                x => x.SendOtpEmailAsync(
                    email,
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Throw_Unauthorized_When_User_Not_Found()
        {
            // Arrange
            _userRepositoryMock
                .Setup(x => x.GetAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((User?)null);

            var request = new LoginDTO
            {
                Email = "test@gmail.com",
                Password = "Password123"
            };

            // Act + Assert
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _authService.LoginAsync(request));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("999999")]
        public async Task Should_Return_BadRequest_When_Otp_Is_Invalid(string otp)
        {
            // Arrange
            _cachingServiceMock
                .Setup(x => x.GetAsync<string>(It.IsAny<string>()))
                .ReturnsAsync("123456");

            var request = new RegisterDTO
            {
                Email = "test@gmail.com",
                Otp = otp, // Use the parameterized OTP
                Flag = "L"
            };

            // Act
            var result = await _authService.VerifyOtpAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ResponseMessages.InvalidOrExpiredOtp, result.Message);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Should_Return_Conflict_When_Email_Already_Registered()
        {
            // Arrange
            _cachingServiceMock
                .Setup(x => x.GetAsync<string>(It.IsAny<string>()))
                .ReturnsAsync("123456");

            _userRepositoryMock
                .Setup(x => x.AnyAsync(
                    It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(true);

            var request = new RegisterDTO
            {
                FirstName = "Nayan",
                LastName = "Barot",
                Email = "test@gmail.com",
                Password = "Password123",
                Otp = "123456",
                Flag = "R"
            };

            // Act
            var result = await _authService.VerifyOtpAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ResponseMessages.EmailAlreadyRegistered, result.Message);
            Assert.Equal((int)HttpStatusCode.Conflict, result.StatusCode);
        }

        [Fact]
        public async Task Should_Register_User_When_Otp_Is_Valid()
        {
            // Arrange
            _cachingServiceMock
                .Setup(x => x.GetAsync<string>(It.IsAny<string>()))
                .ReturnsAsync("123456");

            _userRepositoryMock
                .Setup(x => x.AnyAsync(
                    It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(false);

            var request = new RegisterDTO
            {
                FirstName = "Nayan",
                LastName = "Barot",
                Email = "test@gmail.com",
                Otp = "123456",
                Flag = "R"
            };

            // Act
            var result = await _authService.VerifyOtpAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(ResponseMessages.RegistrationSuccessful, result.Message);
            Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);

            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _cachingServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Should_Return_NotFound_When_Login_User_Does_Not_Exist()
        {
            // Arrange
            _cachingServiceMock
                .Setup(x => x.GetAsync<string>(It.IsAny<string>()))
                .ReturnsAsync("123456");

            _userRepositoryMock
                .Setup(x => x.GetAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((User?)null);

            var request = new RegisterDTO
            {
                Email = "test@gmail.com",
                Otp = "123456",
                Flag = "L"
            };

            // Act
            var result = await _authService.VerifyOtpAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ResponseMessages.UserNotFound, result.Message);
            Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task Should_Login_User_When_Otp_Is_Valid()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                UserId = userId,
                Email = "test@gmail.com",
                FirstName = "Nayan",
                LastName = "Barot"
            };

            _cachingServiceMock
                .Setup(x => x.GetAsync<string>(It.IsAny<string>()))
                .ReturnsAsync("123456");

            _userRepositoryMock
                .Setup(x => x.GetAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(user);

            _jwtTokenServiceMock
                .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
                .Returns("jwt-token");

            _jwtTokenServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            _jwtTokenServiceMock
                .Setup(x => x.HashToken(It.IsAny<string>()))
                .Returns("hashed-refresh-token");

            var request = new RegisterDTO
            {
                Email = "test@gmail.com",
                Otp = "123456",
                Flag = "L"
            };

            // Act
            var result = await _authService.VerifyOtpAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(ResponseMessages.LoginSuccessful, result.Message);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal("jwt-token", result.Data.Token);
            Assert.Equal("refresh-token", result.Data.RefreshToken);
            Assert.Equal(userId, result.Data.UserId);

            // Verify Refresh Token saved
            _refreshTokenRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<RefreshToken>()),
                Times.Once);

            // Verify DB commit
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);

            // Verify OTP removed from cache
            _cachingServiceMock.Verify(
                x => x.RemoveAsync(It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Throw_BadRequest_When_UserId_Is_Invalid()
        {
            // Arrange
            var request = new RefreshTokenRequestDTO
            {
                UserId = "Invalid-Guid",
                RefreshToken = "refresh-token"
            };

            // Act
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                            () => _authService.RefreshTokenAsync(request));

            // Assert
            Assert.Equal(ResponseMessages.InvalidUserId, exception.Message);
        }

        [Fact]
        public async Task Should_Throw_NotFound_When_User_Not_Found()
        {
            // Arrange
            var refreshTokenEntity = new RefreshToken
            {
                UserId = Guid.NewGuid(),
                TokenHash = "hashed-token",
                IsRevoked = false,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _jwtTokenServiceMock
                .Setup(x => x.HashToken(It.IsAny<string>()))
                .Returns("hashed-token");

            _refreshTokenRepositoryMock
                .Setup(x => x.GetAsync(
                    It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(refreshTokenEntity);

            _userRepositoryMock
                .Setup(x => x.GetAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((User?)null);

            var request = new RefreshTokenRequestDTO
            {
                UserId = refreshTokenEntity.UserId.ToString(),
                RefreshToken = "refresh-token"
            };

            // Act
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _authService.RefreshTokenAsync(request));

            // Assert
            Assert.Equal(ResponseMessages.UserNotFound, exception.Message);
        }

        [Fact]
        public async Task Should_Throw_Unauthorized_When_RefreshToken_Is_Revoked()
        {
            // Arrange
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = "test@gmail.com"
            };

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = "hashed-token",
                IsRevoked = true,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _jwtTokenServiceMock
                .Setup(x => x.HashToken(It.IsAny<string>()))
                .Returns("hashed-token");

            _refreshTokenRepositoryMock
                .Setup(x => x.GetAsync(
                    It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(refreshTokenEntity);

            _userRepositoryMock
                .Setup(x => x.GetAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(user);

            var request = new RefreshTokenRequestDTO
            {
                UserId = user.UserId.ToString(),
                RefreshToken = "refresh-token"
            };

            // Act
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(
                () => _authService.RefreshTokenAsync(request));

            // Assert
            Assert.Equal(ResponseMessages.RefreshTokenExpiredOrRevoked, exception.Message);
        }

        [Fact]
        public async Task Should_Throw_Unauthorized_When_RefreshToken_Is_Expired()
        {
            // Arrange
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = "test@gmail.com"
            };

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = "hashed-token",
                IsRevoked = false,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
            };

            _jwtTokenServiceMock
                .Setup(x => x.HashToken(It.IsAny<string>()))
                .Returns("hashed-token");

            _refreshTokenRepositoryMock
                .Setup(x => x.GetAsync(
                    It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(refreshTokenEntity);

            _userRepositoryMock
                .Setup(x => x.GetAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(user);

            var request = new RefreshTokenRequestDTO
            {
                UserId = user.UserId.ToString(),
                RefreshToken = "refresh-token"
            };

            // Act
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(
                () => _authService.RefreshTokenAsync(request));

            // Assert
            Assert.Equal(
                ResponseMessages.RefreshTokenExpiredOrRevoked,
                exception.Message);
        }

        [Fact]
        public async Task Should_Refresh_Token_Successfully()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                UserId = userId,
                Email = "test@gmail.com"
            };

            var storedRefreshToken = new RefreshToken
            {
                UserId = userId,
                TokenHash = "hashed-token",
                IsRevoked = false,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _jwtTokenServiceMock
                .Setup(x => x.HashToken(It.IsAny<string>()))
                .Returns("hashed-token");

            _refreshTokenRepositoryMock
                .Setup(x => x.GetAsync(
                    It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(storedRefreshToken);

            _userRepositoryMock
                .Setup(x => x.GetAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(user);

            _jwtTokenServiceMock
                .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
                .Returns("new-access-token");

            _jwtTokenServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");

            _jwtTokenServiceMock
                .Setup(x => x.HashToken("new-refresh-token"))
                .Returns("new-hashed-refresh-token");

            var request = new RefreshTokenRequestDTO
            {
                UserId = userId.ToString(),
                RefreshToken = "old-refresh-token"
            };

            // Act
            var result = await _authService.RefreshTokenAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(ResponseMessages.TokenRefreshed,result.Message);
            Assert.Equal((int)HttpStatusCode.OK,result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal("new-access-token",result.Data.Token);
            Assert.Equal("new-refresh-token",result.Data.RefreshToken);
            Assert.Equal(userId,result.Data.UserId);

            // Verify old token revoked
            _refreshTokenRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>()),Times.Once);

            // Verify new token created
            _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()),Times.Once);

            // Verify database commit
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(),Times.Once);
        }
    }
}
