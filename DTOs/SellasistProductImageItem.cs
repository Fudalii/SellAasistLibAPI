using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Zdjęcie produktu zwracane przez <c>GET /images/{product_id}</c>. Uwaga: <see cref="Id"/> to identyfikator
/// SAMEGO ZDJĘCIA (używany w <c>DELETE /images/{image_id}</c>), a produkt wskazuje <see cref="ElementId"/> —
/// dla pierwszego zdjęcia produktu obie liczby bywają równe, co łatwo pomylić.</summary>
public class SellasistProductImageItem
{
    /// <summary>ID zdjęcia — to jego podaje się przy usuwaniu (<c>DELETE /images/{id}</c>).</summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>ID produktu, do którego należy zdjęcie.</summary>
    [JsonPropertyName("element_id")]
    public int ElementId { get; set; }

    /// <summary>Data wgrania zdjęcia do Sellasist ("yyyy-MM-dd HH:mm:ss").</summary>
    [JsonPropertyName("creation_date")]
    public string? CreationDate { get; set; }

    /// <summary>URL pełnowymiarowego zdjęcia w CDN Sellasist (nie źródłowy adres, z którego je pobrano).</summary>
    [JsonPropertyName("original_url")]
    public string? OriginalUrl { get; set; }

    /// <summary>URL miniatury.</summary>
    [JsonPropertyName("thumb_url")]
    public string? ThumbUrl { get; set; }
}
