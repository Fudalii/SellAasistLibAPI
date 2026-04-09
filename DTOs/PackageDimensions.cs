namespace Sellasist.DTOs;

/// <summary>Wymiary i waga paczki — courier-agnostic DTO do przekazywania konfiguracji gabarytów</summary>
public record PackageDimensions(
    int Length,
    int Width,
    int Height,
    decimal DefaultWeight,
    bool UseOrderWeight);
