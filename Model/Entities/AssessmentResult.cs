// AssessmentResult.cs - Response shape for POST /api/assessments.
// Also embedded inside LoginResult.LastResult for returning users.
// Not a database row - built in the controller and serialised to JSON.
namespace AiMaturityApp.Model.Entities;

// AssessmentResult - Bundles everything the frontend needs to render
// the result panel in a single response, so the client only needs one call:
//   Assessment      - the stored row (score, level, all seven ratings)
//   Explanation     - one paragraph from ScoringService.GetExplanation
//   Recommendations - bullet list from ScoringService.GetRecommendations
public class AssessmentResult
{
    // Assessment - The full AiMaturityAssessment, including TotalScore and
    // MaturityLevel computed by ScoringService on the backend.
    // We never accept a score from the client - it is always calculated here.
    public AiMaturityAssessment Assessment { get; set; } = new();

    // Explanation - Plain-English paragraph explaining what the maturity
    // level means for this particular SME.
    public string Explanation { get; set; } = "";

    // Recommendations - Concrete next steps rendered as a bullet list
    // in the frontend (#r-recs element in index.html).
    public List<string> Recommendations { get; set; } = new();
}
