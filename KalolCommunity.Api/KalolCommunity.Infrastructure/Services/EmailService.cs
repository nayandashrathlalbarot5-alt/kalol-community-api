using System;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using KalolCommunity.Application.Exceptions;
using KalolCommunity.Application.Interfaces.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KalolCommunity.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _acsConnectionString;
        private readonly string _senderAddress;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _logger = logger;
            _acsConnectionString = configuration["ACS:ConnectionString"]
                ?? throw new BadRequestException("ACS:ConnectionString not configured", "ACS_CONFIG_MISSING");
            _senderAddress = configuration["ACS:SenderAddress"]
                ?? throw new BadRequestException("ACS:SenderAddress not configured", "ACS_CONFIG_MISSING");
        }

        public async Task SendOtpEmailAsync(string recipientEmail, string otp)
        {
            try
            {
                var emailClient = new EmailClient(_acsConnectionString);

                var emailContent = new EmailContent("Your OTP for Kalol Brahmbhatt Community Registration")
                {
                    PlainText = $"Your One-Time Password (OTP) is: {otp}\n\nThis OTP will expire in 5 minutes.\n\nDo not share this OTP with anyone.",
                    Html = $@"
                        <html>
                            <body style='font-family: Arial, sans-serif;'>
                                <h2>Your One-Time Password (OTP)</h2>
                                <p>Your OTP for Kalol Brahmbhatt Community Registration is:</p>
                                <h1 style='color: #007bff; letter-spacing: 2px;'>{otp}</h1>
                                <p style='color: #666; font-size: 14px;'>
                                    This OTP will expire in <strong>5 minutes</strong>.
                                </p>
                                <p style='color: #d9534f; font-weight: bold;'>
                                    Do not share this OTP with anyone.
                                </p>
                                <hr style='margin-top: 30px;' />
                                <p style='color: #999; font-size: 12px;'>
                                    If you didn't request this OTP, please ignore this email.
                                </p>
                            </body>
                        </html>
                    "
                };

                var emailMessage = new EmailMessage(
                    senderAddress: _senderAddress,
                    content: emailContent,
                    recipients: new EmailRecipients(new[] { new EmailAddress(recipientEmail) })
                );

                var emailResult = await emailClient.SendAsync(WaitUntil.Completed, emailMessage);

                _logger.LogInformation(
                    "OTP email sent successfully. Status: {Status}, Recipient: {Recipient}",
                    emailResult.Value.Status,
                    recipientEmail);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "ACS email service failed for {Email}. Status: {Status}", recipientEmail, ex.Status);
                throw new InvalidOperationException(
                    $"Failed to send OTP email. ACS service error: {ex.Message}",
                    ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending OTP email to {Email}", recipientEmail);
                throw;
            }
        }
    }
}
