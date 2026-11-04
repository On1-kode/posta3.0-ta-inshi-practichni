using PostService.Contracts;
using PostService.Data.Models;
using PostService.Data.Repositories;

namespace PostService.Business;

public class PostingService : IPostingService
{
    private readonly IRepository<Posting> repository;

    public PostingService(IRepository<Posting> repository)
    {
        this.repository = repository;
    }

    public PostingGetDto[] GetAll() => repository.GetAll().Select(ToDto).ToArray();

    public PostingGetDto GetById(int id)
    {
        var posting = repository.GetById(id) ?? throw new PostingNotFoundException(id);
        return ToDto(posting);
    }

    public PostingGetDto Create(PostingPostDto dto)
    {
        Validate(dto);
        var posting = ToModel(0, dto);
        repository.Add(posting);
        return ToDto(posting);
    }

    public PostingGetDto Update(int id, PostingPutDto dto)
    {
        if (repository.GetById(id) is null) throw new PostingNotFoundException(id);
        Validate(dto);
        var posting = ToModel(id, dto);
        if (!repository.Update(posting)) throw new PostingNotFoundException(id);
        return ToDto(posting);
    }

    public void Delete(int id)
    {
        if (!repository.Delete(id)) throw new PostingNotFoundException(id);
    }

    private static void Validate(PostingPostDto dto)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(dto.TrackNumber)) errors["TrackNumber"] = ["Вкажіть трек-номер."];
        if (!double.IsFinite(dto.Weight) || dto.Weight <= 0) errors["Weight"] = ["Вага має бути більшою за нуль."];
        if (string.IsNullOrWhiteSpace(dto.SenderAddress)) errors["SenderAddress"] = ["Вкажіть адресу відправника."];
        if (string.IsNullOrWhiteSpace(dto.RecipientAddress)) errors["RecipientAddress"] = ["Вкажіть адресу отримувача."];
        if (string.IsNullOrWhiteSpace(dto.Status)) errors["Status"] = ["Вкажіть статус доставки."];
        if (errors.Count > 0) throw new PostingValidationException(errors);
    }

    private static Posting ToModel(int id, PostingPostDto dto) => new()
    {
        Id = id, TrackNumber = dto.TrackNumber.Trim(), Weight = dto.Weight,
        SenderAddress = dto.SenderAddress.Trim(), RecipientAddress = dto.RecipientAddress.Trim(),
        Status = dto.Status.Trim()
    };

    private static PostingGetDto ToDto(Posting posting) => new()
    {
        Id = posting.Id, TrackNumber = posting.TrackNumber, Weight = posting.Weight,
        SenderAddress = posting.SenderAddress, RecipientAddress = posting.RecipientAddress,
        Status = posting.Status
    };
}
