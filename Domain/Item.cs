namespace Pr1.MinWebService.Domain;

public sealed record Item(
    Guid Id, 
    string Name,           // Название комикса
    decimal Price,
    string Author,         // Автор
    int IssueNumber        // Номер выпуска
);