using PostService.CommonTypes;
namespace PostService.Models;
public class Posting : ModelBase, ICloneable
{
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public string SenderName { get; set; } = "";
    public string SenderPhone { get; set; } = "";
    public string SenderAddress { get; set; } = "";
    public string RecipientName { get; set; } = "";
    public string RecipientPhone { get; set; } = "";
    public string RecipientAddress { get; set; } = "";
    public string Content { get; set; } = "";
    public DeliveryType DeliveryType { get; set; }
    public float Weight { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float Depth { get; set; }
    public float? Value { get; set; }
    public string? Description { get; set; }
    public float Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public object Clone() => MemberwiseClone();
}
