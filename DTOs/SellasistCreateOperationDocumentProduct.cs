using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Minimalna pozycja produktu w body POST /operationdocuments. Sellasist wymaga 3 pól:
/// <c>product_id</c>, <c>quantity</c>, <c>price_gross_unit</c>. Pola EAN/Symbol/Name są opcjonalne —
/// Sellasist i tak nadpisuje je danymi z kartoteki produktu po stronie serwera.</summary>
public class SellasistCreateOperationDocumentProduct
{
    /// <summary>ID produktu w kartotece Sellasist. WYMAGANE.</summary>
    [JsonPropertyName("product_id")]
    public int ProductId { get; set; }

    /// <summary>Ilość sztuk. WYMAGANE.</summary>
    [JsonPropertyName("quantity")]
    public double Quantity { get; set; }

    /// <summary>Cena brutto jednostkowa. WYMAGANE (Sellasist throw "Missing required fields:
    /// price_gross_unit in product array" gdy null). Dla PZ wewnętrznego można dać 0.</summary>
    [JsonPropertyName("price_gross_unit")]
    public decimal PriceGrossUnit { get; set; }

    /// <summary>Stawka VAT (% jako int — np. 23). Opcjonalne; serwer dopisuje z kartoteki.</summary>
    [JsonPropertyName("vat")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Vat { get; set; }

    /// <summary>EAN — opcjonalne (informacyjne; serwer i tak czyta z kartoteki).</summary>
    [JsonPropertyName("ean")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Ean { get; set; }

    /// <summary>Symbol/SKU — opcjonalne.</summary>
    [JsonPropertyName("symbol")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Symbol { get; set; }

    /// <summary>Nazwa — opcjonalne.</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }
}
