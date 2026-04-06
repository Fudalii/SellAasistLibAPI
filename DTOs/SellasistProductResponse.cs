namespace Sellasist.DTOs;

/// <summary>Pelna odpowiedz z endpointu /products/{id}.</summary>
public class SellasistProductResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    /// <summary>Cena brutto (string z API).</summary>
    public string? Price { get; set; }
    /// <summary>Cena promocyjna brutto. "0.00" = brak promocji.</summary>
    public string? PricePromo { get; set; }
    /// <summary>Stawka VAT w procentach (np. "23").</summary>
    public string? Vat { get; set; }
    /// <summary>Stan magazynowy (string, np. "11.000").</summary>
    public string? Quantity { get; set; }
    public string? Ean { get; set; }
    /// <summary>Symbol/SKU produktu w Sellasist.</summary>
    public string? Symbol { get; set; }
    public string? CatalogNumber { get; set; }
    public string? Weight { get; set; }
    public string? Status { get; set; }
    public string? CategoryId { get; set; }
    public string? ManufacturerId { get; set; }
    public List<SellasistProductImage>? Images { get; set; }
    /// <summary>Kategorie produktu (embedded array z id/title/parent).</summary>
    public List<SellasistProductCategory>? Categories { get; set; }
    /// <summary>Opis produktu — array datacelli z content (moze zawierac JSON sekcji Allegro).</summary>
    public List<SellasistProductDescription>? Description { get; set; }
}

/// <summary>Zdjecie produktu z Sellasist.</summary>
public class SellasistProductImage
{
    public int Key { get; set; }
    /// <summary>URL pelnorozdzielczego obrazu.</summary>
    public string Original { get; set; } = string.Empty;
    /// <summary>URL miniatury.</summary>
    public string Thumb { get; set; } = string.Empty;
}

/// <summary>Kategoria osadzona w odpowiedzi produktu.</summary>
public class SellasistProductCategory
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Parent { get; set; }
}

/// <summary>Datacell opisu produktu.</summary>
public class SellasistProductDescription
{
    public string? DatacellId { get; set; }
    public string? DatacellName { get; set; }
    /// <summary>Tresc opisu — moze byc JSON sekcji Allegro lub zwykly HTML/tekst.</summary>
    public string? Content { get; set; }
}
