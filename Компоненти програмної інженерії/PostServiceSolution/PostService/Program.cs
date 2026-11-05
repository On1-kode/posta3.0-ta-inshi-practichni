using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PostService.BusinessLogic;
using PostService.DataAccess;
using PostService.Dtos;
using PostService.Mappings;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PostingDbContext>();
builder.Services.AddScoped<IPostingRepository, PostingRepository>();
builder.Services.AddScoped<IPostingService, PostingService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.UseAllOfToExtendReferenceSchemas();
    options.UseAllOfToExtendReferenceSchemas();
    options.OperationFilter<PostService.Swagger.IdParameterFilter>();
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "PostService", Version = "v1", Description = "Облік поштових відправлень" });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PostService.xml"));
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PostService.Dtos.xml"));
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<PostingDbContext>().Database.EnsureCreated();
app.Use(async (context, next) =>
{
    try { await next(context); }
    catch (PostingValidationException e) { await Results.ValidationProblem(e.Errors).ExecuteAsync(context); }
    catch (Exception e)
    {
        app.Logger.LogError(e, "Необроблена помилка сервера");
        await Results.Problem("Сталася помилка сервера.", statusCode: 500).ExecuteAsync(context);
    }
});
app.UseSwagger(); app.UseSwaggerUI();
app.UseDefaultFiles(); app.UseStaticFiles();
app.MapGet("/api", () => "Це API поштового клієнта.")
    .WithSummary("Інформація про API").WithDescription("Повертає текстове повідомлення поштового API.");
app.MapGet("/postings", (IPostingService s) => Results.Ok(s.GetAll().Select(PostingMapper.ToDto)))
    .WithSummary("Список відправлень").WithDescription("Повертає всі збережені у SQLite відправлення.").Produces<PostingGetDto[]>();
app.MapGet("/postings/{id:int}", (int id, IPostingService s) =>
    s.Find(id) is { } p ? Results.Ok(PostingMapper.ToDto(p)) : Results.NotFound())
    .WithSummary("Відправлення за ідентифікатором").WithDescription("id — ідентифікатор відправлення. Відсутній запис повертає 404.")
    .Produces<PostingGetDto>().Produces(404);
app.MapPost("/postings", (PostingPostDto dto, IPostingService s) =>
{
    var p = s.Create(PostingMapper.ToModel(dto));
    return Results.Created($"/postings/{p.Id}", PostingMapper.ToDto(p));
}).WithSummary("Нове відправлення").WithDescription("Приймає дані відправлення. Сервер створює Id і CreatedAt та розраховує Price.")
    .Produces<PostingGetDto>(201).ProducesValidationProblem();
app.MapPut("/postings/{id:int}", (int id, PostingPutDto dto, IPostingService s) =>
    s.Update(id, PostingMapper.ToModel(dto)) is { } p ? Results.Ok(PostingMapper.ToDto(p)) : Results.NotFound())
    .WithSummary("Змінити відправлення").WithDescription("id — ідентифікатор запису. Замінює поля та перераховує вартість, зберігаючи дату створення.")
    .Produces<PostingGetDto>().Produces(404).ProducesValidationProblem();
app.MapDelete("/postings/{id:int}", (int id, IPostingService s) =>
    s.Delete(id) > 0 ? Results.NoContent() : Results.NotFound())
    .WithSummary("Видалити відправлення").WithDescription("id — ідентифікатор. Повертає 204 після видалення або 404, якщо запису немає.")
    .Produces(204).Produces(404);
app.Run();
