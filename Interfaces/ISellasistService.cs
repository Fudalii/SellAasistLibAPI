using Sellasist.DTOs;

namespace Sellasist.Interfaces;

public interface ISellasistService
{
    // Orders
    Task<SellasistOrderResponse?> GetOrderAsync(int orderId);
    Task<List<SellasistOrderResponse>> GetOrdersByStatusAsync(int statusId, int limit = 50);
    Task<List<SellasistOrderResponse>> GetOrdersWithCartsAsync(int statusId, int limit = 50);

    // Order updates
    Task<bool> UpdateOrderStatusAsync(int orderId, int statusId);
    Task<bool> UpdateAdditionalFieldAsync(int orderId, int fieldId, string value);
    Task<bool> ClearShippingCostAsync(int orderId);
    Task<bool> UpdatePaymentStatusAsync(int orderId, string status);
    Task<bool> SendShipmentCostAsync(int orderId, string cost);
    Task<bool> UpdateOrderTotalAsync(int orderId, string total);

    // AWB / Shipments
    Task<bool> SubmitAwbAsync(SellasistAddAwbRequest request);
    Task<List<SellasistShipmentDto>> GetOrderShipmentsAsync(int orderId);
}
