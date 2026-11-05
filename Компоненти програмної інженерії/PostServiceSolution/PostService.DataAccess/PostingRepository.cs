using PostService.Models;
namespace PostService.DataAccess;
public class PostingRepository(PostingDbContext context) : GenericEfRepository<Posting>(context), IPostingRepository { }
