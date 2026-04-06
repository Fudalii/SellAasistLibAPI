using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

public class SellasistAddAwbRequest
{
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
    [JsonPropertyName("service")] public string Service { get; set; } = string.Empty;
    [JsonPropertyName("service_internal")] public string ServiceInternal { get; set; } = string.Empty;
    [JsonPropertyName("integration_type")] public string IntegrationType { get; set; } = string.Empty;
    [JsonPropertyName("shipment_type")] public string ShipmentType { get; set; } = string.Empty;
    [JsonPropertyName("order_id")] public int OrderId { get; set; }
    [JsonPropertyName("shipment_price")] public string ShipmentPrice { get; set; } = "0.00";
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("last_status_check")] public string LastStatusCheck { get; set; } = string.Empty;
    [JsonPropertyName("courier_number")] public string CourierNumber { get; set; } = string.Empty;
    [JsonPropertyName("tracking_number")] public string TrackingNumber { get; set; } = string.Empty;
    [JsonPropertyName("delivered_date")] public string DeliveredDate { get; set; } = string.Empty;
    [JsonPropertyName("created_at")] public string CreatedAt { get; set; } = string.Empty;
    [JsonPropertyName("updated_at")] public string UpdatedAt { get; set; } = string.Empty;
    [JsonPropertyName("file")] public string? File { get; set; }
}
