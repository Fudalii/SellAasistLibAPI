using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

public class SellasistShipmentDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("order_id")] public int OrderId { get; set; }
    [JsonPropertyName("courier_number")]
    [JsonConverter(typeof(NumberOrStringJsonConverter))]
    public string? CourierNumber { get; set; }

    /// <summary>Numer śledzenia. Sellasist zwraca go raz jako liczbę, raz jako tekst
    /// (zmierzone 2026-08-30) — stąd konwerter number-or-string.</summary>
    [JsonPropertyName("tracking_number")]
    [JsonConverter(typeof(NumberOrStringJsonConverter))]
    public string? TrackingNumber { get; set; }
    [JsonPropertyName("service")] public string? Service { get; set; }
    [JsonPropertyName("service_internal")] public string? ServiceInternal { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("date")] public string? Date { get; set; }
    [JsonPropertyName("shipment_price")] public string? ShipmentPrice { get; set; }
}
