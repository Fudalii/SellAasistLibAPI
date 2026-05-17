using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Body żądania POST /operationdocuments — tworzy nowy dokument magazynowy w Sellasist.
/// Dla PZ (przyjęcia) używamy Type="stock" + Subtype="admission". Adresy oraz dane kontrahenta
/// są opcjonalne w API, ale Sellasist może wymagać kraju (170 = PL).</summary>
public class SellasistCreateOperationDocumentRequest
{
    /// <summary>Typ dokumentu — "stock" dla operacji magazynowych.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "stock";

    /// <summary>Podtyp dokumentu — "admission" dla PZ (przyjęcie), "release" dla WZ (wydanie).</summary>
    [JsonPropertyName("subtype")]
    public string Subtype { get; set; } = "admission";

    /// <summary>Kod kraju ISO numeric (170 = Polska).</summary>
    [JsonPropertyName("country")]
    public int Country { get; set; } = 170;

    /// <summary>NIP firmy ("0" gdy brak).</summary>
    [JsonPropertyName("company_nip")]
    public string CompanyNip { get; set; } = "0";

    /// <summary>Telefon kontrahenta (pusty string gdy brak).</summary>
    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>Adres kupującego (firma od której kupujemy).</summary>
    [JsonPropertyName("buyer_address")]
    public SellasistOperationDocumentAddress BuyerAddress { get; set; } = new();

    /// <summary>Adres odbiorcy (nasz magazyn).</summary>
    [JsonPropertyName("receiver_address")]
    public SellasistOperationDocumentAddress ReceiverAddress { get; set; } = new();

    /// <summary>Lista pozycji produktowych. Każda zawiera Id, Quantity oraz pola informacyjne (Symbol, Ean, Name).</summary>
    [JsonPropertyName("products")]
    public List<SellasistOperationDocumentProduct> Products { get; set; } = new();
}
