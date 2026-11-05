using PostService.Models;
namespace PostService.DataAccess;
public interface IPostingRepository
{
    List<Posting> GetList(); Posting? GetById(int id); int Create(Posting model); int Update(Posting model); int Delete(int id);
}
