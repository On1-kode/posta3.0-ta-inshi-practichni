using PostService.Models;
using PostService.CommonTypes;
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var postings = new List<Posting>
{
    new() { Id = 1, From = "Дмитро, Київ", To = "Олена, Львів", Content = "Книги", DeliveryType = DeliveryType.Department, Weight = 2, Width = 30, Height = 20, Depth = 10, Value = 500, Price = 60, CreatedAt = DateTime.UtcNow },
    new() { Id = 2, From = "Іван, Дніпро", To = "Марія, Одеса", Content = "Документи", DeliveryType = DeliveryType.Courier, Weight = 1, Width = 20, Height = 15, Depth = 5, Value = 100, Price = 85, CreatedAt = DateTime.UtcNow }
};
app.MapGet("/postings", () => Results.Ok(postings));
app.MapGet("/postings/{id:int}", (int id) => postings.Find(p => p.Id == id) is { } p ? Results.Ok(p) : Results.NotFound());
app.MapPost("/postings", (Posting p) =>
{
    p.Id = postings.Select(x => x.Id).DefaultIfEmpty(0).Max() + 1;
    p.CreatedAt = DateTime.UtcNow; postings.Add(p); return Results.Created($"/postings/{p.Id}", p);
});
app.MapPut("/postings/{id:int}", (int id, Posting updated) =>
{
    int index = postings.FindIndex(p => p.Id == id); if (index < 0) return Results.NotFound();
    updated.Id = id; updated.CreatedAt = postings[index].CreatedAt; postings[index] = updated; return Results.Ok(updated);
});
app.MapDelete("/postings/{id:int}", (int id) => postings.RemoveAll(x => x.Id == id) > 0 ? Results.NoContent() : Results.NotFound());
app.Run();
