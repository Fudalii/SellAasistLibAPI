using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Producent z endpointu /manufacturers.</summary>
public class SellasistManufacturerResponse
{
    /// <summary>ID producenta. ⚠️ Ten endpoint zwraca `id` jako LICZBĘ (reszta API oddaje id stringiem),
    /// więc bez konwertera cała lista wywalała się na deserializacji i konsument dostawał pustkę.</summary>
    [JsonConverter(typeof(NumberOrStringJsonConverter))]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}
