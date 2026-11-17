using FluentValidation;
using KalolCommunity.Application.Common;
using KalolCommunity.Contracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Validators
{
    public class RegisterDTOValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterDTOValidator()
        {
            RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(ResponseMessages.EmailMandantory)
            .EmailAddress()
                .WithMessage(ResponseMessages.InvalidEmail)
            .MaximumLength(100);

            RuleFor(x => x.Otp)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(ResponseMessages.OtpMandantory)
            .Matches(@"^\d{6}$")
                .WithMessage(ResponseMessages.OtpMustBe6Digits);

            // Registration specific validations
            When(x => x.Flag?.Trim().ToUpperInvariant() == "R", () =>
            {
                RuleFor(x => x.FirstName)
                    .NotEmpty()
                    .WithMessage(ResponseMessages.FirstNameMandantory);

                RuleFor(x => x.LastName)
                    .NotEmpty()
                    .WithMessage(ResponseMessages.LastNameMandantory);                
            });
        }
    }
}
