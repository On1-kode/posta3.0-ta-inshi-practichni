using PostService.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5080");
var app = builder.Build();
var postings = new List<PostingGetDto>();
object sync = new();
int nextId = 1;

app.MapGet("/postings", () =>
{
    lock (sync) return Results.Ok(postings.ToArray());
});

app.MapGet("/postings/{id:int}", (int id) =>
{
    lock (sync)
    {
        var posting = postings.Find(p => p.Id == id);
        return posting is null ? Results.NotFound() : Results.Ok(posting);
    }
});

app.MapPost("/postings", (PostingPostDto dto) =>
{
    var errors = Validate(dto);
    if (errors.Count > 0) return Results.ValidationProblem(errors);
    lock (sync)
    {
        var posting = ToPosting(nextId++, dto);
        postings.Add(posting);
        return Results.Created($"/postings/{posting.Id}", posting);
    }
});

app.MapPut("/postings/{id:int}", (int id, PostingPutDto dto) =>
{
    lock (sync)
    {
        int index = postings.FindIndex(p => p.Id == id);
        if (index < 0) return Results.NotFound();
        var errors = Validate(dto);
        if (errors.Count > 0) return Results.ValidationProblem(errors);
        var posting = ToPosting(id, dto);
        postings[index] = posting;
        return Results.Ok(posting);
    }
});

app.MapDelete("/postings/{id:int}", (int id) =>
{
    lock (sync)
    {
        int removed = postings.RemoveAll(p => p.Id == id);
        return removed == 0 ? Results.NotFound() : Results.NoContent();
    }
});

app.Run();

static Dictionary<string, string[]> Validate(PostingPostDto dto)
{
    var errors = new Dictionary<string, string[]>();
    if (string.IsNullOrWhiteSpace(dto.TrackNumber)) errors["TrackNumber"] = ["Вкажіть трек-номер."];
    if (!double.IsFinite(dto.Weight) || dto.Weight <= 0) errors["Weight"] = ["Вага має бути більшою за нуль."];
    if (string.IsNullOrWhiteSpace(dto.SenderAddress)) errors["SenderAddress"] = ["Вкажіть адресу відправника."];
    if (string.IsNullOrWhiteSpace(dto.RecipientAddress)) errors["RecipientAddress"] = ["Вкажіть адресу отримувача."];
    if (string.IsNullOrWhiteSpace(dto.Status)) errors["Status"] = ["Вкажіть статус доставки."];
    return errors;
}

static PostingGetDto ToPosting(int id, PostingPostDto dto) => new()
{
    Id = id,
    TrackNumber = dto.TrackNumber,
    Weight = dto.Weight,
    SenderAddress = dto.SenderAddress,
    RecipientAddress = dto.RecipientAddress,
    Status = dto.Status
};
