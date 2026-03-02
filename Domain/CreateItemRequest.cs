namespace Pr1.MinWebService.Domain;

public sealed record CreateItemRequest(
    string Name,           // Название комикса
    decimal Price,         // Цена
    string Author,         // Добавляем автора
    int IssueNumber        // Добавляем номер выпуска
);