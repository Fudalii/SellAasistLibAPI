using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Log operacji na zamówieniu z /orders_logs. Sellasist zwraca id/order_id czasami jako string,
/// czasami jako int — stąd <c>JsonNumberHandling.AllowReadingFromString</c>.</summary>
public class SellasistOrderLog
{
    /// <summary>Identyfikator loga.</summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    /// <summary>Identyfikator zamówienia którego dotyczy log.</summary>
    [JsonPropertyName("order_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int OrderId { get; set; }

    /// <summary>Id użytkownika który wykonał operację (może być null dla akcji systemowych).</summary>
    [JsonPropertyName("user_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? UserId { get; set; }

    /// <summary>Tag operacji. Możliwe: order_create, order_edit, order_status_change, order_delete,
    /// order_merge, invoice_create, invoice_remove, receipt_create, receipt_remove.</summary>
    [JsonPropertyName("operation_tag")]
    public string OperationTag { get; set; } = string.Empty;

    /// <summary>Powiązany identyfikator obiektu (dla order_status_change = id nowego statusu,
    /// dla order_merge/invoice_*/receipt_* = odpowiednio zależne od operacji).</summary>
    [JsonPropertyName("object_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? ObjectId { get; set; }

    /// <summary>Opis akcji — może zawierać HTML (np. "Zmiana statusu z &lt;b&gt;X&lt;/b&gt; na &lt;b&gt;Y&lt;/b&gt;").</summary>
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    /// <summary>Data operacji w formacie "yyyy-MM-dd HH:mm:ss" (czas lokalny Sellasist, bez strefy).</summary>
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
}
