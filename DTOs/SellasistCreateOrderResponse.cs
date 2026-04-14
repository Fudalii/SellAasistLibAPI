using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Odpowiedź API na POST /orders. Status "exist" oznacza duplikat.</summary>
public class SellasistCreateOrderResponse
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    [JsonPropertyName("order_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int OrderId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
