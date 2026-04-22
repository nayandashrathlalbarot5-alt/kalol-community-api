using Azure;
using Azure.Communication.Email;
using KalolCommunity.Contracts.DTO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace KalolCommunity.Communication.Functions;

/// <summary>
/// Azure Function that listens to registration-email-queue
/// and sends registration confirmation email using Azure Communication Services.
/// </summary>
public class SendRegistrationEmailFunction
{
    private readonly ILogger<SendRegistrationEmailFunction> _logger;
    private readonly IConfiguration _configuration;

    public SendRegistrationEmailFunction(
        ILogger<SendRegistrationEmailFunction> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Triggered automatically when a message arrives in Service Bus queue.
    /// Queue name: registration-email-queue
    /// </summary>
    [Function("SendRegistrationEmail")]
    public async Task Run(
        [ServiceBusTrigger("registration-email-queue", Connection = "ServiceBusConnection")] string message)
    {
        try
        {
            _logger.LogInformation("SendRegistrationEmail triggered. Payload: {Payload}", message);

            var notification = JsonSerializer.Deserialize<UserNotificationEventDTO>(message);
            if (notification is null)
            {
                _logger.LogWarning("Invalid payload. Deserialization returned null. Payload: {Payload}", message);
                return;
            }

            _logger.LogInformation(
                "Preparing email. UserId: {UserId}, Email: {Email}",
                notification.UserId,
                notification.Email);

            var connectionString = _configuration["ACS:ConnectionString"]
                ?? throw new InvalidOperationException("Missing configuration: ACS:ConnectionString");

            var senderAddress = _configuration["ACS:SenderAddress"]
                ?? throw new InvalidOperationException("Missing configuration: ACS:SenderAddress");

            _logger.LogInformation("ACS configuration loaded. SenderAddress: {SenderAddress}", senderAddress);

            var templatePath = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", "RegistrationSuccess.html");
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("Email template file not found.", templatePath);
            }

            var htmlBody = await File.ReadAllTextAsync(templatePath);
            htmlBody = htmlBody.Replace("{UserName}", notification.Name);

            var emailClient = new EmailClient(connectionString);

            var emailMessage = new EmailMessage(
                senderAddress: senderAddress,
                content: new EmailContent("Registration Successful")
                {
                    PlainText = $"Dear {notification.Name},\n\nYour registration has been successfully completed.",
                    Html = htmlBody
                },
                recipients: new EmailRecipients(new[] { new EmailAddress(notification.Email, notification.Name) })
            );

            var emailResult = await emailClient.SendAsync(WaitUntil.Completed, emailMessage);

            _logger.LogInformation(
                "Email sent successfully. UserId: {UserId}, Email: {Email}, Status: {Status}",
                notification.UserId,
                notification.Email,
                emailResult.Value.Status);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(
                ex,
                "ACS RequestFailedException. HttpStatus: {Status}, ErrorCode: {ErrorCode}, Message: {Message}",
                ex.Status,
                ex.ErrorCode,
                ex.Message);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing failed. Payload: {Payload}", message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in SendRegistrationEmail. Payload: {Payload}", message);
            throw;
        }
    }
}