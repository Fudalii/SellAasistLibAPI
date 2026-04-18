# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Czym jest ten projekt

`SellasistAPI` to **.NET class library** (nie aplikacja) — klient HTTP do integracji z platformą e-commerce [Sellasist](https://sellasist.pl). Przeznaczona do konsumowania przez inne projekty (np. `B2B.Infrastructure`) przez DI.

Brak bazy danych, brak endpointów HTTP — tylko opakowywanie REST API Sellasist.

## Komendy

```bash
# Budowanie
dotnet build Sellasist.csproj
```

Brak testów i runnable app — to library.

## Architektura

Płaska struktura jednego projektu:

```
Config/       – SellasistConfig (Username, ApiToken, BaseUrl = https://{Username}.sellasist.pl/api/v1)
DTOs/         – modele requestów i odpowiedzi (snake_case JSON)
Interfaces/   – ISellasistService (kontrakt)
Services/     – SellasistService (implementacja)
Extensions/   – ServiceCollectionExtensions (rejestracja DI)
```

Rejestracja przez `AddSellasist()` z `ServiceCollectionExtensions` — rejestruje named `HttpClient` `"SellasistApi"` z timeout 30s.

## Wzorzec HTTP

Wszystkie wywołania przechodzą przez jedną metodę:

```csharp
SendRequestAsync<T>(HttpMethod, string endpoint, object? body = null)
```

- Nagłówek autentykacji: `apiKey: {ApiToken}`
- Serializacja: `JsonNamingPolicy.SnakeCaseLower`, `PropertyNameCaseInsensitive`, null omitted, `NumberHandling.AllowReadingFromString`
- Przy błędzie HTTP lub deserializacji: loguje + zwraca `null` (bez rzucania wyjątku)

## Operacje na zamówieniach

**Odczyt:**
- `GetOrderAsync(int orderId)` — pojedyncze zamówienie
- `GetOrdersByStatusAsync(int statusId)` — lista po statusie (bez koszyków), paginacja auto-batch po 50
- `GetOrdersWithCartsAsync(int statusId)` — lista z pozycjami koszyka, paginacja auto-batch po 50
- `GetOrdersAsync(DateTime dateFrom, int limit = 50)` — lista zamówień od daty (GET `/orders?date_from=YYYY-MM-DD`). Paginacja auto-batch. Używana do dwukierunkowej synchronizacji statusu SA → B2B

**Tworzenie zamówień:**
- `CreateOrderAsync(SellasistCreateOrderRequest)` — POST `/orders`, zwraca `SellasistCreateOrderResponse` z `Id`/`OrderId`/`Status` ("ok"/"exist"/null). Status `null` przy `Id > 0` = sukces (niektóre endpointy odpowiadają bez pola `status`)
- `CreateOrderRawAsync(SellasistCreateOrderRequest)` — analogiczne POST, ale zwraca tuple `(int StatusCode, string RawBody, SellasistCreateOrderResponse? Parsed)` dla pełnej diagnostyki (zapis do audit log w consumerze przy błędach HTTP 4xx/5xx)

**Aktualizacja całego zamówienia:**
- `UpdateOrderAsync(int orderId, object body)` — generyczny PUT `/orders/{id}` z dowolnym częściowym body. Używany dla sync: `{ bill_address, shipment_address, shipment_price, status, payment_status, paid, ... }`. Sellasist zachowuje niepodane pola

**Aktualizacja pól (dedykowane metody):**
- `UpdateOrderStatusAsync(orderId, statusId)` / `UpdatePaymentStatusAsync(orderId, status)`
- `UpdateAdditionalFieldAsync(orderId, fieldId, value)` — własne pola Sellasist
- `ClearShippingCostAsync` / `SendShipmentCostAsync` / `UpdateOrderTotalAsync`
- `UpdateOrderBillAddressAsync(orderId, SellasistUpdateBillAddressRequest)` — aktualizacja `bill_address` (PUT `/orders/{id}` z body `{ bill_address: { ... } }`). Dedykowane DTO zawiera 6 pól PL (CompanyName, Street, HomeNumber, FlatNumber, Postcode, City) — kontrakt dla scenariusza GUS w `GUS-Weryfikator`

**Linie zamówień (`/orders_lines`) — dla synchronizacji po utworzeniu:**
- `CreateOrderLineAsync(SellasistOrderLineRequest)` — POST `/orders_lines`. Zwraca `SellasistCreateOrderLineResponse` z `Id` nowej linii (consumer zapisuje do lokalnego pola mappingowego, np. `OrderItem.SellasistLineId`)
- `UpdateOrderLineAsync(int lineId, SellasistOrderLineRequest)` — PUT `/orders_lines/{id}`. Aktualizacja quantity/price/name istniejącej linii
- `DeleteOrderLineAsync(int lineId)` — DELETE `/orders_lines/{id}`

**AWB / Shipments:**
- `SubmitAwbAsync(SellasistAddAwbRequest)` — przesłanie numeru śledzenia do Sellasist
- `GetOrderShipmentsAsync(int orderId)` — historia przesyłek zamówienia

## Operacje na produktach i kategoriach

**Konfiguracja runtime:**
- `Configure(SellasistConfig newConfig)` — nadpisanie credentials w runtime (np. dane z DB zamiast appsettings)

**Produkty:**
- `GetProductsBulkAsync(int limit = 500)` — lista produktów z `/products_bulk`, paginacja auto-batch. Zwraca `SellasistProductBulkItem` (uwaga: pole to `ProductId`, nie `Id`)
- `GetProductAsync(int productId)` — szczegóły produktu z `/products/{id}`. Opis to `List<SellasistProductDescription>` (datacells z Allegro JSON lub HTML). Kategorie osadzone w produkcie jako `List<SellasistProductCategory>`

**Kategorie:**
- `GetCategoriesAsync(int limit = 500)` — lista kategorii, paginacja auto-batch
- `GetCategoryAsync(int categoryId)` — szczegóły kategorii z `/categories/{id}`. Nazwa w `Languages[0].Title`

**Producenci (manufacturers):**
- `GetManufacturersAsync(int limit = 500)` — lista producentów z `/manufacturers`, paginacja auto-batch. Zwraca `SellasistManufacturerResponse` (id, title)

**Statusy zamówień:**
- `GetOrderStatusesAsync()` — lista statusów zamówień z `/statuses`. Pojedyncze żądanie (bez paginacji). Zwraca `SellasistStatusResponse` (id, name). Używane przez consumerów do mapowania własnych statusów B2B → Sellasist przy push zamówień.

## Kluczowe DTO

| DTO | Opis |
|-----|------|
| `SellasistOrderResponse` | Zamówienie: adresy, koszyk (`SellasistCartItem[]` z `Id`, `Symbol`, `Ean`, `Name`, `Quantity`, `Price`), koszt wysyłki, pola dodatkowe, external_id |
| `SellasistAddress` | Adres odczytu (billing/shipping w `SellasistOrderResponse`): pełne dane osobowe, firma, NIP, kraj |
| `SellasistCreateOrderRequest` | Request POST `/orders`: id (string — idempotencja), currency, payment_status, paid, status (int), email, date, shipment_price, payment_id/name, shipment_id/name, invoice, comment, bill_address, shipment_address, carts[], pickup_point? |
| `SellasistCreateOrderAddress` | Adres tworzenia zamówienia (POST `/orders`) — street + home_number + flat_number osobno |
| `SellasistCreateOrderCartItem` | Pozycja koszyka w POST: product_id, variant_id?, name, quantity, price, tax (int %), catalog_number, ean |
| `SellasistCreateOrderResponse` | POST response: `Id` (int), `OrderId` (int), `Status` (string?: "ok"/"exist"/null). Null przy Id > 0 = sukces |
| `SellasistOrderLineRequest` | Request POST/PUT `/orders_lines`: order_id, product_id, variant_id?, name, quantity, price, weight?, additional_information?, selected_options? |
| `SellasistCreateOrderLineResponse` | POST `/orders_lines` response: `Id` (int) nowej linii |
| `SellasistUpdateBillAddressRequest` | Adres aktualizacji bill_address (PUT `/orders/{id}`) — 6 pól PL z GUS |
| `SellasistAddAwbRequest` | Request AWB: order_id, tracking_number, courier_number, service |
| `SellasistShipmentDto` | Historia przesyłki: status, daty, cena, numery śledzenia |
| `CourierWebhookResult` | Odpowiedź webhooka kuriera: success, shipment_id, tracking_number, error |
| `SellasistProductBulkItem` | Produkt z listy bulk: ProductId, Sku, Ean, cena, status archiwizacji |
| `SellasistProductResponse` | Szczegóły produktu: opis (datacells), kategorie, zdjęcia, cena promo, **`AsSet`** (string?, "1"/"0") — czy produkt jest zestawem |
| `SellasistCategoryResponse` | Kategoria z listy: id, parent, title |
| `SellasistCategoryDetailResponse` | Szczegóły kategorii: Languages z tytułami |
| `SellasistManufacturerResponse` | Producent: id, title |
| `SellasistStatusResponse` | Status zamówienia: id (int), name (string) |

## Zależności

```
Microsoft.Extensions.Http (9.0.4)
Microsoft.Extensions.DependencyInjection.Abstractions (9.0.4)
Microsoft.Extensions.Logging.Abstractions (9.0.4)
```

Target framework: `.NET 10.0`

## Uwagi dot. API Sellasist

- Endpoint `/categories` zwraca dużo śmieciowych wpisów ("Nadrzędna Grupa Główna"). Realne kategorie są na dalszych stronach. Lepszym podejściem jest wyciąganie kategorii z detali produktów (`categories` w `SellasistProductResponse`) i pobieranie szczegółów per kategoria via `/categories/{id}`.
- Pole `description` w produkcie to tablica datacells (format Allegro JSON) — wymaga konwersji na HTML. Może też zawierać czysty HTML.
- Produkt bulk zwraca `product_id` (nie `id`) — DTO `SellasistProductBulkItem` mapuje to poprawnie.
- Pola dodatkowe zamówień (`GetExtraFieldsAsync`) — endpoint to `/orders_fields` (NIE `/extra-fields`).
- Pole `as_set` (zestawy) w `SellasistProductResponse` jest typu **`string?`** (nie `bool`!). Sellasist API zwraca `"0"`/`"1"`/`"true"`/`"false"` jako string mimo schematu boolean. Konsumenci muszą parsować: `isSet = AsSet == "1" || AsSet?.Equals("true", IgnoreCase) == true`.
- **Konwencja DTO**: osobne DTO per operacja CRUD na tym samym zasobie, mimo że pola się powtarzają. Adresy: `SellasistAddress` (read), `SellasistCreateOrderAddress` (create), `SellasistUpdateBillAddressRequest` (update). Powód: każdy endpoint Sellasist akceptuje inny zestaw pól; dedykowane DTO dokumentuje kontrakt API i ogranicza błędne użycie.

## Kontekst integracji z B2B

Ta library jest konsumowana przez projekt `B2B` (`d:\Claude\B2B`) — używana w warstwie `B2B.Infrastructure` do:
- **Importu produktów** — `SellasistProductImportSource` (adapter pattern) pobiera produkty, kategorie, producentów i zdjęcia z Sellasist do lokalnej bazy B2B
- **Importu producentów** — `GetManufacturersAsync()` + mapowanie `manufacturer_id` z produktu na `ProducerId` w B2B z deduplikacją po `NormalizedName`
- **Wysyłki zamówień** — `SellasistOrderPushService` (w B2B) woła `CreateOrderRawAsync` po zmianie statusu z flagą `SendToSellasist`. Po sukcesie `GetOrderAsync` pobiera carts i mapuje `SellasistCartItem.Id` na B2B `OrderItem.SellasistLineId` (priorytet matchowania: SKU/Symbol → EAN → Name)
- **Synchronizacji zmian zamówień** — `UpdateOrderAsync` dla adresów/metod/statusu top-level + `CreateOrderLineAsync`/`UpdateOrderLineAsync`/`DeleteOrderLineAsync` dla pozycji koszyka przy manualnym sync z B2B
- **Lekkich aktualizacji** — `UpdateOrderStatusAsync` (zmiana statusu w SA gdy admin zmienia B2B na Cancelled), `UpdateOrderAsync` z `{ payment_status, paid }` (auto po MarkAsPaid)
- **Dwukierunkowego pulla** — `GetOrdersAsync(dateFrom)` co 10 min w `SellasistStatusPullBackgroundService` (B2B). Aktualizuje B2B status gdy SA status różni się i jest zmapowany
- **AWB (inne projekty)** — `SubmitAwbAsync` / `GetOrderShipmentsAsync` w projektach kurierskich (DHL Express, eMAG Courier)

## Diagnostyka

`SendRequestAsync<T>` loguje przy `!IsSuccessStatusCode`: `LogWarning("Sellasist {Method} /{Endpoint} failed: {Status} {Body}")`. Przy deserialization fail: `LogError`. Zwraca `default(T)` (null) — consumer musi to obsłużyć.

Dla krytycznych operacji (tworzenie zamówień) użyj `CreateOrderRawAsync` — zwraca raw body + status code, które consumer zapisuje w audit log (w B2B: `AuditLog.Payload` nvarchar max). Pozwala na debug bez reprodukcji: payload z requestem/response/stack trace'em kopiowany ze strony `/admin/orders/{id}` w modalu "Szczegoly".
