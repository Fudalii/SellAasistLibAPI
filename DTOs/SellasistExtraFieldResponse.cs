using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Dodatkowe pole zam��wienia z /extra-fields.</summary>
public class SellasistExtraFieldResponse
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}
