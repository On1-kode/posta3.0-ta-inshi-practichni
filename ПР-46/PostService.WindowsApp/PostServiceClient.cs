using System.Net;
using System.Net.Http.Json;
using PostService.Contracts;

namespace PostService.WindowsApp;

public interface IPostServiceClient
{
    Task<PostingGetDto[]> GetPostingsAsync();
    Task<PostingGetDto> GetPostingByIdAsync(int id);
    Task<PostingGetDto> CreatePostingAsync(PostingPostDto posting);
    Task<PostingGetDto> UpdatePostingAsync(int id, PostingPutDto posting);
    Task DeletePostingAsync(int id);
}

public class PostingValidationException(Dictionary<string, string[]> errors)
    : Exception("Дані не пройшли перевірку.")
{
    public Dictionary<string, string[]> Errors { get; } = errors;
}

public class ValidationErrorResponse
{
    public Dictionary<string, string[]> Errors { get; set; } = new();
}

public class PostServiceClient(HttpClient httpClient) : IPostServiceClient
{
    public async Task<PostingGetDto[]> GetPostingsAsync()
    {
        using var response = await httpClient.GetAsync("/postings");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<PostingGetDto[]>()
            ?? throw new InvalidDataException("Сервер не повернув список відправлень.");
    }

    public async Task<PostingGetDto> GetPostingByIdAsync(int id)
    {
        using var response = await httpClient.GetAsync($"/postings/{id}");
        await EnsureSuccessAsync(response);
        return await ReadPostingAsync(response);
    }

    public async Task<PostingGetDto> CreatePostingAsync(PostingPostDto posting)
    {
        using var response = await httpClient.PostAsJsonAsync("/postings", posting);
        await EnsureSuccessAsync(response);
        return await ReadPostingAsync(response);
    }

    public async Task<PostingGetDto> UpdatePostingAsync(int id, PostingPutDto posting)
    {
        using var response = await httpClient.PutAsJsonAsync($"/postings/{id}", posting);
        await EnsureSuccessAsync(response);
        return await ReadPostingAsync(response);
    }

    public async Task DeletePostingAsync(int id)
    {
        using var response = await httpClient.DeleteAsync($"/postings/{id}");
        await EnsureSuccessAsync(response);
    }

    private static async Task<PostingGetDto> ReadPostingAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<PostingGetDto>()
        ?? throw new InvalidDataException("Сервер не повернув відправлення.");

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>()
                ?? throw new InvalidDataException("Сервер не повернув помилки валідації.");
            throw new PostingValidationException(problem.Errors);
        }
        throw new HttpRequestException(
            $"Сервер повернув код {(int)response.StatusCode}.", null, response.StatusCode);
    }
}
