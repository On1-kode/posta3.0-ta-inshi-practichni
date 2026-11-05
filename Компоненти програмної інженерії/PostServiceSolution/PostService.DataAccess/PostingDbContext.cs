using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using PostService.Models;
namespace PostService.DataAccess;
public class PostingDbContext : DbContext
{
    public PostingDbContext(DbContextOptions<PostingDbContext> options) : base(options) { }
    public DbSet<Posting> Postings => Set<Posting>();
    public void InitializeDatabase()
    {
        Database.EnsureCreated();
        var connection = (SqliteConnection)Database.GetDbConnection();
        connection.Open();
        try
        {
            var columns = new HashSet<string>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA table_info(Postings)";
                using var reader = command.ExecuteReader();
                while (reader.Read()) columns.Add(reader.GetString(1));
            }
            string[] contactFields = ["SenderName", "SenderPhone", "SenderAddress", "RecipientName", "RecipientPhone", "RecipientAddress"];
            if (contactFields.All(columns.Contains)) return;
            // Оновлюємо стару базу без видалення відправлень.
            using var transaction = connection.BeginTransaction();
            foreach (string field in contactFields.Where(f => !columns.Contains(f)))
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = $"ALTER TABLE Postings ADD COLUMN {field} TEXT NOT NULL DEFAULT ''";
                command.ExecuteNonQuery();
            }
            transaction.Commit();
        }
        finally { connection.Close(); }
    }
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
