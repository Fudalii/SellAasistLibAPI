namespace Sellasist.DTOs;

/// <summary>Wynik sprawdzenia danych dostępowych do API.
///
/// <para><b>Po co osobny typ.</b> Zwykłe metody słownikowe (<c>GetOrderStatusesAsync</c> i pokrewne)
/// połykają błąd HTTP i zwracają pustą listę — to jest właściwe zachowanie dla importu, który nie może
/// wywrócić się na jednym nieudanym zapytaniu, ale bezużyteczne dla przycisku „Testuj połączenie”:
/// zły token daje wtedy dokładnie ten sam wynik co konto bez zdefiniowanych statusów, czyli zielone
/// „pobrano 0 pozycji”. Ten typ rozróżnia te dwa przypadki i dlatego istnieje.</para></summary>
/// <param name="Success">Czy API odpowiedziało poprawnie (2xx). Zero pozycji przy 2xx to nadal sukces.</param>
/// <param name="HttpStatus">Kod odpowiedzi, o ile w ogóle doszło do odpowiedzi. Null = błąd sieci albo timeout.</param>
/// <param name="ItemCount">Ile pozycji zwrócił endpoint kontrolny.</param>
/// <param name="Error">Powód niepowodzenia po polsku, gotowy do pokazania w panelu. Null przy sukcesie.</param>
public sealed record SellasistConnectionTestResult(
    bool Success,
    int? HttpStatus,
    int ItemCount,
    string? Error);
