using KalolCommunity.Contracts.DTO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public SendRegistrationWhatsAppFunction(
        ILogger<SendRegistrationWhatsAppFunction> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
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

            var provider = (_configuration["WhatsApp:Provider"] ?? "meta").Trim().ToLowerInvariant();

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
        var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"]
            ?? throw new InvalidOperationException("Missing configuration: WhatsApp:PhoneNumberId");

        var accessToken = _configuration["WhatsApp:AccessToken"]
            ?? throw new InvalidOperationException("Missing configuration: WhatsApp:AccessToken");

        var apiUrlTemplate = _configuration["WhatsApp:MessagesApiUrlTemplate"]
            ?? throw new InvalidOperationException("Missing configuration: WhatsApp:MessagesApiUrlTemplate");

        var templateName = _configuration["WhatsApp:TemplateName"]
            ?? throw new InvalidOperationException("Missing configuration: WhatsApp:TemplateName");

        var recipient = NormalizeIndianMobile(notification.Mobile);
        var memberName = notification.Name;

        var url = apiUrlTemplate.Replace(MetaMessagesUrlPhoneNumberPlaceholder, phoneNumberId, StringComparison.OrdinalIgnoreCase);
        var payload = CreateMetaTemplateMessageRequest(recipient, templateName, memberName);

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

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
        var accountSid = _configuration["Twilio:AccountSid"]
            ?? throw new InvalidOperationException("Missing configuration: Twilio:AccountSid");

        var authToken = _configuration["Twilio:AuthToken"]
            ?? throw new InvalidOperationException("Missing configuration: Twilio:AuthToken");

        var fromWhatsApp = _configuration["Twilio:FromWhatsAppNumber"]
            ?? throw new InvalidOperationException("Missing configuration: Twilio:FromWhatsAppNumber");

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
