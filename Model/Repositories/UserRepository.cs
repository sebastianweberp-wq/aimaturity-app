// UserRepository.cs - All SQL for the appuser table lives here.
// Follows the same repository pattern as AiMaturityAssessmentRepository:
// inherit BaseRepository, build one NpgsqlCommand per method, call the base helpers.
using AiMaturityApp.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace AiMaturityApp.Model.Repositories;

// UserRepository - Encapsulates every database operation on the appuser table.
// The controller calls these methods and never writes SQL directly.
public class UserRepository : BaseRepository
{
    // MAX_USERS - We cap the table at 10 rows to demonstrate storage limits.
    // When the table is full, InsertUser removes the oldest row first
    // (lowest id = earliest created) before inserting the new one.
    private const int MAX_USERS = 10;

    public UserRepository(IConfiguration configuration) : base(configuration) { }

    // GetByUsername - Returns the AppUser whose username matches, or null.
    // Called by UsersController.Login to decide if this is a new or returning user.
    // All queries use parameterised values to prevent SQL injection.
    public AppUser? GetByUsername(string username)
    {
        NpgsqlConnection dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from appuser where username = @username";
            cmd.Parameters.Add("@username", NpgsqlDbType.Text).Value = username;

            var data = GetData(dbConn, cmd);
            if (data != null && data.Read())
            {
                return MapRow(data);
            }
            return null;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // InsertUser - Adds a new row to the appuser table.
    // Checks the MAX_USERS cap first; if we are at 10 rows, the oldest is deleted.
    public bool InsertUser(string username)
    {
        if (CountUsers() >= MAX_USERS)
        {
            DeleteOldest();
        }

        NpgsqlConnection dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            // We don't pass an id - the sequence in CreateTable.sql assigns it.
            cmd.CommandText = "insert into appuser (username) values (@username)";
            cmd.Parameters.AddWithValue("@username", NpgsqlDbType.Text, username);
            return InsertData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // CountUsers - Returns how many rows are currently in the appuser table.
    // Uses ExecuteScalar because we only need one integer value back.
    private int CountUsers()
    {
        NpgsqlConnection dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select count(*) from appuser";
            dbConn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // DeleteOldest - Removes the row with the lowest id (the first user ever added).
    // The subquery finds the target id in the same statement to avoid a second round-trip.
    private void DeleteOldest()
    {
        NpgsqlConnection dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText =
                "delete from appuser where id = (select min(id) from appuser)";
            DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // MapRow - Converts the current reader position into an AppUser object.
    // Private because only this repository knows the column names of appuser.
    private AppUser MapRow(NpgsqlDataReader data)
    {
        return new AppUser(Convert.ToInt32(data["id"]))
        {
            Username = data["username"].ToString()!
        };
    }
}
