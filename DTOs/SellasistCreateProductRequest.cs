using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Dane nowego produktu do utworzenia przez POST /products. Wszystkie pola opcjonalne poza
/// <see cref="Title"/> — niewypełnione (null) są pomijane w JSON (DefaultIgnoreCondition.WhenWritingNull).</summary>
public class SellasistCreateProductRequest
{
    /// <summary>Nazwa produktu (wymagane).</summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Numer katalogowy produktu.</summary>
    [JsonPropertyName("catalog")]
    public string? Catalog { get; set; }

    /// <summary>Kod EAN produktu (kluczowy dla matchowania — firma operuje na EAN-ach).</summary>
    [JsonPropertyName("ean")]
    public string? Ean { get; set; }

    /// <summary>Symbol/SKU produktu.</summary>
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    /// <summary>Lokalizacja magazynowa (free-form).</summary>
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    /// <summary>Stan magazynowy początkowy.</summary>
    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }

    /// <summary>Waga produktu (kg).</summary>
    [JsonPropertyName("weight")]
    public decimal? Weight { get; set; }

    /// <summary>Kod waluty (np. "PLN"). Domyślnie ustawiane na walutę sklepu.</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>Cena jednostkowa brutto.</summary>
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    /// <summary>Cena promocyjna brutto.</summary>
    [JsonPropertyName("price_promo")]
    public decimal? PricePromo { get; set; }

    /// <summary>Cena zakupu brutto.</summary>
    [JsonPropertyName("price_buy")]
    public decimal? PriceBuy { get; set; }

    /// <summary>Czy promocja aktywna (0/1).</summary>
    [JsonPropertyName("promotion")]
    public int? Promotion { get; set; }

    /// <summary>Stawka VAT (np. 23).</summary>
    [JsonPropertyName("vat")]
    public int? Vat { get; set; }

    /// <summary>Status produktu (1 = aktywny).</summary>
    [JsonPropertyName("status")]
    public int? Status { get; set; }

    /// <summary>Główna kategoria produktu (ID w Sellasist).</summary>
    [JsonPropertyName("category_id")]
    public int? CategoryId { get; set; }

    /// <summary>Lista ID kategorii produktu.</summary>
    [JsonPropertyName("categories")]
    public List<int>? Categories { get; set; }

    /// <summary>ID grupy wariantów (gdy produkt jest wariantowy).</summary>
    [JsonPropertyName("variants_group_id")]
    public int? VariantsGroupId { get; set; }

    /// <summary>ID jednostki miary.</summary>
    [JsonPropertyName("unit_id")]
    public int? UnitId { get; set; }

    /// <summary>ID czasu dostępności/wysyłki.</summary>
    [JsonPropertyName("availability_id")]
    public int? AvailabilityId { get; set; }

    /// <summary>ID producenta w Sellasist.</summary>
    [JsonPropertyName("manufacturer_id")]
    public int? ManufacturerId { get; set; }

    /// <summary>Pełny opis produktu (HTML).</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Krótki opis produktu.</summary>
    [JsonPropertyName("description_short")]
    public string? DescriptionShort { get; set; }

    /// <summary>Lista URL-i zdjęć. Sellasist pobiera obrazy z podanych adresów — nie trzeba kodować base64.</summary>
    [JsonPropertyName("images")]
    public List<string>? Images { get; set; }
}

/// <summary>Odpowiedź z POST /products — zawiera ID nadane nowemu produktowi.</summary>
public class SellasistCreateProductResponse
{
    /// <summary>ID utworzonego produktu w Sellasist.</summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }
}

/// <summary>Dane zdjęcia do dodania do istniejącego produktu przez POST /images.</summary>
public class SellasistCreateImageRequest
{
    /// <summary>ID produktu, do którego dodajemy zdjęcie.</summary>
    [JsonPropertyName("product_id")]
    public int ProductId { get; set; }

    /// <summary>Obraz zakodowany base64 w formacie "data:image/jpeg;base64,..." (jpg lub png).</summary>
    [JsonPropertyName("image_content")]
    public string ImageContent { get; set; } = string.Empty;

    /// <summary>Nazwa/tytuł zdjęcia.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>Tekst alternatywny (alt).</summary>
    [JsonPropertyName("alt")]
    public string? Alt { get; set; }

    /// <summary>Kolejność wyświetlania zdjęcia w produkcie.</summary>
    [JsonPropertyName("order")]
    public int? Order { get; set; }
}

/// <summary>Odpowiedź z POST /images.</summary>
public class SellasistCreateImageResponse
{
    /// <summary>Status wykonania (np. "success").</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>ID dodanego zdjęcia.</summary>
    [JsonPropertyName("image_id")]
    public int ImageId { get; set; }

    /// <summary>ID zdjęcia w systemie fotolister.pl (jeśli zintegrowany).</summary>
    [JsonPropertyName("fotolister_id")]
    public int? FotolisterId { get; set; }
}
