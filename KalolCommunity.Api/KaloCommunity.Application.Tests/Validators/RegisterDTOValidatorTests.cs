using FluentValidation.TestHelper;
using KalolCommunity.Application.Common;
using KalolCommunity.Application.Validators;
using KalolCommunity.Contracts.DTO;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaloCommunity.Application.Tests.Validators
{
    public class RegisterDTOValidatorTests
    {
        private readonly RegisterDTOValidator _validator;

        public RegisterDTOValidatorTests()
        {
            _validator = new RegisterDTOValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Empty()
        {
            // Arrange
            var request = new RegisterDTO
            {
                Email = string.Empty,
                Otp = "123456",
                Flag = "L"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage(ResponseMessages.EmailMandantory);
        }

        [Theory]
        [InlineData("test.com")]
        [InlineData("gmail")]
        [InlineData("abc@")]
        [InlineData("@gmail.com")]
        public void Should_Have_Error_When_Email_Is_Invalid(string email)
        {
            var request = new RegisterDTO
            {
                Email = email,
                Otp = "123456",
                Flag = "L"
            };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage(ResponseMessages.InvalidEmail);
        }

        [Fact]
        public void Should_Have_Error_When_Otp_Is_Empty()
        {
            var request = new RegisterDTO
            {
                Email = "test@gmail.com",
                Otp = string.Empty,
                Flag = "L"
            };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Otp)
                  .WithErrorMessage(ResponseMessages.OtpMandantory);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("12345")]
        [InlineData("1234567")]
        [InlineData("ABC123")]
        public void Should_Have_Error_When_Otp_Is_Invalid(string otp)
        {
            var request = new RegisterDTO
            {
                Email = "test@gmail.com",
                Otp = otp,
                Flag = "L"
            };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Otp)
                  .WithErrorMessage(ResponseMessages.OtpMustBe6Digits);
        }

        [Fact]
        public void Should_Have_Error_When_FirstName_Is_Empty_For_Registration()
        {
            // Arrange
            var request = new RegisterDTO
            {
                Email = "test@gmail.com",
                Otp = "123456",
                Flag = "R",
                FirstName = string.Empty,
                LastName = "Barot"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.FirstName)
                  .WithErrorMessage(ResponseMessages.FirstNameMandantory);
        }

        [Fact]
        public void Should_Have_Error_When_LastName_Is_Empty_For_Registration()
        {
            // Arrange
            var request = new RegisterDTO
            {
                Email = "test@gmail.com",
                Otp = "123456",
                Flag = "R",
                FirstName = "Nayan",
                LastName = string.Empty
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.LastName)
                  .WithErrorMessage(ResponseMessages.LastNameMandantory);
        }

        [Fact]
        public void Should_Not_Have_Error_When_FirstName_Is_Empty_For_Login()
        {
            // Arrange
            var request = new RegisterDTO
            {
                Email = "test@gmail.com",
                Otp = "123456",
                Flag = "L",
                FirstName = string.Empty,
                LastName = "Barot"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        }

        [Fact]
        public void Should_Not_Have_Error_When_LastName_Is_Empty_For_Login()
        {
            // Arrange
            var request = new RegisterDTO
            {
                Email = "test@gmail.com",
                Otp = "123456",
                Flag = "L",
                FirstName = "Nayan",
                LastName = string.Empty
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.LastName);
        }
    }
}
