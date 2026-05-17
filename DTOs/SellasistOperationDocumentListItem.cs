using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Element listy zwracanej z GET /operationdocuments?type=stock&amp;subtype=admission.
/// Zawiera podstawowe pola dokumentu — pełne szczegóły (linie produktów, adresy) pobiera się
/// osobno przez GET /operationdocuments/{id}.</summary>
public class SellasistOperationDocumentListItem
{
    /// <summary>ID dokumentu magazynowego.</summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    /// <summary>Numer dokumentu nadany przez Sellasist (np. "PZ/1/05/2026").</summary>
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    /// <summary>Niestandardowy numer dokumentu (zwykle taki sam jak <see cref="Number"/>, ale może
    /// być nadpisany przy tworzeniu). Wyświetlamy w UI gdy istnieje.</summary>
    [JsonPropertyName("custom_number")]
    public string? CustomNumber { get; set; }

    /// <summary>Numer oryginalny (np. PZ dostawcy gdy importujemy z zewnętrznego dokumentu).</summary>
    [JsonPropertyName("original_number")]
    public string? OriginalNumber { get; set; }

    /// <summary>Data wystawienia dokumentu (format ISO "YYYY-MM-DD").</summary>
    [JsonPropertyName("issue_date")]
    public string? IssueDate { get; set; }

    /// <summary>Data sprzedaży/operacji (zwykle taka sama jak <see cref="IssueDate"/>).</summary>
    [JsonPropertyName("sale_date")]
    public string? SaleDate { get; set; }

    /// <summary>Typ dokumentu — dla magazynowych "stock".</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Podtyp — "admission" dla PZ (przyjęcie), "release" dla WZ (wydanie).</summary>
    [JsonPropertyName("subtype")]
    public string? Subtype { get; set; }

    /// <summary>ID serii dokumentu (per sklep konfigurowane — np. 6 dla "PZ - Przyjęcie zewnętrzne").</summary>
    [JsonPropertyName("series_id")]
    public int? SeriesId { get; set; }

    /// <summary>Czy dokument został wydrukowany (0/1).</summary>
    [JsonPropertyName("is_printed")]
    public int IsPrinted { get; set; }

    /// <summary>Czy druk zawieszony (0/1).</summary>
    [JsonPropertyName("print_suspended")]
    public int PrintSuspended { get; set; }

    /// <summary>Łączna wartość dokumentu (może być null gdy 0).</summary>
    [JsonPropertyName("total")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? Total { get; set; }

    /// <summary>Waluta dokumentu (np. "PLN").</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}
