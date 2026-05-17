using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Pozycja produktu na dokumencie magazynowym (PZ/admission). Używana w response z GET /operationdocuments/{id}
/// (lista linii dokumentu) oraz w body POST /operationdocuments (lista do utworzenia).</summary>
public class SellasistOperationDocumentProduct
{
    /// <summary>ID produktu w Sellasist (z kartoteki).</summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    /// <summary>Ilość sztuk na pozycji.</summary>
    [JsonPropertyName("quantity")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double Quantity { get; set; }

    /// <summary>Symbol/SKU produktu.</summary>
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    /// <summary>Kod EAN produktu.</summary>
    [JsonPropertyName("ean")]
    public string? Ean { get; set; }

    /// <summary>Nazwa produktu (snapshot z chwili utworzenia dokumentu).</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Cena jednostkowa (opcjonalna).</summary>
    [JsonPropertyName("price")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Price { get; set; }
}
