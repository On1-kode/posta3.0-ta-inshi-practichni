using PostService.Data.Models;

namespace PostService.Data.Repositories;

public class GenericRepository<T> : IRepository<T> where T : Model
{
    private static readonly List<T> items = new();
    private static readonly object sync = new();
    private static int nextId = 1;

    public void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        lock (sync)
        {
            if (entity.Id < 0) throw new ArgumentOutOfRangeException(nameof(entity.Id));
            if (entity.Id == 0) entity.Id = nextId++;
            else
            {
                if (items.Any(item => item.Id == entity.Id))
                    throw new ArgumentException("Сутність із таким Id уже існує.");
                nextId = Math.Max(nextId, entity.Id + 1);
            }
            items.Add(entity);
        }
    }

    public T? GetById(int id)
    {
        lock (sync) return items.Find(item => item.Id == id);
    }

    public List<T> GetAll()
    {
        lock (sync) return new List<T>(items);
    }

    public bool Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        lock (sync)
        {
            int index = items.FindIndex(item => item.Id == entity.Id);
            if (index < 0) return false;
            items[index] = entity;
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (sync) return items.RemoveAll(item => item.Id == id) > 0;
    }
}
