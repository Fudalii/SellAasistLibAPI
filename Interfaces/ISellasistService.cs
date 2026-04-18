using Sellasist.DTOs;

namespace Sellasist.Interfaces;

public interface ISellasistService
{
    /// <summary>Ustawia konfiguracje dynamicznie (np. z bazy danych). Nadpisuje config z DI.</summary>
    void Configure(Sellasist.Config.SellasistConfig newConfig);

    // Order creation
    /// <summary>Tworzy zamówienie w Sellasist (POST /orders). Zwraca odpowiedź z ID lub status "exist" dla duplikatów.</summary>
    Task<SellasistCreateOrderResponse?> CreateOrderAsync(SellasistCreateOrderRequest request);

    /// <summary>Tworzy zamówienie z pełną diagnostyką HTTP — zwraca status code, raw body oraz parsed response.
    /// Używane gdy potrzebna pełna diagnostyka błędu (np. zapis do audit log).</summary>
    Task<(int StatusCode, string RawBody, SellasistCreateOrderResponse? Parsed)> CreateOrderRawAsync(SellasistCreateOrderRequest request);

    // Orders
    Task<SellasistOrderResponse?> GetOrderAsync(int orderId);
    Task<List<SellasistOrderResponse>> GetOrdersByStatusAsync(int statusId, int limit = 50);
    Task<List<SellasistOrderResponse>> GetOrdersWithCartsAsync(int statusId, int limit = 50);

    /// <summary>Pobiera zamówienia zmienione od daty dateFrom (paginacja po limit). Używane do synchronizacji statusów SA → B2B.</summary>
    Task<List<SellasistOrderResponse>> GetOrdersAsync(DateTime dateFrom, int limit = 50);

    // Order updates
    Task<bool> UpdateOrderStatusAsync(int orderId, int statusId);
    Task<bool> UpdateAdditionalFieldAsync(int orderId, int fieldId, string value);
    Task<bool> ClearShippingCostAsync(int orderId);
    Task<bool> UpdatePaymentStatusAsync(int orderId, string status);
    Task<bool> SendShipmentCostAsync(int orderId, string cost);
    Task<bool> UpdateOrderTotalAsync(int orderId, string total);

    /// <summary>Aktualizuje dane adresowe dokumentu sprzedaży (bill_address) na zamówieniu</summary>
    Task<bool> UpdateOrderBillAddressAsync(int orderId, SellasistUpdateBillAddressRequest address);

    /// <summary>Generyczny PUT /orders/{id} z obiektem — dowolne pola (bill_address, shipment_address, shipment_price,
    /// status, payment_status, paid, itp.). Uzywane przez sync B2B → Sellasist po edycji zamowienia w adminie.</summary>
    Task<bool> UpdateOrderAsync(int orderId, object body);

    // Order lines

    /// <summary>POST /orders_lines — dodaje nowa linie do istniejacego zamowienia. Zwraca ID nowej linii
    /// lub null przy bledzie. Uzywane do synchronizacji B2B: gdy admin doda produkt do juz-wyslanego zamowienia.</summary>
    Task<SellasistCreateOrderLineResponse?> CreateOrderLineAsync(SellasistOrderLineRequest request);

    /// <summary>PUT /orders_lines/{lineId} — aktualizuje istniejaca linie (quantity, price, name, itp.).
    /// Uzywane do synchronizacji B2B: gdy admin zmieni ilosc lub cene pozycji w juz-wyslanym zamowieniu.</summary>
    Task<bool> UpdateOrderLineAsync(int lineId, SellasistOrderLineRequest request);

    /// <summary>DELETE /orders_lines/{lineId} — usuwa linie z zamowienia. Uzywane do synchronizacji B2B:
    /// gdy admin usunie pozycje z juz-wyslanego zamowienia.</summary>
    Task<bool> DeleteOrderLineAsync(int lineId);

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
