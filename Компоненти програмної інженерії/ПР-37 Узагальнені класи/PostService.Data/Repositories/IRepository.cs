using PostService.Data.Models;

namespace PostService.Data.Repositories;

public interface IRepository<T> where T : Model
{
    void Add(T entity);
    T? GetById(int id);
    List<T> GetAll();
    bool Update(T entity);
    bool Delete(int id);
}
