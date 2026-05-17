using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Szczegóły dokumentu magazynowego z GET /operationdocuments/{id} — zawiera listę linii
/// produktów + adresy buyer/receiver/supplier. UWAGA: Sellasist zwraca tablicę 1-elementową
/// <c>[{...}]</c> zamiast pojedynczego obiektu — deserializacja przez <see cref="SellasistService"/>
/// musi obsłużyć ten kształt (deserialize jako List i wziąć FirstOrDefault).</summary>
public class SellasistOperationDocumentDetail
{
    /// <summary>ID dokumentu.</summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    /// <summary>Numer dokumentu (np. "PZ/1/05/2026").</summary>
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    /// <summary>Niestandardowy numer (zwykle === Number).</summary>
    [JsonPropertyName("custom_number")]
    public string? CustomNumber { get; set; }

    /// <summary>Data wystawienia (ISO "YYYY-MM-DD").</summary>
    [JsonPropertyName("issue_date")]
    public string? IssueDate { get; set; }

    /// <summary>Data sprzedaży (ISO "YYYY-MM-DD").</summary>
    [JsonPropertyName("sale_date")]
    public string? SaleDate { get; set; }

    /// <summary>Typ ("stock").</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Podtyp ("admission" dla PZ, "release" dla WZ).</summary>
    [JsonPropertyName("subtype")]
    public string? Subtype { get; set; }

    /// <summary>Łączna wartość (może być 0 dla PZ wewnętrznych).</summary>
    [JsonPropertyName("total")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? Total { get; set; }

    /// <summary>Waluta ("PLN").</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>Adres kupującego (może być null dla PZ wewnętrznych).</summary>
    [JsonPropertyName("buyer_address")]
    public SellasistOperationDocumentAddress? BuyerAddress { get; set; }

    /// <summary>Adres odbiorcy (zwykle nasz magazyn / firma sklepu).</summary>
    [JsonPropertyName("receiver_address")]
    public SellasistOperationDocumentAddress? ReceiverAddress { get; set; }

    /// <summary>Adres dostawcy (firma od której otrzymujemy towar — relevant dla PZ).</summary>
    [JsonPropertyName("supplier_address")]
    public SellasistOperationDocumentAddress? SupplierAddress { get; set; }

    /// <summary>Komentarze do dokumentu.</summary>
    [JsonPropertyName("comments")]
    public string? Comments { get; set; }

    /// <summary>Czy dokument jest gotowy ("0"/"1" jako string).</summary>
    [JsonPropertyName("is_ready")]
    public string? IsReady { get; set; }

    /// <summary>ID serii dokumentu.</summary>
    [JsonPropertyName("series_id")]
    public int? SeriesId { get; set; }

    /// <summary>Seria dokumentu (id + nazwa, np. "PZ - Przyjęcie zewnętrzne").</summary>
    [JsonPropertyName("series")]
    public SellasistOperationDocumentSeries? Series { get; set; }

    /// <summary>Pozycje produktowe dokumentu.</summary>
    [JsonPropertyName("products")]
    public List<SellasistOperationDocumentProduct> Products { get; set; } = new();
}

/// <summary>Seria dokumentu magazynowego — Sellasist osadza tutaj id+nazwę. Przy tworzeniu PZ
/// trzeba podać <see cref="Id"/> w request (pole series_id). Standardowo dla sklepu cvsklep id=6
/// ("PZ - Przyjęcie zewnętrzne"); inne sklepy mogą mieć inną wartość — odczytujemy z pierwszego
/// dokumentu na liście.</summary>
public class SellasistOperationDocumentSeries
{
    /// <summary>ID serii.</summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    /// <summary>Nazwa serii (np. "PZ - Przyjęcie zewnętrzne").</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
