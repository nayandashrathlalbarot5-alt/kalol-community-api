using FluentValidation;
using KalolCommunity.Application.Common;
using KalolCommunity.Application.Interfaces;
using KalolCommunity.Application.Interfaces.Repositories;
using KalolCommunity.Contracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Validators
{
    public class SendOtpRequestValidator : AbstractValidator<SendOtpRequestDTO>
    {
        public SendOtpRequestValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(ResponseMessages.EmailMandantory)
                .EmailAddress().WithMessage(ResponseMessages.InvalidEmail);

            // Only when registering, ensure email is NOT already registered (async DB check)
            When(x => x.Flag?.Trim().ToUpperInvariant() == "R", () =>
            {
                RuleFor(x => x.Email)
                    .MustAsync(async (email, ct) =>
                        !await unitOfWork.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
                    .WithMessage(ResponseMessages.EmailAlreadyRegistered);
            });
        }
    }
}