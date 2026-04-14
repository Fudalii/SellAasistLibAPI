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

**Aktualizacja pól:**
- `UpdateOrderStatusAsync` / `UpdatePaymentStatusAsync`
- `UpdateAdditionalFieldAsync(orderId, fieldId, value)` — własne pola Sellasist
- `ClearShippingCostAsync` / `SendShipmentCostAsync` / `UpdateOrderTotalAsync`

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
| `SellasistOrderResponse` | Zamówienie: adresy, koszyk, koszt wysyłki, pola dodatkowe, external_id |
| `SellasistAddress` | Adres (billing/shipping): dane osobowe, firma, NIP, kraj |
| `SellasistAddAwbRequest` | Request AWB: order_id, tracking_number, courier_number, service |
| `SellasistShipmentDto` | Historia przesyłki: status, daty, cena, numery śledzenia |
| `CourierWebhookResult` | Odpowiedź webhooka kuriera: success, shipment_id, tracking_number, error |
| `SellasistProductBulkItem` | Produkt z listy bulk: ProductId, Sku, Ean, cena, status archiwizacji |
| `SellasistProductResponse` | Szczegóły produktu: opis (datacells), kategorie, zdjęcia, cena promo |
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

## Kontekst integracji z B2B

Ta library jest konsumowana przez projekt `B2B` (`d:\Claude\B2B`) — używana w warstwie `B2B.Infrastructure` do:
- **Importu produktów** — `SellasistProductImportSource` (adapter pattern) pobiera produkty, kategorie, producentów i zdjęcia z Sellasist do lokalnej bazy B2B
- **Importu producentów** — `GetManufacturersAsync()` + mapowanie `manufacturer_id` z produktu na `ProducerId` w B2B z deduplikacją po `NormalizedName`
- **Synchronizacji zamówień** — statusy i przesyłki między systemem B2B a platformą Sellasist
