using Microsoft.Extensions.FileProviders;
using PostService.Business;
using PostService.Contracts;
using PostService.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5080");
builder.Services.AddSingleton(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPostingService, PostingService>();
var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (PostingValidationException exception)
    {
        await Results.ValidationProblem(exception.Errors).ExecuteAsync(context);
    }
    catch (PostingNotFoundException)
    {
        await Results.NotFound().ExecuteAsync(context);
    }
    catch (Exception exception)
    {
        app.Logger.LogError(exception, "Необроблена помилка сервера.");
        await Results.Problem("Сталася помилка сервера.", statusCode: 500).ExecuteAsync(context);
    }
});

var webFiles = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "wwwroot"));
app.Lifetime.ApplicationStopped.Register(webFiles.Dispose);
app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = webFiles });
app.UseStaticFiles(new StaticFileOptions { FileProvider = webFiles });

app.MapGet("/postings", (IPostingService service) => Results.Ok(service.GetAll()));
app.MapGet("/postings/{id:int}", (int id, IPostingService service) => Results.Ok(service.GetById(id)));
app.MapPost("/postings", (PostingPostDto dto, IPostingService service) =>
{
    var posting = service.Create(dto);
    return Results.Created($"/postings/{posting.Id}", posting);
});
app.MapPut("/postings/{id:int}", (int id, PostingPutDto dto, IPostingService service) =>
    Results.Ok(service.Update(id, dto)));
app.MapDelete("/postings/{id:int}", (int id, IPostingService service) =>
{
    service.Delete(id);
    return Results.NoContent();
});

app.Run();
