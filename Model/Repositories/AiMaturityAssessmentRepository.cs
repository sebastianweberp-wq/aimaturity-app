// AiMaturityAssessmentRepository.cs - All SQL for the aimaturityassessment table.
// Inherits from BaseRepository to reuse the connection string and helper methods.
// The controller calls these methods and never writes SQL directly.
using AiMaturityApp.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace AiMaturityApp.Model.Repositories;

public class AiMaturityAssessmentRepository : BaseRepository
{
    public AiMaturityAssessmentRepository(IConfiguration configuration)
        : base(configuration) { }

    // GetAssessmentById - SELECT one row by primary key.
    // Returns null if no match so the controller can return a 404.
    public AiMaturityAssessment GetAssessmentById(int id)
    {
        NpgsqlConnection dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from aimaturityassessment where id = @id";
            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

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

    // GetLatestByUsername - SELECT the most recent assessment for a given user.
    // Used when a returning user logs in, so we can show their previous result
    // without asking them to fill in the form again.
    public AiMaturityAssessment GetLatestByUsername(string username)
    {
        NpgsqlConnection dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            // ORDER BY id DESC LIMIT 1 gives us the most recently inserted row.
            cmd.CommandText = @"
select * from aimaturityassessment
where username = @username
order by id desc
limit 1";
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

    // GetAssessments - SELECT every row, newest first.
    public List<AiMaturityAssessment> GetAssessments()
    {
        NpgsqlConnection dbConn = null;
        var assessments = new List<AiMaturityAssessment>();
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from aimaturityassessment order by id desc";

            var data = GetData(dbConn, cmd);
            if (data != null)
            {
                while (data.Read())
                {
                    assessments.Add(MapRow(data));
                }
            }
            return assessments;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // InsertAssessment - INSERT a new row. The id is omitted because the
    // database sequence assigns it. All values use parameterised queries
    // to prevent SQL injection.
    public bool InsertAssessment(AiMaturityAssessment a)
    {
        NpgsqlConnection dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
insert into aimaturityassessment
(username, companyname, industry, companysize,
 digitalmaturity, datareadiness, currentaiusage,
 processautomation, employeeskills, managementsupport,
 budgetreadiness, totalscore, maturitylevel)
values
(@username, @companyname, @industry, @companysize,
 @digitalmaturity, @datareadiness, @currentaiusage,
 @processautomation, @employeeskills, @managementsupport,
 @budgetreadiness, @totalscore, @maturitylevel)";

            cmd.Parameters.AddWithValue("@username",          NpgsqlDbType.Text,    a.Username);
            cmd.Parameters.AddWithValue("@companyname",       NpgsqlDbType.Text,    a.CompanyName);
            cmd.Parameters.AddWithValue("@industry",          NpgsqlDbType.Text,    a.Industry);
            cmd.Parameters.AddWithValue("@companysize",       NpgsqlDbType.Text,    a.CompanySize);
            cmd.Parameters.AddWithValue("@digitalmaturity",   NpgsqlDbType.Integer, a.DigitalMaturity);
            cmd.Parameters.AddWithValue("@datareadiness",     NpgsqlDbType.Integer, a.DataReadiness);
            cmd.Parameters.AddWithValue("@currentaiusage",    NpgsqlDbType.Integer, a.CurrentAiUsage);
            cmd.Parameters.AddWithValue("@processautomation", NpgsqlDbType.Integer, a.ProcessAutomation);
            cmd.Parameters.AddWithValue("@employeeskills",    NpgsqlDbType.Integer, a.EmployeeSkills);
            cmd.Parameters.AddWithValue("@managementsupport", NpgsqlDbType.Integer, a.ManagementSupport);
            cmd.Parameters.AddWithValue("@budgetreadiness",   NpgsqlDbType.Integer, a.BudgetReadiness);
            cmd.Parameters.AddWithValue("@totalscore",        NpgsqlDbType.Integer, a.TotalScore);
            cmd.Parameters.AddWithValue("@maturitylevel",     NpgsqlDbType.Text,    a.MaturityLevel);

            return InsertData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // UpdateAssessment - UPDATE an existing row identified by its primary key.
    public bool UpdateAssessment(AiMaturityAssessment a)
    {
        NpgsqlConnection dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
update aimaturityassessment set
username=@username, companyname=@companyname, industry=@industry,
companysize=@companysize, digitalmaturity=@digitalmaturity,
datareadiness=@datareadiness, currentaiusage=@currentaiusage,
processautomation=@processautomation, employeeskills=@employeeskills,
managementsupport=@managementsupport, budgetreadiness=@budgetreadiness,
totalscore=@totalscore, maturitylevel=@maturitylevel
where id = @id";

            cmd.Parameters.AddWithValue("@username",          NpgsqlDbType.Text,    a.Username);
            cmd.Parameters.AddWithValue("@companyname",       NpgsqlDbType.Text,    a.CompanyName);
            cmd.Parameters.AddWithValue("@industry",          NpgsqlDbType.Text,    a.Industry);
            cmd.Parameters.AddWithValue("@companysize",       NpgsqlDbType.Text,    a.CompanySize);
            cmd.Parameters.AddWithValue("@digitalmaturity",   NpgsqlDbType.Integer, a.DigitalMaturity);
            cmd.Parameters.AddWithValue("@datareadiness",     NpgsqlDbType.Integer, a.DataReadiness);
            cmd.Parameters.AddWithValue("@currentaiusage",    NpgsqlDbType.Integer, a.CurrentAiUsage);
            cmd.Parameters.AddWithValue("@processautomation", NpgsqlDbType.Integer, a.ProcessAutomation);
            cmd.Parameters.AddWithValue("@employeeskills",    NpgsqlDbType.Integer, a.EmployeeSkills);
            cmd.Parameters.AddWithValue("@managementsupport", NpgsqlDbType.Integer, a.ManagementSupport);
            cmd.Parameters.AddWithValue("@budgetreadiness",   NpgsqlDbType.Integer, a.BudgetReadiness);
            cmd.Parameters.AddWithValue("@totalscore",        NpgsqlDbType.Integer, a.TotalScore);
            cmd.Parameters.AddWithValue("@maturitylevel",     NpgsqlDbType.Text,    a.MaturityLevel);
            cmd.Parameters.AddWithValue("@id",                NpgsqlDbType.Integer, a.Id);

            return UpdateData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // DeleteAssessment - DELETE a row by primary key. Returns true on success.
    public bool DeleteAssessment(int id)
    {
        NpgsqlConnection dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "delete from aimaturityassessment where id = @id";
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            return DeleteData(dbConn, cmd);
        }
        finally
        {
            dbConn?.Close();
        }
    }

    // MapRow - Converts the current NpgsqlDataReader position into an entity object.
    // Private because only this repository knows the column names of its table.
    private AiMaturityAssessment MapRow(NpgsqlDataReader data)
    {
        return new AiMaturityAssessment(Convert.ToInt32(data["id"]))
        {
            Username          = data["username"].ToString(),
            CompanyName       = data["companyname"].ToString(),
            Industry          = data["industry"].ToString(),
            CompanySize       = data["companysize"].ToString(),
            DigitalMaturity   = Convert.ToInt32(data["digitalmaturity"]),
            DataReadiness     = Convert.ToInt32(data["datareadiness"]),
            CurrentAiUsage    = Convert.ToInt32(data["currentaiusage"]),
            ProcessAutomation = Convert.ToInt32(data["processautomation"]),
            EmployeeSkills    = Convert.ToInt32(data["employeeskills"]),
            ManagementSupport = Convert.ToInt32(data["managementsupport"]),
            BudgetReadiness   = Convert.ToInt32(data["budgetreadiness"]),
            TotalScore        = Convert.ToInt32(data["totalscore"]),
            MaturityLevel     = data["maturitylevel"].ToString()
        };
    }
}
