using Azure.Communication.Email;
using KalolCommunity.Contracts.DTO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;

namespace KalolCommunity.Communication.Functions;

/// <summary>
/// Azure Function that listens to registration-email-queue
/// and sends registration confirmation email using Azure Communication Services.
/// </summary>
public class SendRegistrationEmailFunction
{
    private readonly ILogger<SendRegistrationEmailFunction> _logger;

    public SendRegistrationEmailFunction(ILogger<SendRegistrationEmailFunction> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Triggered automatically when a message arrives in Service Bus queue.
    /// Queue name: registration-email-queue
    /// </summary>
    [Function("SendRegistrationEmail")]
    public async Task Run([ServiceBusTrigger("registration-email-queue", Connection = "ServiceBusConnection")] string message)
    {
        // Log raw incoming queue message
        _logger.LogInformation("SendRegistrationEmail triggered. Message: {Message}", message);
        Console.WriteLine($"[SendRegistrationEmail] Triggered with message: {message}");

        // Convert JSON queue payload into DTO object
        var notification = JsonSerializer.Deserialize<UserNotificationEventDTO>(message);

        // If payload is invalid/null, skip processing safely
        if (notification is null)
        {
            _logger.LogWarning("Received null or invalid message. Skipping.");
            Console.WriteLine("[SendRegistrationEmail] Invalid/empty payload. Skipped.");
            return;
        }

        // Log important details before sending email
        _logger.LogInformation("Sending registration email to {Email} for user {UserId}", notification.Email, notification.UserId);
        Console.WriteLine($"[SendRegistrationEmail] Sending registration email to: {notification.Email} (UserId: {notification.UserId})");

        // Read Azure Communication Services connection string from environment settings
        var connectionString = Environment.GetEnvironmentVariable("AzureCommunicationServices:ConnectionString")
            ?? throw new InvalidOperationException("AzureCommunicationServices:ConnectionString is not configured.");

        // Read sender address from environment settings
        var senderAddress = Environment.GetEnvironmentVariable("AzureCommunicationServices:SenderAddress")
            ?? throw new InvalidOperationException("AzureCommunicationServices:SenderAddress is not configured.");

        // Read HTML template from project output folder and replace placeholders
        var templatePath = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", "RegistrationSuccess.html");
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Email template not found at: {templatePath}");
        }

        var htmlBody = await File.ReadAllTextAsync(templatePath);
        htmlBody = htmlBody.Replace("{UserName}", notification.Name);

        // Create ACS email client
        var emailClient = new EmailClient(connectionString);

        // Build email message content and recipient
        var emailMessage = new EmailMessage(
            senderAddress: senderAddress,
            content: new EmailContent("Registration Successful")
            {
                PlainText = $"Dear {notification.Name},\n\nYour registration has been successfully completed.",
                Html = htmlBody
            },
            recipients: new EmailRecipients(new[] { new EmailAddress(notification.Email, notification.Name) })
        );

        // Send email and wait for completion
        var emailResult = await emailClient.SendAsync(Azure.WaitUntil.Completed, emailMessage);

        // Log final status from ACS
        _logger.LogInformation("Registration email sent. Status: {Status}", emailResult.Value.Status);
        Console.WriteLine($"[SendRegistrationEmail] Email sent. Status: {emailResult.Value.Status}");
    }
}