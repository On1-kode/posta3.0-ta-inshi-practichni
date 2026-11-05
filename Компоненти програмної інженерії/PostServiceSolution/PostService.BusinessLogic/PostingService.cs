using PostService.CommonTypes;
using PostService.DataAccess;
using PostService.Models;
namespace PostService.BusinessLogic;
public class PostingValidationException(Dictionary<string, string[]> errors) : Exception("Перевірте введені дані.")
{
    public Dictionary<string, string[]> Errors { get; } = errors;
}
public class PostingService(IPostingRepository repository) : IPostingService
{
    public Posting Create(Posting posting)
    {
        Prepare(posting);
        posting.Id = 0;
        posting.CreatedAt = DateTime.UtcNow;
        posting.Id = repository.Create(posting);
        return posting;
    }
    public List<Posting> GetAll() => repository.GetList();
    public Posting? Find(int id) => repository.GetById(id);
    public Posting? Update(int id, Posting posting)
    {
        var old = repository.GetById(id);
        if (old is null) return null;
        Prepare(posting);
        posting.Id = id;
        posting.CreatedAt = old.CreatedAt;
        return repository.Update(posting) == 0 ? null : posting;
    }
    public int Delete(int id) => repository.Delete(id);
    private static void Prepare(Posting p)
    {
        var errors = new Dictionary<string, string[]>();
        void Required(string? value, string field, string title)
        {
            if (string.IsNullOrWhiteSpace(value)) errors[field] = [$"Введіть {title}."];
        }
        void Positive(float value, string field, string title)
        {
            if (!float.IsFinite(value) || value <= 0) errors[field] = [$"{title} має бути більше нуля."];
        }
        Required(p.From, "From", "місто відправлення"); Required(p.To, "To", "місто отримання");
        Required(p.SenderName, "SenderName", "піб відправника");
        Required(p.SenderPhone, "SenderPhone", "телефон відправника");
        Required(p.SenderAddress, "SenderAddress", "адреса або відділення відправлення");
        Required(p.RecipientName, "RecipientName", "піб отримувача");
        Required(p.RecipientPhone, "RecipientPhone", "телефон отримувача");
        Required(p.RecipientAddress, "RecipientAddress", "адреса або відділення отримання");
        foreach (var (phone, field) in new[] { (p.SenderPhone, "SenderPhone"), (p.RecipientPhone, "RecipientPhone") })
        {
            string digits = new((phone ?? "").Where(char.IsAsciiDigit).ToArray());
            bool symbolsValid = (phone ?? "").All(c => char.IsAsciiDigit(c) || "+ ()-".Contains(c));
            if (!symbolsValid || !(digits.Length == 10 && digits.StartsWith("0") || digits.Length == 12 && digits.StartsWith("380")))
                errors[field] = ["Введіть український номер: 0XX XXX XX XX або +380 XX XXX XX XX."];
        }
        Required(p.Content, "Content", "вміст відправлення");
        Positive(p.Weight, "Weight", "Вага"); Positive(p.Width, "Width", "Ширина");
        Positive(p.Height, "Height", "Висота"); Positive(p.Depth, "Depth", "Глибина");
        if (!Enum.IsDefined(p.DeliveryType)) errors["DeliveryType"] = ["Оберіть спосіб доставки."];
        if (p.Value is float value && (!float.IsFinite(value) || value < 0)) errors["Value"] = ["Цінність не може бути відʼємною."];
        if (errors.Count > 0) throw new PostingValidationException(errors);
        p.From = p.From.Trim(); p.To = p.To.Trim(); p.Content = p.Content.Trim();
        p.SenderName = p.SenderName.Trim();
        p.SenderPhone = p.SenderPhone.Trim();
        p.SenderAddress = p.SenderAddress.Trim();
        p.RecipientName = p.RecipientName.Trim();
        p.RecipientPhone = p.RecipientPhone.Trim();
        p.RecipientAddress = p.RecipientAddress.Trim();
        p.SenderPhone = "+380" + new string(p.SenderPhone.Where(char.IsAsciiDigit).ToArray())[^9..];
        p.RecipientPhone = "+380" + new string(p.RecipientPhone.Where(char.IsAsciiDigit).ToArray())[^9..];
        // Навчальні тарифи: відділення 40 + 10/кг, курʼєр 70 + 15/кг, експрес 100 + 20/кг.
        p.Price = p.DeliveryType switch
        {
            DeliveryType.Department => 40 + p.Weight * 10,
            DeliveryType.Courier => 70 + p.Weight * 15,
            _ => 100 + p.Weight * 20
        };
        if (!float.IsFinite(p.Price)) throw new PostingValidationException(new() { ["Weight"] = ["Вага завелика для розрахунку вартості."] });
    }
}
