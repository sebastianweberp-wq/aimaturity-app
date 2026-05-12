// BaseRepository.cs - Shared base class for all repositories.
// We use this to avoid repeating connection-string setup and
// basic SQL helper methods in every repository class.
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AiMaturityApp.Model.Repositories;

// BaseRepository - Inherited by AiMaturityAssessmentRepository and UserRepository.
// Holds the connection string (read from appsettings.json via IConfiguration)
// and four helper methods that open the connection and run a command.
public class BaseRepository
{
    // ConnectionString - The Npgsql connection string from appsettings.json,
    // under "ConnectionStrings": { "AppProgDb": "..." }.
    // Protected so subclasses can pass it to new NpgsqlConnection().
    protected string ConnectionString { get; }

    // Constructor - IConfiguration is injected by ASP.NET Core's DI container.
    // We throw immediately if the key is missing so the error is obvious at startup.
    public BaseRepository(IConfiguration configuration)
    {
        ConnectionString = configuration.GetConnectionString("AppProgDb")
            ?? throw new Exception("Connection string 'AppProgDb' not found in appsettings.json.");
    }

    // GetData - Opens the connection and runs a SELECT command.
    // Returns an NpgsqlDataReader the caller loops over with data.Read().
    // The caller must close the connection in a finally block.
    protected NpgsqlDataReader GetData(NpgsqlConnection conn, NpgsqlCommand cmd)
    {
        conn.Open();
        return cmd.ExecuteReader();
    }

    // InsertData - Opens the connection and runs an INSERT.
    // We use ExecuteNonQuery for statements that don't return rows.
    protected bool InsertData(NpgsqlConnection conn, NpgsqlCommand cmd)
    {
        conn.Open();
        cmd.ExecuteNonQuery();
        return true;
    }

    // UpdateData - Same pattern as InsertData but for UPDATE statements.
    protected bool UpdateData(NpgsqlConnection conn, NpgsqlCommand cmd)
    {
        conn.Open();
        cmd.ExecuteNonQuery();
        return true;
    }

    // DeleteData - Same pattern as InsertData but for DELETE statements.
    protected bool DeleteData(NpgsqlConnection conn, NpgsqlCommand cmd)
    {
        conn.Open();
        cmd.ExecuteNonQuery();
        return true;
    }
}
