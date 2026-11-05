using Microsoft.Data.Sqlite;
using PostService.CommonTypes;
using PostService.Models;
namespace PostService.DataAccess;
public class PostingRepository : IPostingRepository
{
    private static string ConnectionString => "Data Source=" + (Environment.GetEnvironmentVariable("POSTSERVICE_DB") ?? Path.Combine(AppContext.BaseDirectory, "postings-ado.db"));
    public void CreateDb()
    {
        using var conn = new SqliteConnection(ConnectionString); conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS Postings (Id INTEGER PRIMARY KEY AUTOINCREMENT, [From] TEXT NOT NULL, [To] TEXT NOT NULL, Content TEXT NOT NULL, DeliveryType INTEGER NOT NULL, Weight REAL NOT NULL, Width REAL NOT NULL, Height REAL NOT NULL, Depth REAL NOT NULL, Value REAL NULL, Description TEXT NULL, Price REAL NOT NULL, CreatedAt INTEGER NOT NULL)";
        cmd.ExecuteNonQuery();
    }
    private static Posting Read(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0), From = r.GetString(1), To = r.GetString(2), Content = r.GetString(3),
        DeliveryType = (DeliveryType)r.GetInt32(4), Weight = r.GetFloat(5), Width = r.GetFloat(6), Height = r.GetFloat(7), Depth = r.GetFloat(8),
        Value = r.IsDBNull(9) ? null : r.GetFloat(9), Description = r.IsDBNull(10) ? null : r.GetString(10), Price = r.GetFloat(11),
        CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(r.GetInt64(12)).UtcDateTime
    };
    public List<Posting> GetList()
    {
        using var conn = new SqliteConnection(ConnectionString); conn.Open(); using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Postings ORDER BY Id"; using var reader = cmd.ExecuteReader();
        var list = new List<Posting>(); while(reader.Read()) list.Add(Read(reader)); return list;
    }
    public Posting? GetById(int id)
    {
        using var conn = new SqliteConnection(ConnectionString); conn.Open(); using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Postings WHERE Id=@Id"; cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader(); return reader.Read() ? Read(reader) : null;
    }
    private static void Parameters(SqliteCommand cmd, Posting p)
    {
        cmd.Parameters.AddWithValue("@Id",p.Id); cmd.Parameters.AddWithValue("@From",p.From); cmd.Parameters.AddWithValue("@To",p.To);
        cmd.Parameters.AddWithValue("@Content",p.Content); cmd.Parameters.AddWithValue("@DeliveryType",(int)p.DeliveryType);
        cmd.Parameters.AddWithValue("@Weight",p.Weight); cmd.Parameters.AddWithValue("@Width",p.Width); cmd.Parameters.AddWithValue("@Height",p.Height); cmd.Parameters.AddWithValue("@Depth",p.Depth);
        cmd.Parameters.AddWithValue("@Value",(object?)p.Value ?? DBNull.Value); cmd.Parameters.AddWithValue("@Description",(object?)p.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Price",p.Price); cmd.Parameters.AddWithValue("@CreatedAt",new DateTimeOffset(p.CreatedAt).ToUnixTimeMilliseconds());
    }
    public int Create(Posting p)
    {
        using var conn = new SqliteConnection(ConnectionString); conn.Open(); using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Postings ([From],[To],Content,DeliveryType,Weight,Width,Height,Depth,Value,Description,Price,CreatedAt) VALUES (@From,@To,@Content,@DeliveryType,@Weight,@Width,@Height,@Depth,@Value,@Description,@Price,@CreatedAt) RETURNING Id";
        Parameters(cmd,p); return Convert.ToInt32(cmd.ExecuteScalar());
    }
    public int Update(Posting p)
    {
        using var conn = new SqliteConnection(ConnectionString); conn.Open(); using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Postings SET [From]=@From,[To]=@To,Content=@Content,DeliveryType=@DeliveryType,Weight=@Weight,Width=@Width,Height=@Height,Depth=@Depth,Value=@Value,Description=@Description,Price=@Price,CreatedAt=@CreatedAt WHERE Id=@Id";
        Parameters(cmd,p); return cmd.ExecuteNonQuery();
    }
    public int Delete(int id)
    {
        using var conn = new SqliteConnection(ConnectionString); conn.Open(); using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Postings WHERE Id=@Id"; cmd.Parameters.AddWithValue("@Id",id); return cmd.ExecuteNonQuery();
    }
}
