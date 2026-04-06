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

## Kluczowe DTO

| DTO | Opis |
|-----|------|
| `SellasistOrderResponse` | Zamówienie: adresy, koszyk, koszt wysyłki, pola dodatkowe, external_id |
| `SellasistAddress` | Adres (billing/shipping): dane osobowe, firma, NIP, kraj |
| `SellasistAddAwbRequest` | Request AWB: order_id, tracking_number, courier_number, service |
| `SellasistShipmentDto` | Historia przesyłki: status, daty, cena, numery śledzenia |
| `CourierWebhookResult` | Odpowiedź webhooka kuriera: success, shipment_id, tracking_number, error |

## Zależności

```
Microsoft.Extensions.Http (9.0.4)
Microsoft.Extensions.DependencyInjection.Abstractions (9.0.4)
Microsoft.Extensions.Logging.Abstractions (9.0.4)
```

Target framework: `.NET 10.0`

## Kontekst integracji z B2B

Ta library jest konsumowana przez projekt `B2B` (`d:\Claude\B2B\src`) — używana w warstwie `B2B.Infrastructure` do synchronizacji statusów zamówień i przesyłek między systemem B2B a platformą Sellasist.
