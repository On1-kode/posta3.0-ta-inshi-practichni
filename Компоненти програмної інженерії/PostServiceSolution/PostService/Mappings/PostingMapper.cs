using PostService.Dtos;
using PostService.Models;
namespace PostService.Mappings;
/// <summary>Перетворення між контрактами API та моделлю.</summary>
public static class PostingMapper
{
    /// <summary>Перетворює вхідний контракт на модель без службових полів.</summary>
    public static Posting ToModel(PostingPostDto dto) => new()
    {
        From = dto.From,
        To = dto.To,
        SenderName = dto.SenderName,
        SenderPhone = dto.SenderPhone,
        SenderAddress = dto.SenderAddress,
        RecipientName = dto.RecipientName,
        RecipientPhone = dto.RecipientPhone,
        RecipientAddress = dto.RecipientAddress,
        Content = dto.Content,
        DeliveryType = dto.DeliveryType,
        Weight = dto.Weight,
        Width = dto.Width,
        Height = dto.Height,
        Depth = dto.Depth,
        Value = dto.Value,
        Description = dto.Description,
    };
    /// <summary>Перетворює модель на контракт відповіді.</summary>
    public static PostingGetDto ToDto(Posting p) => new()
    {
        From = p.From,
        To = p.To,
        SenderName = p.SenderName,
        SenderPhone = p.SenderPhone,
        SenderAddress = p.SenderAddress,
        RecipientName = p.RecipientName,
        RecipientPhone = p.RecipientPhone,
        RecipientAddress = p.RecipientAddress,
        Content = p.Content,
        DeliveryType = p.DeliveryType,
        Weight = p.Weight,
        Width = p.Width,
        Height = p.Height,
        Depth = p.Depth,
        Value = p.Value,
        Description = p.Description,
        Id = p.Id, Price = p.Price, CreatedAt = p.CreatedAt
    };
}
