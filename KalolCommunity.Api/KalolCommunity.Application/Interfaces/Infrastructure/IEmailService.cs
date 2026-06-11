using System.Threading.Tasks;

namespace KalolCommunity.Application.Interfaces.Infrastructure
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends OTP email using Azure Communication Services
        /// </summary>
        Task SendOtpEmailAsync(string recipientEmail, string otp);
    }
}
