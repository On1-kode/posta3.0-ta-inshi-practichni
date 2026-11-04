using System.Net;
using PostService.Business;
using PostService.Contracts;
using PostService.Data.Models;
using PostService.Data.Repositories;
using PostService.WindowsApp;
using BusinessValidation = PostService.Business.PostingValidationException;
using ClientValidation = PostService.WindowsApp.PostingValidationException;

void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

void Expect<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new Exception($"Очікувався виняток {typeof(T).Name}.");
}

var first = new GenericRepository<TestModel>();
var second = new GenericRepository<TestModel>();
var item = new TestModel();
first.Add(item);
Check(item.Id > 0 && second.GetById(item.Id) == item, "Екземпляри одного типу не поділяють дані.");
Check(second.Update(new TestModel { Id = item.Id, Name = "Змінено" }), "Не вдалося оновити запис.");
Check(first.GetById(item.Id)!.Name == "Змінено", "Зміни не видно в іншому екземплярі.");
second.GetAll().Clear();
Check(first.GetAll().Count == 1, "Зміна копії списку вплинула на репозиторій.");
Check(first.Delete(item.Id) && !first.Delete(item.Id), "Неправильний результат видалення.");
var next = new TestModel();
second.Add(next);
Check(next.Id > item.Id, "Видалений Id використано повторно.");
Check(!first.Update(new TestModel { Id = 999999 }), "Оновлення відсутнього запису мало бути невдалим.");
Expect<ArgumentException>(() => first.Add(new TestModel { Id = next.Id }));

var contacts = new GenericRepository<ContactPerson>();
contacts.Add(new ContactPerson { Id = next.Id, Name = "Олена" });
Check(first.GetAll().Count == 1 && contacts.GetAll().Count == 1, "Дані різних типів змішалися.");
var parallel = new GenericRepository<ParallelModel>();
Parallel.For(0, 100, _ => parallel.Add(new ParallelModel()));
Check(parallel.GetAll().Select(x => x.Id).Distinct().Count() == 100, "Паралельне додавання створило дублікати Id.");
Console.WriteLine("Репозиторій: CRUD, спільні дані, ізоляція типів і паралельне додавання — OK.");

IPostingService service = new PostingService(new GenericRepository<Posting>());
var valid = new PostingPostDto
{
    TrackNumber = " CHECK-001 ", Weight = 1.5, SenderAddress = " Київ ",
    RecipientAddress = " Львів ", Status = " Прийнято "
};
var created = service.Create(valid);
Check(created.TrackNumber == "CHECK-001" && created.SenderAddress == "Київ", "Сервіс не нормалізував дані.");
try
{
    service.Create(new PostingPostDto());
    throw new Exception("Сервіс прийняв порожні дані.");
}
catch (BusinessValidation e) { Check(e.Errors.Count == 5, "Потрібні помилки всіх п’яти полів."); }
Expect<BusinessValidation>(() => service.Update(created.Id, new PostingPutDto()));
Check(service.GetById(created.Id).Weight == 1.5, "Невдала валідація змінила збережений запис.");
var updated = service.Update(created.Id, new PostingPutDto
{
    TrackNumber = "CHECK-001", Weight = 2, SenderAddress = "Київ",
    RecipientAddress = "Львів", Status = "Доставлено"
});
Check(updated.Weight == 2 && service.GetAll().Single().Status == "Доставлено", "Сервіс не оновив запис.");
service.Delete(created.Id);
Expect<PostingNotFoundException>(() => service.GetById(created.Id));
Expect<PostingNotFoundException>(() => service.Update(created.Id, new PostingPutDto()));
Console.WriteLine("Бізнес-логіка: перевірка даних, нормалізація, CRUD і відсутні записи — OK.");

if (args.Contains("--server"))
{
    using var http = new HttpClient { BaseAddress = new Uri("http://localhost:5080") };
    var client = new PostServiceClient(http);
    try
    {
        await client.CreatePostingAsync(new PostingPostDto());
        throw new Exception("Очікувалася помилка HTTP 400.");
    }
    catch (ClientValidation e) { Check(e.Errors.Count == 5, "Клієнт загубив помилки валідації."); }
    var posting = await client.CreatePostingAsync(valid);
    try
    {
        Check((await client.GetPostingByIdAsync(posting.Id)).TrackNumber == "CHECK-001", "API не викликає бізнес-логіку.");
        Check((await client.GetPostingsAsync()).Any(p => p.Id == posting.Id), "Запис відсутній у списку.");
        await client.UpdatePostingAsync(posting.Id, new PostingPutDto
        {
            TrackNumber = "CHECK-001", Weight = 3, SenderAddress = "Київ",
            RecipientAddress = "Львів", Status = "У дорозі"
        });
        Check((await client.GetPostingByIdAsync(posting.Id)).Weight == 3, "HTTP PUT не оновив дані.");
    }
    finally { await client.DeletePostingAsync(posting.Id); }
    try
    {
        await client.GetPostingByIdAsync(posting.Id);
        throw new Exception("Очікувалася помилка HTTP 404.");
    }
    catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound) { }
    Check((await http.GetStringAsync("/")).Contains("posting-form"), "Сервер не віддає вебклієнт.");
    Check((await http.GetStringAsync("/app.js")).Contains("/postings"), "Вебклієнт не використовує API.");
    Console.WriteLine("HTTP-клієнт: CRUD, 400, 404 і файли вебклієнта — OK.");
}

public class TestModel : Model
{
    public string Name { get; set; } = "";
}

public class ParallelModel : Model { }
