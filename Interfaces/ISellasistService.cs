using Sellasist.DTOs;

namespace Sellasist.Interfaces;

public interface ISellasistService
{
    /// <summary>Ustawia konfiguracje dynamicznie (np. z bazy danych). Nadpisuje config z DI.</summary>
    void Configure(Sellasist.Config.SellasistConfig newConfig);

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

    // Products
    /// <summary>Pobiera liste produktow z /products_bulk (paginacja po 500).</summary>
    Task<List<SellasistProductBulkItem>> GetProductsBulkAsync(int limit = 500);

    /// <summary>Pobiera szczegoly produktu z /products/{productId}.</summary>
    Task<SellasistProductResponse?> GetProductAsync(int productId);

    // Categories
    /// <summary>Pobiera liste kategorii z /categories (paginacja po 500).</summary>
    Task<List<SellasistCategoryResponse>> GetCategoriesAsync(int limit = 500);

    /// <summary>Pobiera szczegoly kategorii z /categories/{id}.</summary>
    Task<SellasistCategoryDetailResponse?> GetCategoryAsync(int categoryId);

    // Manufacturers
    /// <summary>Pobiera liste producentow z /manufacturers (paginacja po 500).</summary>
    Task<List<SellasistManufacturerResponse>> GetManufacturersAsync(int limit = 500);
}
