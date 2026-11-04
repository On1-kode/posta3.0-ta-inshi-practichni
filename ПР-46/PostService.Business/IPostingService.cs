using PostService.Contracts;

namespace PostService.Business;

public interface IPostingService
{
    PostingGetDto[] GetAll();
    PostingGetDto GetById(int id);
    PostingGetDto Create(PostingPostDto dto);
    PostingGetDto Update(int id, PostingPutDto dto);
    void Delete(int id);
}
