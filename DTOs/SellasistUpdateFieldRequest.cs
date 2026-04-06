using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

public class SellasistUpdateFieldRequest
{
    [JsonPropertyName("additional_fields")]
    public List<SellasistFieldUpdate> AdditionalFields { get; set; } = [];
}

public class SellasistFieldUpdate
{
    [JsonPropertyName("field_id")] public int FieldId { get; set; }
    [JsonPropertyName("field_value")] public string FieldValue { get; set; } = string.Empty;
}

public class SellasistUpdateStatusRequest
{
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
}
