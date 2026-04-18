using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>DTO do aktualizacji danych adresowych dokumentu sprzedaży (bill_address) — pola PL</summary>
public class SellasistUpdateBillAddressRequest
{
    [JsonPropertyName("company_name")]
    public string? CompanyName { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("home_number")]
    public string? HomeNumber { get; set; }

    [JsonPropertyName("flat_number")]
    public string? FlatNumber { get; set; }

    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }
}
