// ScoringService.cs - Business logic for scoring and interpreting assessments.
// We keep this in a dedicated service class so the scoring formula is
// centralised and the controller stays focused on routing.
using AiMaturityApp.Model.Entities;

namespace AiMaturityApp.Services
{
    public class ScoringService
    {
        // CalculateScore - Produces a weighted 0-100 score from the seven ratings.
        // Each rating is 1-5. The weights (4,4,3,3,2,2,2) sum to 20, and
        // 20 × 5 = 100, so the result is naturally in a 0-100 range.
        // Higher weights go to the dimensions that most determine AI readiness.
        public int CalculateScore(AiMaturityAssessment a)
        {
            int score =
                a.DigitalMaturity   * 4 +  // 20% - core digital infrastructure
                a.DataReadiness     * 4 +  // 20% - AI needs clean, accessible data
                a.CurrentAiUsage    * 3 +  // 15% - prior experience helps adoption
                a.ProcessAutomation * 3 +  // 15% - automated processes are ready for AI
                a.EmployeeSkills    * 2 +  // 10% - skills can be trained
                a.ManagementSupport * 2 +  // 10% - leadership drives adoption
                a.BudgetReadiness   * 2;   // 10% - budget enables but doesn't guarantee success
            return score;
        }

        // GetMaturityLevel - Maps a 0-100 score to one of three bands.
        // Cut-offs at 40 and 70 give three roughly equal zones.
        public string GetMaturityLevel(int score)
        {
            if (score < 40) return "Low AI Maturity";
            if (score < 70) return "Medium AI Maturity";
            return "High AI Maturity";
        }

        // GetExplanation - Returns a plain-English paragraph explaining
        // what the maturity level means for this particular SME.
        public string GetExplanation(string level)
        {
            if (level == "Low AI Maturity")
                return "Your business is in the early stages of AI readiness. Before AI can really help, you'll get more value from getting the basics in order: reliable customer records, decent accounting software, and a clear picture of your sales.";
            if (level == "Medium AI Maturity")
                return "You have a solid digital foundation. You're ready to try targeted AI experiments - small, low-risk pilots in one part of the business at a time.";
            if (level == "High AI Maturity")
                return "You're well positioned to use AI strategically. SMEs at this level are already seeing real value from AI tools and can scale what works across the rest of the business.";
            return "Score calculated.";
        }

        // GetRecommendations - Returns a list of concrete next steps
        // appropriate for the maturity level.
        public List<string> GetRecommendations(string level)
        {
            if (level == "Low AI Maturity")
                return new List<string>
                {
                    "Pick one customer/sales tool that fits your business (e.g. a simple CRM or cloud accounting).",
                    "Identify one repetitive task (writing invoices, replying to enquiries) and try an AI tool for it.",
                    "Give yourself or a key staff member two hours a week to learn one new digital tool properly."
                };
            if (level == "Medium AI Maturity")
                return new List<string>
                {
                    "Pick one specific use case (e.g. AI-written marketing copy or automated booking confirmations) and run it for three months.",
                    "Connect the tools you already use so customer data flows between them instead of being copy-pasted.",
                    "Decide what data you're willing to share with AI tools and what should stay private."
                };
            if (level == "High AI Maturity")
                return new List<string>
                {
                    "Roll out your most successful AI experiments to the rest of the business.",
                    "Use AI insights (sales patterns, customer behaviour) to guide bigger strategic decisions.",
                    "Review what's working every quarter - the tool landscape changes fast."
                };
            return new List<string> { "Review your assessment inputs and resubmit." };
        }
    }
}
