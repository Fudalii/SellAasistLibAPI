namespace Sellasist.DTOs;

/// <summary>Element listy z endpointu /products_bulk.</summary>
public class SellasistProductBulkItem
{
    /// <summary>ID produktu (pole "product_id" w API).</summary>
    public string ProductId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Price { get; set; }
    public string? PricePromo { get; set; }
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
