using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Pozycja masowej aktualizacji zamówień (PUT /orders_bulk) — pola null są pomijane
/// w JSON (partial update), więc klasę można rozszerzać o kolejne pola zamówienia.</summary>
public class SellasistOrderBulkUpdateItem
{
    [JsonPropertyName("order_id")] public int OrderId { get; set; }

    /// <summary>Docelowy status zamówienia.</summary>
    [JsonPropertyName("status_id")] public int? StatusId { get; set; }
}

/// <summary>Odpowiedź PUT /orders_bulk — status per pozycja (linia).</summary>
public class SellasistOrdersBulkResponse
{
    /// <summary>"SUCCESS" gdy zapytanie przetworzone (poszczególne linie mogą mieć błędy).</summary>
    [JsonPropertyName("status")] public string? Status { get; set; }

    [JsonPropertyName("lines")] public List<SellasistOrdersBulkLine>? Lines { get; set; }
}

/// <summary>Wynik pojedynczej pozycji masowej aktualizacji.</summary>
public class SellasistOrdersBulkLine
{
    /// <summary>Numer linii w żądaniu (1-based).</summary>
    [JsonPropertyName("line")] public int Line { get; set; }

    [JsonPropertyName("order_id")] public int OrderId { get; set; }

    /// <summary>"success" albo "error".</summary>
    [JsonPropertyName("status")] public string? Status { get; set; }

    /// <summary>Komunikat błędu (np. "item_not_found") — null przy sukcesie.</summary>
    [JsonPropertyName("message")] public string? Message { get; set; }
}
