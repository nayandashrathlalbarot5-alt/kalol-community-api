using System.Text.Json.Serialization;

namespace KalolCommunity.Contracts.DTO;

public class MetaTemplateMessageRequestDTO
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("template")]
    public MetaTemplateDTO Template { get; set; } = new();
}

public class MetaTemplateDTO
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public MetaLanguageDTO Language { get; set; } = new();

    [JsonPropertyName("components")]
    public List<MetaComponentDTO> Components { get; set; } = [];
}

public class MetaLanguageDTO
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}

public class MetaComponentDTO
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public List<MetaParameterDTO> Parameters { get; set; } = [];
}

public class MetaParameterDTO
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
