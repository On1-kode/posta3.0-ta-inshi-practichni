namespace PostService.Business;

public class PostingNotFoundException : Exception
{
    public PostingNotFoundException(int id) : base($"Відправлення з Id {id} не знайдено.") { }
}
