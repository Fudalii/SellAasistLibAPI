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

    /// <summary>Jak <see cref="GetOrderAsync"/>, ale zwraca też przyczynę niepowodzenia (status HTTP,
    /// błąd parsowania odpowiedzi, błąd sieci) zamiast samego null — do czytelnych komunikatów błędów
    /// w logach konsumenta (np. „HTTP 404" vs „błąd parsowania: Path $.carts[0]...").</summary>
    Task<SellasistOrderFetchResult> GetOrderDetailedAsync(int orderId);

    Task<List<SellasistOrderResponse>> GetOrdersByStatusAsync(int statusId, int limit = 50);

    /// <summary>Jak GetOrdersByStatusAsync, ale rozróżnia błąd od pustej listy: Success=false gdy
    /// dowolna strona paginacji padła (HTTP/sieć/deserializacja) — lista jest wtedy NIEPEŁNA i nie
    /// wolno jej traktować jako stanu statusu. Używać w pollingu sterującym automatyką.</summary>
    Task<(bool Success, List<SellasistOrderResponse> Orders)> TryGetOrdersByStatusAsync(int statusId, int limit = 100);
    Task<List<SellasistOrderResponse>> GetOrdersWithCartsAsync(int statusId, int limit = 50);

    /// <summary>Pobiera zamówienia zmienione od daty dateFrom (paginacja po limit). Używane do synchronizacji statusów SA → B2B.</summary>
    Task<List<SellasistOrderResponse>> GetOrdersAsync(DateTime dateFrom, int limit = 50);

    /// <summary>Zamówienia z oknem czasowym RAZEM z pozycjami koszyka (GET /orders_with_carts
    /// z date_from/date_to). Endpoint przyjmuje filtry daty, mimo że dokumentacja Sellasist wymienia
    /// przy nim tylko status_id — zweryfikowane na żywym koncie 2026-08-31.
    /// <para>Okno jest domknięte z dołu i OTWARTE z góry: <c>[dateFrom, dateToExclusive)</c>.
    /// Dzień <paramref name="dateToExclusive"/> NIE wchodzi do wyniku, więc jeden dzień to
    /// <c>(d, d.AddDays(1))</c>. Granica włączająca zgubiłaby wszystkie dni poza pierwszym.</para>
    /// <para>Wyniki posortowane malejąco po dacie. Wyjście offsetem poza zbiór zwraca HTTP 404,
    /// które warstwa transportowa traktuje jak pustą listę — paginacja kończy się wtedy normalnie.</para>
    /// </summary>
    /// <param name="dateFrom">Początek okna (włącznie), rozdzielczość dnia.</param>
    /// <param name="dateToExclusive">Koniec okna (wyłącznie), rozdzielczość dnia.</param>
    /// <param name="limit">Rekordów na stronę — twardy sufit API to 100.</param>
    Task<List<SellasistOrderResponse>> GetOrdersWithCartsByDateAsync(
        DateTime dateFrom, DateTime dateToExclusive, int limit = 100);

    // Order updates
    Task<bool> UpdateOrderStatusAsync(int orderId, int statusId);

    /// <summary>Masowa aktualizacja zamówień (PUT /orders_bulk, max 1000 pozycji) — np. zmiana statusu
    /// wielu zamówień JEDNYM żądaniem (oszczędza limit 100 zapytań/min). Odpowiedź zawiera wynik
    /// per pozycja (lines[].status: success/error). Null = błąd HTTP całego żądania.</summary>
    Task<SellasistOrdersBulkResponse?> UpdateOrdersBulkAsync(List<SellasistOrderBulkUpdateItem> orders);
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

    /// <summary>Generyczny PUT /orders_lines/{lineId} z partial body — aktualizuje tylko podane pola linii
    /// (np. <c>weight</c>, <c>additional_information</c>), pozostałe zostają bez zmian. Bezpieczniejsze niż pełny
    /// rebuild <see cref="SellasistOrderLineRequest"/> gdy zmieniamy jedno pole (brak ryzyka nadpisania ceny/ilości).
    /// <para>UWAGA: <c>signature</c> i <c>catalog_number</c> są przez API TYLKO DO ODCZYTU — PUT zwraca 200
    /// i po cichu je ignoruje. Zero w <c>weight</c> zapisuje wyłącznie string "0.000"; <c>0</c> i "0" są pomijane.</para></summary>
    Task<bool> UpdateOrderLineRawAsync(int lineId, object body);

    /// <summary>PUT /orders_lines/{lineId} — zapisuje pola dodatkowe POZYCJI zamówienia
    /// (<c>carts[].additional_fields</c>, czyli „Pola danych" o dostępności „Produkt zamówienia").
    /// Nie rusza pozostałych pól linii ani pozostałych linii zamówienia.
    /// <para>Pusta wartość (pusty string, <c>null</c>, spacja) CZYŚCI pole — inaczej niż
    /// <c>additional_information</c>, gdzie czyści wyłącznie spacja.</para>
    /// <para>Pole niedostępne dla pozycji zamówienia jest pomijane po cichu: odpowiedź 200, a przy kolejnym
    /// odczycie wartości nie ma. Po zapisie warto zweryfikować odczytem.</para>
    /// <para>Zweryfikowane na żywo 2026-08-30 (konto electroskypl, zamówienie 444).</para></summary>
    Task<bool> UpdateOrderLineAdditionalFieldsAsync(int lineId, IEnumerable<SellasistFieldUpdate> fields);

    /// <summary>DELETE /orders_lines/{lineId} — usuwa linie z zamowienia. Uzywane do synchronizacji B2B:
    /// gdy admin usunie pozycje z juz-wyslanego zamowienia.</summary>
    Task<bool> DeleteOrderLineAsync(int lineId);

    // AWB / Shipments
    Task<bool> SubmitAwbAsync(SellasistAddAwbRequest request);
    Task<List<SellasistShipmentDto>> GetOrderShipmentsAsync(int orderId);

    /// <summary>Pobiera szczegóły pojedynczego listu przewozowego (GET /ordersshipments/{id}) wraz z etykietą PDF
    /// w polu <c>File</c> (base64, możliwy prefiks "data:"). Jako identyfikator przyjmuje <c>ordershipment_uuid</c>
    /// z GET /orders/{id} (pole shipments[].ordershipment_uuid) lub numeryczne id listu. Zwraca null gdy brak listu.</summary>
    Task<SellasistOrdershipmentDetail?> GetOrdershipmentAsync(string uuidOrId);

    // Products
    /// <summary>Tworzy nowy produkt w Sellasist (POST /products). Zwraca odpowiedź z nadanym ID.
    /// Pole images[] przyjmuje URL-e zdjęć — Sellasist pobiera obrazy samodzielnie (base64 niepotrzebne).</summary>
    Task<SellasistCreateProductResponse?> CreateProductAsync(SellasistCreateProductRequest request);

    /// <summary>Dodaje zdjęcie do istniejącego produktu (POST /images). image_content w formacie
    /// base64 "data:image/jpeg;base64,...". Fallback gdy URL zdjęcia nie jest publicznie dostępny dla Sellasist.</summary>
    Task<SellasistCreateImageResponse?> AddProductImageAsync(SellasistCreateImageRequest request);

    /// <summary>Zdjęcia produktu (GET /images/{productId}) — z ID każdego zdjęcia i datą wgrania.
    /// Osobna rodzina endpointów: <c>PUT /products/{id}</c> z polem <c>images</c> tylko DODAJE zdjęcia do galerii
    /// (pusta tablica ani flaga zastępowania nic nie robią), więc podmiana wymaga skasowania starych przez
    /// <see cref="DeleteProductImageAsync"/>. Zweryfikowane na żywym API 2026-08-26.</summary>
    Task<List<SellasistProductImageItem>> GetProductImagesAsync(int productId);

    /// <summary>Usuwa jedno zdjęcie produktu (DELETE /images/{imageId}). <paramref name="imageId"/> to ID ZDJĘCIA
    /// z <see cref="GetProductImagesAsync"/>, nie ID produktu.</summary>
    Task<bool> DeleteProductImageAsync(int imageId);

    /// <summary>Aktualizuje stan magazynowy produktu (PUT /products/{productId}).</summary>
    Task<bool> UpdateProductQuantityAsync(int productId, string quantity);

    /// <summary>Generyczny PUT /products/{id} z partial body — aktualizuje tylko podane pola produktu
    /// (np. title, symbol, catalog, vat, manufacturer_id, description, images, status). Pola pominięte
    /// pozostają bez zmian. Używane do aktualizacji istniejących produktów KQS→Sellasist (bez ceny/stanu).</summary>
    Task<bool> UpdateProductAsync(int productId, object body);

    /// <summary>Masowa aktualizacja produktów (PUT /products_bulk). Max 999 na raz.</summary>
    Task<SellasistProductBulkUpdateResponse?> UpdateProductsBulkAsync(List<SellasistProductBulkUpdateItem> items);

    /// <summary>Pobiera stany magazynowe (GET /products_stock). Bez filtra zwraca całą listę stronami.</summary>
    Task<List<SellasistProductStock>> GetProductsStockAsync(int limit = 100, CancellationToken ct = default);

    /// <summary>Pobiera stan jednego produktu wskazanego symbolem (SKU). Zwraca null, gdy produktu nie ma.</summary>
    Task<SellasistProductStock?> GetProductStockBySymbolAsync(string symbol, CancellationToken ct = default);

    /// <summary>Masowa aktualizacja stanów magazynowych (PUT /products_stock). Endpoint przyjmuje do 1000
    /// pozycji na żądanie — większe listy dzielone są automatycznie. Zwraca pozycje potwierdzone przez API.</summary>
    Task<List<SellasistProductStock>> UpdateProductsStockAsync(List<SellasistProductStockUpdate> items, CancellationToken ct = default);

    /// <summary>Pobiera liste produktow z /products_bulk (paginacja po 500). Szybka lista z ID produkty i podstawowymi danymi jak EAN, Symbol...</summary>
    Task<List<SellasistProductBulkItem>> GetProductsBulkAsync(int limit = 500);

    /// <summary>Pobiera liste produktow z /products (paginacja po 100 — limit endpointu). W odroznieniu
    /// od /products_bulk zwraca takze <c>image_url</c> (miniature) — idealne do hurtowego cache zdjec.</summary>
    Task<List<SellasistProductListItem>> GetProductsAsync(int limit = 100);

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

    /// <summary>Pobiera słownik krajów (GET /countries) — <c>{ id, code (ISO-2), name }</c>. Słownik jest globalny
    /// dla platformy (te same id we wszystkich sklepach; PL = 170). Używaj do wypełnienia
    /// <see cref="SellasistCreateOrderAddress.Country"/> — Sellasist przyjmuje kraj tylko jako zagnieżdżony obiekt.</summary>
    Task<List<SellasistCountry>> GetCountriesAsync();

    // Extra fields
    /// <summary>Pobiera katalog pól dodatkowych ZAMÓWIENIA z GET /orders_fields (<c>{id, name, type}</c>).
    /// <para>To jeden z TRZECH rozłącznych bytów w Sellasist: (1) pola dodatkowe zamówienia — ten endpoint,
    /// wartości w <c>orders.additional_fields</c>, zapis przez <see cref="UpdateAdditionalFieldAsync"/>;
    /// (2) „Pola danych" POZYCJI (panel: Treści → Pola danych, dostępność „Produkt zamówienia") — wartości
    /// w <c>orders.carts[].additional_fields</c>, zapis przez <see cref="UpdateOrderLineAdditionalFieldsAsync"/>,
    /// <b>bez endpointu katalogowego</b> (ID przepisuje się z adresu edycji pola w panelu);
    /// (3) pola dodatkowe PRODUKTU — GET /products_fields.</para>
    /// <para>Uwaga na <c>type</c>: do pola plikowego (<c>saledocument</c>, <c>files</c>) nie wolno wysyłać
    /// zwykłego tekstu — Sellasist potraktuje go jak base64 pliku i zapisze śmieci bez błędu.</para></summary>
    Task<List<SellasistExtraFieldResponse>> GetExtraFieldsAsync();

    // Operation documents (dokumenty magazynowe — PZ, WZ)

    /// <summary>Pobiera listę dokumentów magazynowych (GET /operationdocuments). Dla PZ użyj domyślnych:
    /// type="stock", subtype="admission". Sort "desc" zwraca najnowsze na górze. <paramref name="seriesId"/>
    /// (gdy &gt; 0) filtruje po serii dokumentu (query <c>series_id</c>) — pozwala pobierać tylko dokumenty
    /// z wybranej serii (np. PZ tworzone z kolektora vs korekty automatyczne).</summary>
    Task<List<SellasistOperationDocumentListItem>> GetOperationDocumentsAsync(
        string type = "stock", string subtype = "admission", string sort = "desc", int? seriesId = null);

    /// <summary>Pobiera szczegóły dokumentu magazynowego z listą pozycji produktowych (GET /operationdocuments/{id}).</summary>
    Task<SellasistOperationDocumentDetail?> GetOperationDocumentAsync(int documentId);

    /// <summary>Pobiera listę serii dokumentów operacyjnych (GET /operationdocuments_series) — źródło
    /// wartości <c>series_id</c> dla /operationdocuments. UWAGA: to INNY endpoint niż /documents_series
    /// (tamten dotyczy dokumentów sprzedażowych). Dla serii PZ filtruj po stronie klienta:
    /// Type="stock" + Subtype="admission".</summary>
    Task<List<SellasistOperationDocumentSeriesListItem>> GetOperationDocumentSeriesAsync();

    /// <summary>Tworzy nowy dokument magazynowy (POST /operationdocuments). Dla PZ ustaw Type="stock" i Subtype="admission".
    /// Zwraca odpowiedź z ID i numerem nadanym przez Sellasist.</summary>
    Task<SellasistCreateOperationDocumentResponse?> CreateOperationDocumentAsync(SellasistCreateOperationDocumentRequest request);

    /// <summary>Aktualizuje istniejący dokument magazynowy (PUT /operationdocuments/{id}) z dowolnym
    /// partial body. Używane np. do dodania pola <c>comments</c> po utworzeniu PZ (POST z comments
    /// łamie parser Sellasist, ale PUT comments działa). Zwraca true gdy serwer odpowiedział "ok".</summary>
    Task<bool> UpdateOperationDocumentAsync(int documentId, object body);

    // Cloud Print (druk dokumentów na stanowiskach WMS — przez główne API Sellasist, ten sam apiKey)

    /// <summary>Zleca wydruk dokumentu przez Sellasist Cloud Print (POST /printfile) — na głównym API sklepu
    /// (host i apiKey z <see cref="Configure"/>). Zwraca status HTTP + surowe body — sukces = 2xx; nie rzuca
    /// wyjątku. Zlecenie odbiera i drukuje aplikacja desktopowa Sellasist Cloud Print na stanowisku WMS.</summary>
    Task<(int StatusCode, string RawBody)> PrintFileAsync(SellasistPrintFileRequest request);

    /// <summary>Pobiera listę stanowisk/drukarek Cloud Print (GET /printerpoints) — źródło <c>printerPointId</c>
    /// do wyboru w żądaniu druku.</summary>
    Task<List<SellasistPrinterPoint>> GetPrinterPointsAsync();
}
