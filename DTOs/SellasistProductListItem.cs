namespace Sellasist.DTOs;

/// <summary>
/// Element listy z endpointu <c>GET /products</c> (paginacja po 100).
/// Od 2026-06 Sellasist zwraca tu komplet pol listowych wraz z <c>image_url</c> (miniatura) —
/// dzieki temu bulk pobranie produktow daje od razu adresy zdjec bez osobnego <c>/products/{id}</c>.
/// Deserializacja przez <c>JsonNamingPolicy.SnakeCaseLower</c> (np. <c>image_url</c> -> <see cref="ImageUrl"/>).
/// </summary>
public class SellasistProductListItem
{
    /// <summary>ID produktu (pole "id" w API — string, np. "1").</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Nazwa produktu (pole "name").</summary>
    public string? Name { get; set; }

    /// <summary>Numer katalogowy.</summary>
    public string? CatalogNumber { get; set; }

    /// <summary>Kod EAN produktu.</summary>
    public string? Ean { get; set; }

    /// <summary>Symbol / SKU produktu.</summary>
    public string? Symbol { get; set; }

    /// <summary>Lokalizacja magazynowa.</summary>
    public string? Location { get; set; }

    /// <summary>Objetosc (string, np. "0").</summary>
    public string? Volume { get; set; }

    /// <summary>Stan zarezerwowany — moze byc null gdy brak rezerwacji.</summary>
    public string? Reserved { get; set; }

    /// <summary>Cena brutto (string z API, np. "0.00").</summary>
    public string? Price { get; set; }

    /// <summary>Cena promocyjna brutto. "0.00" = brak promocji.</summary>
    public string? PricePromo { get; set; }

    /// <summary>Stan magazynowy (string, np. "999999.000").</summary>
    public string? Quantity { get; set; }

    /// <summary>Waluta ceny (np. "PLN").</summary>
    public string? Currency { get; set; }

    /// <summary>URL miniatury zdjecia (pole "image_url"); null gdy produkt nie ma zdjecia.
    /// Pelne zdjecie = ten adres z podmienionym segmentem /t/ na /n/.</summary>
    public string? ImageUrl { get; set; }
}
