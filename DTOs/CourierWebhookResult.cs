namespace Sellasist.DTOs;

public record CourierWebhookResult(
    bool Success,
    Guid? ShipmentId = null,
    string? TrackingNumber = null,
    string? Error = null
);
