using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using Pr1.MinWebService.Domain;
using Pr1.MinWebService.Errors;
using Pr1.MinWebService.Middlewares;
using Pr1.MinWebService.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Настройка сериализации
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.WriteIndented = true;
});

// Добавляем Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Comic Books API", 
        Version = "v1",
        Description = "API для коллекции комиксов"
    });
});

builder.Services.AddSingleton<IItemRepository, InMemoryItemRepository>();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Comic Books API v1"));

// Конвейер обработки запросов
app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<TimingAndLogMiddleware>();

// GET /api/items (или /api/comics - выберите одно название)
app.MapGet("/api/items", (IItemRepository repo) =>
{
    return Results.Ok(repo.GetAll());
})
.WithName("GetAllItems"); 

// GET /api/items/{id}
app.MapGet("/api/items/{id:guid}", (Guid id, IItemRepository repo) =>
{
    var item = repo.GetById(id);
    if (item is null)
        throw new NotFoundException("Комикс не найден");

    return Results.Ok(item);
})
.WithName("GetItemById"); 


app.MapPost("/api/items", (HttpContext ctx, CreateItemRequest request, IItemRepository repo) =>
{
    // Валидация
    if (string.IsNullOrWhiteSpace(request.Name))
        throw new ValidationException("Название комикса не может быть пустым");

    if (request.Price < 0)
        throw new ValidationException("Цена не может быть отрицательной");
    
    if (string.IsNullOrWhiteSpace(request.Author))
        throw new ValidationException("Автор не может быть пустым");
    
    if (request.IssueNumber <= 0)
        throw new ValidationException("Номер выпуска должен быть положительным");

    var created = repo.Create(
        request.Name.Trim(), 
        request.Price,
        request.Author.Trim(),
        request.IssueNumber
    );

    var location = $"/api/items/{created.Id}";
    ctx.Response.Headers.Location = location;

    return Results.Created(location, created);
})
.WithName("CreateItem"); 

// Опционально: поиск
app.MapGet("/api/items/search", (string? author, IItemRepository repo) =>
{
    var items = repo.GetAll();
    if (!string.IsNullOrWhiteSpace(author))
        items = items.Where(c => c.Author.Contains(author, StringComparison.OrdinalIgnoreCase)).ToList();
    return Results.Ok(items);
})
.WithName("SearchItems"); 

app.Run();

public partial class Program { }