using System.Text;
using PostService.Data.Models;
using PostService.Data.Repositories;

Console.OutputEncoding = Encoding.UTF8;

var postings = new GenericRepository<Posting>();
postings.Add(new Posting { Id = 1, TrackNumber = "UA001", Weight = 1.5, SenderAddress = "Київ", RecipientAddress = "Львів", Status = "Прийнято" });
postings.Add(new Posting { Id = 2, TrackNumber = "UA002", Weight = 2, SenderAddress = "Одеса", RecipientAddress = "Київ", Status = "У дорозі" });
postings.Add(new Posting { Id = 3, TrackNumber = "UA003", Weight = 0.7, SenderAddress = "Львів", RecipientAddress = "Одеса", Status = "Прийнято" });
Console.WriteLine("Відправлення з Id = 1: " + postings.GetById(1));
postings.Update(new Posting { Id = 1, TrackNumber = "UA001", Weight = 1.5, SenderAddress = "Київ", RecipientAddress = "Львів", Status = "Доставлено" });
postings.Delete(2);
Console.WriteLine("Відправлення після оновлення Id = 1 і видалення Id = 2:");
foreach (var posting in postings.GetAll()) Console.WriteLine(posting);

var contacts = new GenericRepository<ContactPerson>();
contacts.Add(new ContactPerson { Id = 1, Name = "Іван Петренко", Phone = "+380501111111", Email = "ivan@example.com" });
contacts.Add(new ContactPerson { Id = 2, Name = "Олена Коваль", Phone = "+380502222222", Email = "olena@example.com" });
contacts.Add(new ContactPerson { Id = 3, Name = "Андрій Бондар", Phone = "+380503333333", Email = "andrii@example.com" });
Console.WriteLine("\nКонтакт з Id = 1: " + contacts.GetById(1));
contacts.Update(new ContactPerson { Id = 1, Name = "Іван Петренко", Phone = "+380509999999", Email = "ivan@example.com" });
contacts.Delete(2);
Console.WriteLine("Контакти після оновлення Id = 1 і видалення Id = 2:");
foreach (var contact in contacts.GetAll()) Console.WriteLine(contact);
Console.WriteLine("\nВідправлення після роботи з контактами:");
foreach (var posting in postings.GetAll()) Console.WriteLine(posting);
