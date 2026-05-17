using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Element listy zwracanej z GET /operationdocuments?type=stock&amp;subtype=admission.
/// Zawiera podstawowe pola dokumentu (id, numer, daty) bez listy produktów — pełne szczegóły
/// pobiera się osobno przez GET /operationdocuments/{id}.</summary>
public class SellasistOperationDocumentListItem
{
    /// <summary>ID dokumentu magazynowego.</summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    /// <summary>Numer dokumentu w Sellasist (np. "PZ/260/2026").</summary>
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    /// <summary>Typ dokumentu (zwykle "stock").</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Podtyp dokumentu (zwykle "admission" dla PZ).</summary>
    [JsonPropertyName("subtype")]
    public string? Subtype { get; set; }

    /// <summary>Status dokumentu (np. "open", "closed").</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>Data wystawienia dokumentu (string ISO).</summary>
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    /// <summary>Data utworzenia w systemie (string ISO).</summary>
    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    /// <summary>Wartość dokumentu (string, np. "1234.50").</summary>
    [JsonPropertyName("total")]
    public string? Total { get; set; }

    /// <summary>Nazwa kontrahenta (kupującego) — może być w polu buyer_address.company_name lub buyer_name.</summary>
    [JsonPropertyName("buyer_name")]
    public string? BuyerName { get; set; }

    /// <summary>Adres kupującego (gdy zawarty w odpowiedzi).</summary>
    [JsonPropertyName("buyer_address")]
    public SellasistOperationDocumentAddress? BuyerAddress { get; set; }
}
