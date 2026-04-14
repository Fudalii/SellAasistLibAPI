using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Sposob dostawy zwrocony z endpointu /shipments (lista metod wysylki skonfigurowanych w sklepie Sellasist).</summary>
public class SellasistShipmentMethodResponse
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
}

/// <summary>Metoda platnosci zwrocona z endpointu /payments (lista metod platnosci skonfigurowanych w sklepie Sellasist).</summary>
public class SellasistPaymentMethodResponse
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
}
