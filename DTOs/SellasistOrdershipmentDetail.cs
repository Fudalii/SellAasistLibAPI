using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Szczegóły listu przewozowego z GET /ordersshipments/{id}. Realny kształt odpowiedzi
/// (zweryfikowany na żywym koncie 2026-06-12, zapytanie po uuid) RÓŻNI SIĘ od spec OpenAPI:
/// {id (uuid string), order_id, integration_type, shipment_type, created_at, updated_at, error,
/// file (TABLICA base64 — jeden PDF per paczka)}. Pola ze spec (service, status, tracking_number…)
/// zostawione jako nullable na wypadek wariantu odpowiedzi przy zapytaniu po id numerycznym.</summary>
public class SellasistOrdershipmentDetail
{
    /// <summary>Identyfikator listu. Przy zapytaniu po ordershipment_uuid Sellasist zwraca tu ten UUID
    /// (string); przy zapytaniu po id numerycznym — liczbę. Konwerter akceptuje oba typy tokenów.</summary>
    [JsonPropertyName("id")]
    [JsonConverter(typeof(NumberOrStringJsonConverter))]
    public string? Id { get; set; }

    [JsonPropertyName("order_id")] public int OrderId { get; set; }

    /// <summary>Integracja kurierska (np. "inpost").</summary>
    [JsonPropertyName("integration_type")] public string? IntegrationType { get; set; }

    /// <summary>Typ przesyłki w integracji (np. "inpostCourierStandard").</summary>
    [JsonPropertyName("shipment_type")] public string? ShipmentType { get; set; }

    [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")] public string? UpdatedAt { get; set; }

    /// <summary>Komunikat błędu integracji kurierskiej (null gdy etykieta wygenerowana poprawnie).</summary>
    [JsonPropertyName("error")] public string? Error { get; set; }

    /// <summary>Etykiety PDF w base64 — TABLICA: jeden list może mieć wiele paczek (PDF per paczka).
    /// Konwerter przyjmuje też pojedynczy string (wariant ze spec). Możliwy prefiks "data:" per element.</summary>
    [JsonPropertyName("file")]
    [JsonConverter(typeof(StringOrStringArrayJsonConverter))]
    public List<string>? Files { get; set; }

    // === Pola ze spec OpenAPI (nieobecne w odpowiedzi po uuid — zostawione dla kompatybilności) ===

    /// <summary>Data utworzenia listu (format "yyyy-MM-dd HH:mm:ss").</summary>
    [JsonPropertyName("date")] public string? Date { get; set; }

    /// <summary>Usługa kurierska (np. "INPOST - Paczka kurierska").</summary>
    [JsonPropertyName("service")] public string? Service { get; set; }

    [JsonPropertyName("service_internal")] public string? ServiceInternal { get; set; }

    /// <summary>Status listu (np. "Wygenerowano przesyłkę.").</summary>
    [JsonPropertyName("status")] public string? Status { get; set; }

    [JsonPropertyName("last_status_check")] public string? LastStatusCheck { get; set; }

    /// <summary>Numer zlecenia przyjazdu kuriera.</summary>
    [JsonPropertyName("courier_number")] public string? CourierNumber { get; set; }

    [JsonPropertyName("tracking_number")] public string? TrackingNumber { get; set; }

    [JsonPropertyName("delivered_date")] public string? DeliveredDate { get; set; }
}
