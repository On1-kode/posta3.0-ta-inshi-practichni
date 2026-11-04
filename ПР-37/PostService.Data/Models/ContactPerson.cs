namespace PostService.Data.Models;

public class ContactPerson : Model
{
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";

    public override string ToString() => $"{Id}: {Name}, {Phone}, {Email}";
}
