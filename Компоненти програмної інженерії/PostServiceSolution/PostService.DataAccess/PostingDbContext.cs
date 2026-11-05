using Microsoft.EntityFrameworkCore;
using PostService.Models;
namespace PostService.DataAccess;
public class PostingDbContext : DbContext
{
    public PostingDbContext(DbContextOptions<PostingDbContext> options) : base(options) { }
    public DbSet<Posting> Postings => Set<Posting>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Posting>().Property(p => p.CreatedAt)
            .HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            string path = Environment.GetEnvironmentVariable("POSTSERVICE_DB")
                ?? Path.Combine(AppContext.BaseDirectory, "postings-ef.db");
            optionsBuilder.UseSqlite($"Data Source={path}");
        }
    }
}
