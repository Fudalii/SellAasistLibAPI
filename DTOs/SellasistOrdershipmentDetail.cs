using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Szczegóły listu przewozowego z GET /ordersshipments/{id} — w polu <see cref="File"/>
/// etykieta PDF w base64 (możliwy prefiks "data:" — konsument musi go obciąć przed dekodowaniem).</summary>
public class SellasistOrdershipmentDetail
{
    /// <summary>Identyfikator listu. UWAGA: gdy zapytanie idzie po ordershipment_uuid, Sellasist zwraca
    /// tu ten UUID (string, zweryfikowane 2026-06-12); przy zapytaniu po id numerycznym — liczbę.
    /// Stąd string + konwerter akceptujący oba typy tokenów JSON.</summary>
    [JsonPropertyName("id")]
    [JsonConverter(typeof(NumberOrStringJsonConverter))]
    public string? Id { get; set; }

    /// <summary>Data utworzenia listu (format "yyyy-MM-dd HH:mm:ss").</summary>
    [JsonPropertyName("date")] public string? Date { get; set; }

    /// <summary>Usługa kurierska (np. "INPOST - Paczka kurierska").</summary>
    [JsonPropertyName("service")] public string? Service { get; set; }

    [JsonPropertyName("service_internal")] public string? ServiceInternal { get; set; }

    [JsonPropertyName("order_id")] public int OrderId { get; set; }

    /// <summary>Status listu (np. "Wygenerowano przesyłkę.").</summary>
    [JsonPropertyName("status")] public string? Status { get; set; }

    [JsonPropertyName("last_status_check")] public string? LastStatusCheck { get; set; }

    /// <summary>Numer zlecenia przyjazdu kuriera.</summary>
    [JsonPropertyName("courier_number")] public string? CourierNumber { get; set; }

    [JsonPropertyName("tracking_number")] public string? TrackingNumber { get; set; }

    [JsonPropertyName("delivered_date")] public string? DeliveredDate { get; set; }

    /// <summary>Etykieta PDF w base64 (możliwy prefiks "data:" lub "data:application/pdf;base64,").</summary>
    [JsonPropertyName("file")] public string? File { get; set; }
}
