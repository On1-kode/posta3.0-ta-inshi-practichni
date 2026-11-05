using Microsoft.EntityFrameworkCore;
using PostService.Models;
namespace PostService.DataAccess;
public class GenericEfRepository<TModel>(DbContext context) : IGenericRepository<TModel>
    where TModel : ModelBase
{
    protected readonly DbContext _context = context;
    public List<TModel> GetList() => _context.Set<TModel>().AsNoTracking().OrderBy(x => x.Id).ToList();
    public TModel? GetById(int id) => _context.Set<TModel>().AsNoTracking().FirstOrDefault(x => x.Id == id);
    public int Create(TModel model)
    {
        _context.Set<TModel>().Add(model);
        _context.SaveChanges();
        return model.Id;
    }
    public int Update(TModel model)
    {
        var existing = _context.Set<TModel>().Find(model.Id);
        if (existing is null) return 0;
        _context.Entry(existing).CurrentValues.SetValues(model);
        _context.SaveChanges();
        return 1;
    }
    public int Delete(int id)
    {
        var model = _context.Set<TModel>().Find(id);
        if (model is null) return 0;
        _context.Set<TModel>().Remove(model);
        _context.SaveChanges();
        return 1;
    }
}
