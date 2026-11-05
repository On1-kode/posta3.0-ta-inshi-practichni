namespace PostService.Data.Models;

public class Posting : Model
{
    public string TrackNumber { get; set; } = "";
    public double Weight { get; set; }
    public string SenderAddress { get; set; } = "";
    public string RecipientAddress { get; set; } = "";
    public string Status { get; set; } = "";

    public override string ToString() =>
        $"{Id}: {TrackNumber}, {Weight} кг, {SenderAddress} → {RecipientAddress}, {Status}";
}
