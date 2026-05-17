using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Body POST /operationdocuments — tworzy nowy dokument magazynowy w Sellasist.
/// Dla PZ (przyjęcie zewnętrzne) używamy Type="stock" + Subtype="admission".
///
/// MINIMALNE WYMAGANE POLA (zweryfikowane empirycznie 2026-05-17 przez test API):
/// <list type="bullet">
/// <item><c>type</c>, <c>subtype</c></item>
/// <item><c>series_id</c> — per sklep (np. 6 dla cvsklep "PZ - Przyjęcie zewnętrzne")</item>
/// <item><c>currency</c>, <c>payment_id</c>, <c>shipment_id</c></item>
/// <item><c>products[]</c> — minimum: product_id + quantity + price_gross_unit (patrz <see cref="SellasistCreateOperationDocumentProduct"/>)</item>
/// </list>
/// Adresy (buyer/receiver/supplier) są OPCJONALNE — Sellasist dopisuje receiver_address automatycznie
/// z danych sklepu (seller_data). Dla scenariuszy wymagających własnych adresów rozszerzy się DTO
/// w przyszłej iteracji.</summary>
public class SellasistCreateOperationDocumentRequest
{
    /// <summary>Typ dokumentu — "stock" dla operacji magazynowych.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "stock";

    /// <summary>Podtyp — "admission" dla PZ (przyjęcie), "release" dla WZ (wydanie).</summary>
    [JsonPropertyName("subtype")]
    public string Subtype { get; set; } = "admission";

    /// <summary>ID serii dokumentu w Sellasist (per sklep konfigurowane). Należy pobrać z pierwszego
    /// dokumentu na liście GET /operationdocuments albo skonfigurować ręcznie w aplikacji.</summary>
    [JsonPropertyName("series_id")]
    public int SeriesId { get; set; }

    /// <summary>Waluta (np. "PLN").</summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "PLN";

    /// <summary>ID metody płatności (0 dla PZ wewnętrznego — bez płatności).</summary>
    [JsonPropertyName("payment_id")]
    public int PaymentId { get; set; } = 0;

    /// <summary>ID metody dostawy (0 dla PZ wewnętrznego).</summary>
    [JsonPropertyName("shipment_id")]
    public int ShipmentId { get; set; } = 0;

    /// <summary>Data wystawienia (ISO "YYYY-MM-DD"). Opcjonalna — Sellasist używa "dzisiaj" gdy null.</summary>
    [JsonPropertyName("issue_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IssueDate { get; set; }

    /// <summary>Data sprzedaży/operacji (ISO). Opcjonalna.</summary>
    [JsonPropertyName("sale_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SaleDate { get; set; }

    // UWAGA: pole "comments" w request łamie parser Sellasist (zwraca "Missing required field:
    // series_id in document data array" mimo że series_id jest obecne) — empiryczne potwierdzenie
    // 2026-05-17. NIE dodawać Comments do tego DTO dopóki Sellasist nie naprawi parsera.

    /// <summary>Lista pozycji produktowych. Minimum 1 element wymagany.</summary>
    [JsonPropertyName("products")]
    public List<SellasistCreateOperationDocumentProduct> Products { get; set; } = new();
}
