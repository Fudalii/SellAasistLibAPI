using Sellasist.DTOs;

namespace Sellasist.Interfaces;

public interface ISellasistService
{
    /// <summary>Ustawia konfiguracje dynamicznie (np. z bazy danych). Nadpisuje config z DI.</summary>
    void Configure(Sellasist.Config.SellasistConfig newConfig);

    // Order creation
    /// <summary>Tworzy zamówienie w Sellasist (POST /orders). Zwraca odpowiedź z ID lub status "exist" dla duplikatów.</summary>
    Task<SellasistCreateOrderResponse?> CreateOrderAsync(SellasistCreateOrderRequest request);

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
    /// <summary>Aktualizuje stan magazynowy produktu (PUT /products/{productId}).</summary>
    Task<bool> UpdateProductQuantityAsync(int productId, string quantity);

    /// <summary>Masowa aktualizacja produktów (PUT /products_bulk). Max 999 na raz.</summary>
    Task<SellasistProductBulkUpdateResponse?> UpdateProductsBulkAsync(List<SellasistProductBulkUpdateItem> items);

    /// <summary>Pobiera liste produktow z /products_bulk (paginacja po 500). Szybka lista z ID produkty i podstawowymi danymi jak EAN, Symbol...</summary>
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

    // Statuses
    /// <summary>Pobiera liste statusow zamowien z /statuses.</summary>
    Task<List<SellasistStatusResponse>> GetOrderStatusesAsync();

    /// <summary>Pobiera liste metod wysylki skonfigurowanych w sklepie Sellasist (/shipments).</summary>
    Task<List<SellasistShipmentMethodResponse>> GetShipmentMethodsAsync();

    /// <summary>Pobiera liste metod platnosci skonfigurowanych w sklepie Sellasist (/payments).</summary>
    Task<List<SellasistPaymentMethodResponse>> GetPaymentMethodsAsync();

    // Extra fields
    /// <summary>Pobiera liste dodatkowych pól zamówień z /extra-fields.</summary>
    Task<List<SellasistExtraFieldResponse>> GetExtraFieldsAsync();
}
