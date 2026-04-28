using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace KalolCommunity.Communication.Functions;

/// <summary>
/// Converts English text to Gujarati script using Google Transliteration API.
/// Falls back to original text if the API call fails.
/// </summary>
public class TransliterationService
{
    private const string DefaultTransliterationApiUrlTemplate = "https://inputtools.google.com/request?text={0}&itc=gu-t-i0-und&num=1";
    private static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(5);

    private readonly HttpClient _httpClient;
    private readonly ILogger<TransliterationService> _logger;
    private readonly string _transliterationApiUrlTemplate;
    private readonly TimeSpan _requestTimeout;
    private readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

    public TransliterationService(
        IHttpClientFactory httpClientFactory,
        ILogger<TransliterationService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;

        _transliterationApiUrlTemplate = configuration["Transliteration:ApiUrlTemplate"]
            ?? DefaultTransliterationApiUrlTemplate;

        var timeoutSecondsRaw = configuration["Transliteration:TimeoutSeconds"];
        _requestTimeout = int.TryParse(timeoutSecondsRaw, out var timeoutSeconds) && timeoutSeconds > 0
            ? TimeSpan.FromSeconds(timeoutSeconds)
            : DefaultRequestTimeout;

        _httpClient.Timeout = _requestTimeout;
    }

    /// <summary>
    /// Converts an English name to Gujarati script via Google Transliteration API.
    /// Returns the original text if conversion fails.
    /// </summary>
    public async Task<string> ConvertToGujaratiAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        if (_cache.TryGetValue(text, out var cached))
        {
            _logger.LogInformation("Transliteration cache hit for '{Text}'", text);
            return cached;
        }

        try
        {
            var encodedText = Uri.EscapeDataString(text);
            var url = string.Format(_transliterationApiUrlTemplate, encodedText);

            using var cts = new CancellationTokenSource(_requestTimeout);
            var responseBody = await _httpClient.GetStringAsync(url, cts.Token);

            // Response format: ["SUCCESS", [[word, [transliteration, ...]], ...]]
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (root[0].GetString() != "SUCCESS")
            {
                _logger.LogWarning("Google Transliteration API returned non-success for '{Text}'", text);
                return text;
            }

            // Extract each word's first transliteration and join them
            var wordResults = root[1];
            var parts = new List<string>();

            foreach (var wordResult in wordResults.EnumerateArray())
            {
                var transliterations = wordResult[1];
                var first = transliterations[0].GetString();
                if (!string.IsNullOrWhiteSpace(first))
                    parts.Add(first);
            }

            var result = string.Join(" ", parts);

            if (string.IsNullOrWhiteSpace(result))
            {
                _logger.LogWarning("Transliteration result was empty for '{Text}'. Using original.", text);
                return text;
            }

            _cache[text] = result;
            _logger.LogInformation("Transliterated '{Text}' → '{Result}'", text, result);
            return result;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Google Transliteration API timed out for '{Text}'. Using original.", text);
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transliteration failed for '{Text}'. Using original.", text);
            return text;
        }
    }
}
