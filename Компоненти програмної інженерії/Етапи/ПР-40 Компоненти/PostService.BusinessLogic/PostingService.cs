using PostService.CommonTypes;
using PostService.Models;
namespace PostService.BusinessLogic;
public class PostingValidationException(Dictionary<string, string[]> errors) : Exception("Перевірте введені дані.")
{
    public Dictionary<string, string[]> Errors { get; } = errors;
}
public class PostingService : IPostingService
{
    private static readonly MemoryRepository repository = new();
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
        Required(p.From, "From", "відправника"); Required(p.To, "To", "отримувача");
        Required(p.Content, "Content", "вміст відправлення");
        Positive(p.Weight, "Weight", "Вага"); Positive(p.Width, "Width", "Ширина");
        Positive(p.Height, "Height", "Висота"); Positive(p.Depth, "Depth", "Глибина");
        if (!Enum.IsDefined(p.DeliveryType)) errors["DeliveryType"] = ["Оберіть спосіб доставки."];
        if (p.Value is float value && (!float.IsFinite(value) || value < 0)) errors["Value"] = ["Цінність не може бути відʼємною."];
        if (errors.Count > 0) throw new PostingValidationException(errors);
        p.From = p.From.Trim(); p.To = p.To.Trim(); p.Content = p.Content.Trim();
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

internal class MemoryRepository
{
    private static readonly List<Posting> postings = new();
    private static readonly object sync = new();
    private static int nextId = 1;
    public List<Posting> GetList() { lock(sync) return postings.Select(x => (Posting)x.Clone()).ToList(); }
    public Posting? GetById(int id) { lock(sync) return postings.Find(x => x.Id == id) is { } p ? (Posting)p.Clone() : null; }
    public int Create(Posting p) { lock(sync) { p.Id = nextId++; postings.Add((Posting)p.Clone()); return p.Id; } }
    public int Update(Posting p) { lock(sync) { int i = postings.FindIndex(x => x.Id == p.Id); if(i < 0) return 0; postings[i] = (Posting)p.Clone(); return 1; } }
    public int Delete(int id) { lock(sync) return postings.RemoveAll(x => x.Id == id); }
}
