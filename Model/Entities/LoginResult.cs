// LoginResult.cs - Response shape for POST /api/users/login.
// This is not a database entity - it is built in UsersController
// and sent back as JSON so the frontend gets everything it needs
// in one HTTP round-trip.
namespace AiMaturityApp.Model.Entities;

// LoginResult - Tells the frontend two things after a login attempt:
//   IsNew      - true  → show the assessment form (new user, no data yet)
//   IsNew      - false → show the previous result panel (returning user)
//   LastResult - the previous AssessmentResult, or null for new users.
public class LoginResult
{
    // IsNew - We use this flag to decide which screen the frontend shows next.
    public bool IsNew { get; set; }

    // LastResult - The most recent assessment for this user, fetched via
    // AiMaturityAssessmentRepository.GetLatestByUsername.
    // Uses the same AssessmentResult shape as POST /api/assessments so
    // the frontend's renderResult() function works for both cases.
    public AssessmentResult? LastResult { get; set; }
}
