using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Struktura pozycji zamowienia uzywana przy POST /orders_lines (dodanie nowej linii do istniejacego zamowienia)
/// oraz PUT /orders_lines/{lineId} (aktualizacja istniejacej linii). OrderId wymagany dla POST, opcjonalny dla PUT.</summary>
public class SellasistOrderLineRequest
{
    /// <summary>Id zamowienia Sellasist (wymagane dla POST, opcjonalne dla PUT).</summary>
    [JsonPropertyName("order_id")]
    public int OrderId { get; set; }

    /// <summary>Id produktu Sellasist (wymagane).</summary>
    [JsonPropertyName("product_id")]
    public int ProductId { get; set; }

    /// <summary>Id wariantu produktu (opcjonalne — B2B nie uzywa wariantow).</summary>
    [JsonPropertyName("variant_id")]
    public int? VariantId { get; set; }

    /// <summary>Nazwa pozycji (snapshot tytulu produktu z chwili zakupu).</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Ilosc sztuk.</summary>
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    /// <summary>Cena jednostkowa brutto (PLN).</summary>
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    /// <summary>Waga w kg (opcjonalna).</summary>
    [JsonPropertyName("weight")]
    public decimal? Weight { get; set; }

    /// <summary>Dodatkowe informacje wyswietlane przy pozycji (opcjonalne).</summary>
    [JsonPropertyName("additional_information")]
    public string? AdditionalInformation { get; set; }

    /// <summary>Wybrane opcje produktu (np. rozmiar, kolor) — B2B nie uzywa.</summary>
    [JsonPropertyName("selected_options")]
    public string? SelectedOptions { get; set; }
}

/// <summary>Odpowiedz POST /orders_lines — zwraca id nowej linii.</summary>
public class SellasistCreateOrderLineResponse
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }
}
