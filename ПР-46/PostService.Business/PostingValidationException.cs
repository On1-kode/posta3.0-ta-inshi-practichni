namespace PostService.Business;

public class PostingValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public PostingValidationException(Dictionary<string, string[]> errors)
        : base("Дані відправлення не пройшли перевірку.")
    {
        Errors = errors;
    }
}
