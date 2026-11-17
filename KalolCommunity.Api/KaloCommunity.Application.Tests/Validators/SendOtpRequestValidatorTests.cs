using FluentValidation.TestHelper;
using KalolCommunity.Application.Common;
using KalolCommunity.Application.Interfaces.Repositories;
using KalolCommunity.Application.Validators;
using KalolCommunity.Contracts.DTO;
using KalolCommunity.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace KaloCommunity.Application.Tests.Validators
{
    public class SendOtpRequestValidatorTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly SendOtpRequestValidator _validator;
        
        public SendOtpRequestValidatorTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _unitOfWorkMock
                .Setup(x => x.Users)
                .Returns(_userRepositoryMock.Object);

            _validator = new SendOtpRequestValidator(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Should_Have_Error_When_Email_Is_Empty()
        {
            // Arrange
            var request = new SendOtpRequestDTO
            {
                Email = string.Empty,
                Flag = "L"
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage(ResponseMessages.EmailMandantory);
        }

        [Theory]
        [InlineData("test.com")]
        [InlineData("gmail")]
        [InlineData("abc@")]
        [InlineData("@gmail.com")]
        public async Task Should_Have_Error_When_Email_Is_Invalid(string email)
        {
            // Arrange
            var request = new SendOtpRequestDTO
            {
                Email = email,
                Flag = "L"
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage(ResponseMessages.InvalidEmail);
        }

        [Fact]
        public async Task Should_Not_Have_Error_When_Email_Is_Valid()
        {
            // Arrange
            var request = new SendOtpRequestDTO
            {
                Email = "nayan.barot@gmail.com",
                Flag = "L"
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public async Task Should_Have_Error_When_Email_Already_Registered()
        {
            // Arrange
            _userRepositoryMock
                .Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(true);

            var request = new SendOtpRequestDTO
            {
                Email = "nayan@gmail.com",
                Flag = "R"
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage(ResponseMessages.EmailAlreadyRegistered);
        }

        [Fact]
        public async Task Should_Not_Have_Error_When_Email_Is_Not_Registered()
        {
            // Arrange
            _userRepositoryMock
                .Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(false);

            var request = new SendOtpRequestDTO
            {
                Email = "nayan.barot@gmail.com",
                Flag = "R"
            };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Email);                  

        }
    }
}
