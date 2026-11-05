using PostService.Models;
namespace PostService.DataAccess;
public interface IGenericRepository<TModel> where TModel : ModelBase
{
    List<TModel> GetList();
    TModel? GetById(int id);
    int Create(TModel model);
    int Update(TModel model);
    int Delete(int id);
}
public interface IPostingRepository : IGenericRepository<Posting> { }
