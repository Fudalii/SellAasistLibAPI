using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Element listy z GET /operationdocuments_series — seria dokumentów operacyjnych
/// (magazynowych) zdefiniowana w sklepie Sellasist. UWAGA: to INNY endpoint niż
/// /documents_series (tamten zwraca serie dokumentów sprzedażowych — faktury/rachunki).
/// Serie PZ/WZ używane przez /operationdocuments (pole series_id) pochodzą stąd.
/// Dla PZ filtruj po <see cref="Type"/>="stock" + <see cref="Subtype"/>="admission".</summary>
public class SellasistOperationDocumentSeriesListItem
{
    /// <summary>ID serii — wartość do użycia jako <c>series_id</c> w /operationdocuments
    /// (filtr listy oraz tworzenie dokumentu).</summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    /// <summary>Nazwa serii (np. "PZ - Przyjęcie zewnętrzne").</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Typ dokumentu operacyjnego (np. "stock", "invoice", "order", "receipt").</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Podtyp dokumentu (np. "admission" dla PZ, "release" dla WZ, "sale").</summary>
    [JsonPropertyName("subtype")]
    public string? Subtype { get; set; }

    /// <summary>Prefiks numeracji dokumentów w serii (np. "PZ").</summary>
    [JsonPropertyName("prefix")]
    public string? Prefix { get; set; }

    /// <summary>Sufiks numeracji dokumentów w serii (np. "/SU").</summary>
    [JsonPropertyName("suffix")]
    public string? Suffix { get; set; }
}
