using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Odpowiedź POST /operationdocuments — zwraca ID i numer utworzonego dokumentu.</summary>
public class SellasistCreateOperationDocumentResponse
{
    /// <summary>ID utworzonego dokumentu w Sellasist.</summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    /// <summary>Numer dokumentu nadany przez Sellasist (np. "PZ/265/2026").</summary>
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    /// <summary>Status utworzenia (np. "ok", "created").</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
