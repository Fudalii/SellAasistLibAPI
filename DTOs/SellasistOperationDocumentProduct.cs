using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Pozycja produktu w response z GET /operationdocuments/{id}. UWAGA na rozróżnienie:
/// <list type="bullet">
/// <item><c>Id</c> — ID linii dokumentu (unikalne per dokument, np. 515 dla pierwszej linii PZ 261)</item>
/// <item><c>ProductId</c> — ID produktu z kartoteki Sellasist (np. 101 — używamy do GetProductAsync i scanowania)</item>
/// </list>
/// Do POST /operationdocuments używa się osobnego DTO <see cref="SellasistCreateOperationDocumentProduct"/>
/// z minimalnym setem pól (product_id, quantity, price_gross_unit).</summary>
public class SellasistOperationDocumentProduct
{
    /// <summary>ID linii dokumentu (NIE ID produktu — tym jest <see cref="ProductId"/>).</summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    /// <summary>ID produktu w kartotece Sellasist — używamy do GetProductAsync (thumbnail) i matchingu EAN.</summary>
    [JsonPropertyName("product_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int ProductId { get; set; }

    /// <summary>Ilość na pozycji (Sellasist zwraca jako string "4.000" — parsujemy do double).</summary>
    [JsonPropertyName("quantity")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double Quantity { get; set; }

    /// <summary>Symbol/SKU produktu.</summary>
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    /// <summary>EAN produktu (może być pusty string gdy produkt nie ma EAN w kartotece).</summary>
    [JsonPropertyName("ean")]
    public string? Ean { get; set; }

    /// <summary>Nazwa produktu (snapshot z chwili utworzenia dokumentu).</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Stawka VAT w procentach (np. 8 dla suplementów).</summary>
    [JsonPropertyName("vat")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Vat { get; set; }

    /// <summary>Cena netto pozycji (price × quantity).</summary>
    [JsonPropertyName("price_net")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal PriceNet { get; set; }

    /// <summary>Cena brutto pozycji.</summary>
    [JsonPropertyName("price_gross")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal PriceGross { get; set; }

    /// <summary>Cena netto jednostkowa.</summary>
    [JsonPropertyName("price_net_unit")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal PriceNetUnit { get; set; }

    /// <summary>Cena brutto jednostkowa.</summary>
    [JsonPropertyName("price_gross_unit")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal PriceGrossUnit { get; set; }

    /// <summary>Numer katalogowy (zwykle === Symbol).</summary>
    [JsonPropertyName("catalog_number")]
    public string? CatalogNumber { get; set; }

    /// <summary>Waga pozycji (string "0.000" → double).</summary>
    [JsonPropertyName("weight")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double Weight { get; set; }

    /// <summary>Lokalizacja magazynowa (basic_location) z kartoteki produktu — może być null.</summary>
    [JsonPropertyName("basic_location")]
    public string? BasicLocation { get; set; }

    /// <summary>Ilość zrealizowana (przyjęta/wydana) — relevant dla częściowych realizacji.</summary>
    [JsonPropertyName("completed_quantity")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double CompletedQuantity { get; set; }

    /// <summary>Ilość uszkodzona/defekt.</summary>
    [JsonPropertyName("defect_quantity")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public double DefectQuantity { get; set; }
}
