namespace Sellasist.DTOs;

/// <summary>Stan magazynowy produktu zwracany przez GET/PUT /products_stock (od wersji API 1.90.8).</summary>
public class SellasistProductStock
{
    /// <summary>ID produktu w Sellasist.</summary>
    public int Id { get; set; }

    /// <summary>Numer katalogowy.</summary>
    public string? CatalogNumber { get; set; }

    /// <summary>Kod EAN.</summary>
    public string? Ean { get; set; }

    /// <summary>Symbol produktu (SKU).</summary>
    public string? Symbol { get; set; }

    /// <summary>Stan magazynowy.</summary>
    public int Quantity { get; set; }
}

/// <summary>Pojedyncza pozycja masowej aktualizacji stanów (PUT /products_stock). Wymagane jest
/// <see cref="Quantity"/> oraz przynajmniej jeden identyfikator — przy synchronizacji po SKU
/// wypełniamy wyłącznie <see cref="Symbol"/>.</summary>
public class SellasistProductStockUpdate
{
    public int? Id { get; set; }

    public string? Ean { get; set; }

    /// <summary>Symbol produktu (SKU) — identyfikator używany przy mapowaniu z systemu zewnętrznego.</summary>
    public string? Symbol { get; set; }

    public string? CatalogNumber { get; set; }

    /// <summary>Nowy stan magazynowy. API przyjmuje wyłącznie liczbę całkowitą.</summary>
    public int Quantity { get; set; }
}

/// <summary>Koperta żądania PUT /products_stock.</summary>
public class SellasistProductStockUpdateRequest
{
    public List<SellasistProductStockUpdate> Products { get; set; } = [];
}
