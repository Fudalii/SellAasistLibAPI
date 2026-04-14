using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Status zamowienia z endpointu /statuses.</summary>
public class SellasistStatusResponse
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
