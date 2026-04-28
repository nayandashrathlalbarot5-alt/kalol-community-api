using KalolCommunity.Contracts.DTO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace KalolCommunity.Communication.Functions;

public class SendRegistrationWhatsAppFunction
{
    private const string MetaMessagesUrlPhoneNumberPlaceholder = "{phoneNumberId}";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<SendRegistrationWhatsAppFunction> _logger;
    private readonly HttpClient _httpClient;

    public SendRegistrationWhatsAppFunction(
        ILogger<SendRegistrationWhatsAppFunction> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    [Function("SendRegistrationWhatsApp")]
    public async Task Run(
        [ServiceBusTrigger("registration-whatsapp-queue", Connection = "ServiceBusConnection")] string message,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("SendRegistrationWhatsApp triggered. Payload: {Payload}", message);

            var notification = JsonSerializer.Deserialize<UserNotificationEventDTO>(message, SerializerOptions);
            if (notification is null)
            {
                _logger.LogWarning("Invalid WhatsApp payload. Deserialization returned null. Payload: {Payload}", message);
                return;
            }

            if (string.IsNullOrWhiteSpace(notification.Name) || string.IsNullOrWhiteSpace(notification.Mobile))
            {
                _logger.LogWarning("Invalid notification data for user {UserId}. Name or Mobile is missing.", notification.UserId);
                return;
            }

            var provider = Environment.GetEnvironmentVariable("WhatsApp:Provider")?.Trim().ToLowerInvariant() ?? "meta";

            switch (provider)
            {
                case "meta":
                    await SendViaMetaApiAsync(notification, cancellationToken);
                    break;
                case "twilio":
                    await SendViaTwilioAsync(notification);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported WhatsApp provider '{provider}'. Use 'meta' or 'twilio'.");
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "JSON parsing failed in SendRegistrationWhatsApp. Payload: {Payload}, Message: {ExceptionMessage}, InnerException: {InnerException}, StackTrace: {StackTrace}",
                message,
                ex.Message,
                ex.InnerException?.Message,
                ex.StackTrace);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled error in SendRegistrationWhatsApp. Payload: {Payload}, Message: {ExceptionMessage}, InnerException: {InnerException}, StackTrace: {StackTrace}",
                message,
                ex.Message,
                ex.InnerException?.Message,
                ex.StackTrace);
            throw;
        }
    }

    private async Task SendViaMetaApiAsync(UserNotificationEventDTO notification, CancellationToken cancellationToken)
    {
        // Read required Meta WhatsApp configuration values.
        var phoneNumberId = GetRequiredEnvironmentVariable("WhatsApp:PhoneNumberId");
        var accessToken = GetRequiredEnvironmentVariable("WhatsApp:AccessToken");
        var apiUrlTemplate = GetRequiredEnvironmentVariable("WhatsApp:MessagesApiUrlTemplate");
        var templateName = GetRequiredEnvironmentVariable("WhatsApp:TemplateName");

        // Prepare message data from incoming notification.
        var recipient = NormalizeIndianMobile(notification.Mobile);
        var memberName = notification.Name;

        // Build final Meta API endpoint and request payload.
        var url = apiUrlTemplate.Replace(MetaMessagesUrlPhoneNumberPlaceholder, phoneNumberId, StringComparison.OrdinalIgnoreCase);
        var payload = CreateMetaTemplateMessageRequest(recipient, templateName, memberName);

        // Create HTTP POST request with JSON body.
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        // Add bearer token for Meta API authentication.
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Send request and read response for logging/debugging.
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        // Log and fail fast if Meta API returns non-success status.
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Meta WhatsApp send failed for {Mobile}. StatusCode: {StatusCode}, Response: {Response}",
                recipient,
                response.StatusCode,
                responseBody);
            throw new InvalidOperationException($"Meta WhatsApp API call failed. StatusCode: {response.StatusCode}, Response: {responseBody}");
        }

        _logger.LogInformation("Meta WhatsApp sent successfully to {Mobile}. Response: {Response}", recipient, responseBody);
    }

    private async Task SendViaTwilioAsync(UserNotificationEventDTO notification)
    {
        var accountSid = GetRequiredEnvironmentVariable("Twilio:AccountSid");
        var authToken = GetRequiredEnvironmentVariable("Twilio:AuthToken");
        var fromWhatsApp = GetRequiredEnvironmentVariable("Twilio:FromWhatsAppNumber");
        var recipient = NormalizeIndianMobile(notification.Mobile);

        TwilioClient.Init(accountSid, authToken);

        var whatsappBody =
            $"Hello {notification.Name},\n\n" +
            "Thank you for registering with *Kalol Brahmbhatt Community*.\n\n" +
            "Your registration has been successfully submitted and is currently under review by our administrator. You will receive a confirmation message once your registration is approved.\n\n" +
            "Regards,\n" +
            "Kalol Brahmbhatt Community";

        await MessageResource.CreateAsync(
            from: new PhoneNumber(fromWhatsApp),
            to: new PhoneNumber($"whatsapp:+{recipient}"),
            body: whatsappBody);

        _logger.LogInformation("Twilio WhatsApp sent successfully to {Mobile}", recipient);
    }

    private static string GetRequiredEnvironmentVariable(string key)
        => Environment.GetEnvironmentVariable(key)
           ?? throw new InvalidOperationException($"{key} is not configured.");

    private static string NormalizeIndianMobile(string mobile)
    {
        var digits = new string(mobile.Where(char.IsDigit).ToArray());
        if (digits.Length == 10)
        {
            return $"91{digits}";
        }

        if (digits.Length == 12 && digits.StartsWith("91", StringComparison.Ordinal))
        {
            return digits;
        }

        throw new InvalidOperationException($"Invalid mobile number format: {mobile}");
    }

    private static MetaTemplateMessageRequestDTO CreateMetaTemplateMessageRequest(string recipient, string templateName, string memberName)
        => new()
        {
            MessagingProduct = "whatsapp",
            To = recipient,
            Type = "template",
            Template = new MetaTemplateDTO
            {
                Name = templateName,
                Language = new MetaLanguageDTO
                {
                    Code = "gu"
                },
                Components =
                [
                    new MetaComponentDTO
                    {
                        Type = "body",
                        Parameters =
                        [
                            new MetaParameterDTO
                            {
                                Type = "text",
                                Text = memberName
                            }
                        ]
                    }
                ]
            }
        };
}
