// AppUser.cs - Entity class that maps to one row in the appuser table.
// We keep this as a plain data object (POCO) with no logic - that belongs
// in the repository and controller layers.
namespace AiMaturityApp.Model.Entities;

// AppUser - Represents a logged-in user stored in the appuser table.
// We only store a username (no password) because this is a prototype
// focused on demonstrating data storage and retrieval, not authentication.
// The table is capped at 10 rows, enforced in UserRepository.
public class AppUser
{
    // Empty constructor - needed so ASP.NET Core can deserialise
    // the JSON body of POST /api/users/login into this object.
    public AppUser() { }

    // Constructor with id - used in UserRepository.MapRow when
    // reading a row back from PostgreSQL.
    public AppUser(int id) { Id = id; }

    // Id - Primary key, assigned by the database sequence in CreateTable.sql.
    // We never set this from C# - the database fills it in automatically.
    public int Id { get; set; }

    // Username - The name the user types on the login screen.
    // Unique constraint in CreateTable.sql prevents duplicate entries.
    public string Username { get; set; } = "";
}
