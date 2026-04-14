namespace Sellasist.DTOs;

/// <summary>Pojedynczy element aktualizacji masowej produktu (PUT /products_bulk)</summary>
public class SellasistProductBulkUpdateItem
{
    public int ProductId { get; set; }

    public string? Quantity { get; set; }
}

/// <summary>Odpowiedź z PUT /products_bulk</summary>
public class SellasistProductBulkUpdateResponse
{
    public string Status { get; set; } = "";

    public List<SellasistProductBulkUpdateLine> Lines { get; set; } = [];

    public string? Message { get; set; }
}

/// <summary>Status jednej linii w odpowiedzi bulk update</summary>
public class SellasistProductBulkUpdateLine
{
    public int Line { get; set; }

    public int? ProductId { get; set; }

    public int? VariantId { get; set; }

    public string Status { get; set; } = "";

    public string? Message { get; set; }
}
