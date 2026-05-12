// AiMaturityAssessment.cs - Entity class that maps to one row in the
// aimaturityassessment table. Plain data object with no business logic.
namespace AiMaturityApp.Model.Entities;

// AiMaturityAssessment - Represents one completed assessment submitted
// through POST /api/assessments. Each property maps directly to a
// column in the aimaturityassessment table defined in CreateTable.sql.
public class AiMaturityAssessment
{
    // Empty constructor - required so ASP.NET Core can deserialise
    // the JSON request body into this object via [FromBody].
    public AiMaturityAssessment() { }

    // Constructor with id - used in AiMaturityAssessmentRepository.MapRow
    // when reading a row back from the NpgsqlDataReader.
    public AiMaturityAssessment(int id) { Id = id; }

    // Id - Primary key. Set by the PostgreSQL sequence, never by the application.
    public int Id { get; set; }

    // Username - Links this assessment to the logged-in user in the appuser table.
    // Sent by the frontend after a successful POST /api/users/login.
    public string Username { get; set; } = "";

    // CompanyName, Industry, CompanySize - Basic business info from the form.
    public string CompanyName { get; set; } = "";
    public string Industry    { get; set; } = "";
    public string CompanySize { get; set; } = "";

    // The seven 1-5 rating dimensions.
    // Each maps to a column with a CHECK (value BETWEEN 1 AND 5) constraint
    // in CreateTable.sql - the database rejects anything out of range.
    public int DigitalMaturity   { get; set; }
    public int DataReadiness     { get; set; }
    public int CurrentAiUsage    { get; set; }
    public int ProcessAutomation { get; set; }
    public int EmployeeSkills    { get; set; }
    public int ManagementSupport { get; set; }
    public int BudgetReadiness   { get; set; }

    // TotalScore - Weighted 0-100 score computed by ScoringService.CalculateScore.
    // Always set on the backend before the row is inserted - the client never sends this.
    public int TotalScore { get; set; }

    // MaturityLevel - "Low / Medium / High AI Maturity" from ScoringService.GetMaturityLevel.
    // Stored so returning users can see their previous result without recalculating.
    public string MaturityLevel { get; set; } = "";
}
