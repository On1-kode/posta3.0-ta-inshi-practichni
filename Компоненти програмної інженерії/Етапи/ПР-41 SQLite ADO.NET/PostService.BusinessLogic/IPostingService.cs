using PostService.Models;
namespace PostService.BusinessLogic;
public interface IPostingService
{
    Posting Create(Posting posting);
    List<Posting> GetAll();
    Posting? Find(int id);
    Posting? Update(int id, Posting posting);
    int Delete(int id);
}
