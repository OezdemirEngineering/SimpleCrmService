using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Contracts;
using DbService;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("PeopleDb"));

builder.Services.AddScoped<IPersonRepository, PersonRepository>();

var app = builder.Build();

// Minimal API endpoints
app.MapGet("/people", async (IPersonRepository repo) =>
{
    var people = await repo.GetAllAsync();
    return Results.Ok(people);
});

app.MapGet("/people/{id}", async (int id, IPersonRepository repo) =>
{
    var person = await repo.GetByIdAsync(id);
    return person is not null ? Results.Ok(person) : Results.NotFound();
});

app.MapPost("/people", async (PersonCreateRequest request, IPersonRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        return Results.BadRequest("FirstName and LastName are required.");

    var person = new Person
    {
        FirstName = request.FirstName.Trim(),
        LastName = request.LastName.Trim(),
        Email = request.Email?.Trim()
    };

    await repo.AddAsync(person);
    return Results.Created($"/people/{person.Id}", person);
});

app.MapPut("/people/{id}", async (int id, PersonUpdateRequest request, IPersonRepository repo) =>
{
    var existing = await repo.GetByIdAsync(id);
    if (existing is null) return Results.NotFound();

    if (!string.IsNullOrWhiteSpace(request.FirstName)) existing.FirstName = request.FirstName.Trim();
    if (!string.IsNullOrWhiteSpace(request.LastName)) existing.LastName = request.LastName.Trim();
    if (request.Email is not null) existing.Email = request.Email.Trim();

    await repo.UpdateAsync(existing);
    return Results.NoContent();
});

app.MapDelete("/people/{id}", async (int id, IPersonRepository repo) =>
{
    var deleted = await repo.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();
