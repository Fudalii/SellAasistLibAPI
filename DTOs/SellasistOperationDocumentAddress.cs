using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Adres na dokumencie magazynowym — buyer_address / receiver_address / supplier_address.
/// READ-only DTO (z GET /operationdocuments/{id}). W POST /operationdocuments adresy są opcjonalne
/// i Sellasist sam dopisuje receiver_address z danych sklepu (seller_data) — nie ma potrzeby
/// wysyłać tego DTO przy tworzeniu PZ.</summary>
public class SellasistOperationDocumentAddress
{
    /// <summary>Imię osoby kontaktowej (gdy adres prywatny).</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Nazwisko osoby kontaktowej.</summary>
    [JsonPropertyName("surname")]
    public string? Surname { get; set; }

    /// <summary>Ulica.</summary>
    [JsonPropertyName("street")]
    public string? Street { get; set; }

    /// <summary>Numer domu.</summary>
    [JsonPropertyName("home_number")]
    public string? HomeNumber { get; set; }

    /// <summary>Numer mieszkania.</summary>
    [JsonPropertyName("flat_number")]
    public string? FlatNumber { get; set; }

    /// <summary>Dodatkowy opis adresu (rzadko używany).</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Kod pocztowy (np. "36-030").</summary>
    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    /// <summary>Miasto.</summary>
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>Województwo / stan.</summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>Telefon.</summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>Nazwa firmy.</summary>
    [JsonPropertyName("company_name")]
    public string? CompanyName { get; set; }

    /// <summary>NIP firmy.</summary>
    [JsonPropertyName("company_nip")]
    public string? CompanyNip { get; set; }

    /// <summary>Kraj — Sellasist zwraca jako obiekt { id, name, code } w shape detail.</summary>
    [JsonPropertyName("country")]
    public SellasistOperationDocumentCountry? Country { get; set; }
}

/// <summary>Kraj na dokumencie magazynowym — zagnieżdżony obiekt w adresach.</summary>
public class SellasistOperationDocumentCountry
{
    /// <summary>ID kraju ISO numeric (np. 170 = Polska).</summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    /// <summary>Nazwa kraju (np. "Poland").</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Kod ISO 2-literowy (np. "PL").</summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }
}
