using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Szczegóły dokumentu magazynowego z GET /operationdocuments/{id} — zawiera listę linii produktów.</summary>
public class SellasistOperationDocumentDetail
{
    /// <summary>ID dokumentu.</summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    /// <summary>Numer dokumentu (np. "PZ/260/2026").</summary>
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    /// <summary>Typ ("stock").</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Podtyp ("admission" dla PZ).</summary>
    [JsonPropertyName("subtype")]
    public string? Subtype { get; set; }

    /// <summary>Status dokumentu.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>Data wystawienia.</summary>
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    /// <summary>Data utworzenia.</summary>
    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    /// <summary>Kod kraju ISO numeric (np. 170 = PL).</summary>
    [JsonPropertyName("country")]
    public int? Country { get; set; }

    /// <summary>NIP kontrahenta.</summary>
    [JsonPropertyName("company_nip")]
    public string? CompanyNip { get; set; }

    /// <summary>Telefon kontrahenta.</summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>Adres kupującego.</summary>
    [JsonPropertyName("buyer_address")]
    public SellasistOperationDocumentAddress? BuyerAddress { get; set; }

    /// <summary>Adres odbiorcy (magazyn docelowy).</summary>
    [JsonPropertyName("receiver_address")]
    public SellasistOperationDocumentAddress? ReceiverAddress { get; set; }

    /// <summary>Pozycje produktowe dokumentu.</summary>
    [JsonPropertyName("products")]
    public List<SellasistOperationDocumentProduct> Products { get; set; } = new();
}

/// <summary>Adres na dokumencie magazynowym (buyer_address lub receiver_address). Współdzielony przez
/// SellasistOperationDocumentDetail (GET) oraz SellasistCreateOperationDocumentRequest (POST).</summary>
public class SellasistOperationDocumentAddress
{
    /// <summary>Nazwa firmy.</summary>
    [JsonPropertyName("company_name")]
    public string? CompanyName { get; set; }

    /// <summary>Imię osoby kontaktowej.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Nazwisko osoby kontaktowej.</summary>
    [JsonPropertyName("surname")]
    public string? Surname { get; set; }

    /// <summary>Ulica i numer domu (jeden string — Sellasist nie rozdziela dla operationdocuments).</summary>
    [JsonPropertyName("street")]
    public string? Street { get; set; }

    /// <summary>Numer domu (opcjonalne — gdy Sellasist rozdziela).</summary>
    [JsonPropertyName("home_number")]
    public string? HomeNumber { get; set; }

    /// <summary>Numer mieszkania (opcjonalne).</summary>
    [JsonPropertyName("flat_number")]
    public string? FlatNumber { get; set; }

    /// <summary>Kod pocztowy.</summary>
    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    /// <summary>Miasto.</summary>
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>Telefon.</summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>NIP firmy.</summary>
    [JsonPropertyName("company_nip")]
    public string? CompanyNip { get; set; }

    /// <summary>Email.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }
}
