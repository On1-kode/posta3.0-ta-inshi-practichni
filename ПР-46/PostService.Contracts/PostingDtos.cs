namespace PostService.Contracts;

public class PostingPostDto
{
    public string TrackNumber { get; set; } = "";
    public double Weight { get; set; }
    public string SenderAddress { get; set; } = "";
    public string RecipientAddress { get; set; } = "";
    public string Status { get; set; } = "";
}

public class PostingPutDto : PostingPostDto;

public class PostingGetDto : PostingPostDto
{
    public int Id { get; set; }
}
