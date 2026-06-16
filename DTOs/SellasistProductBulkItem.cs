namespace Sellasist.DTOs;

/// <summary>Element listy z endpointu /products_bulk.</summary>
public class SellasistProductBulkItem
{
    /// <summary>ID produktu (pole "product_id" w API).</summary>
    public string ProductId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Price { get; set; }
    public string? PricePromo { get; set; }
    /// <summary>Cena zakupu produktu (pole "price_buy" w API, od v1.86.0 GET /products_bulk). Moze byc netto lub brutto — interpretacja po stronie integracji.</summary>
    public string? PriceBuy { get; set; }
    public string? Quantity { get; set; }
    public bool Archived { get; set; }

    /// <summary>Kod EAN produktu.</summary>
    public string? Ean { get; set; }

    /// <summary>Numer katalogowy.</summary>
    public string? CatalogNumber { get; set; }

    /// <summary>Symbol produktu.</summary>
    public string? Symbol { get; set; }

    /// <summary>Lokalizacja produktu w magazynie.</summary>
    public string? Location { get; set; }
}
