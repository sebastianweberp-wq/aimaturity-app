// UsersController.cs - REST controller for user login.
// Maps to POST /api/users/login.
using AiMaturityApp.Model.Entities;
using AiMaturityApp.Model.Repositories;
using AiMaturityApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiMaturityApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        protected UserRepository UserRepository { get; }
        protected AiMaturityAssessmentRepository AssessmentRepository { get; }
        protected ScoringService ScoringService { get; }

        // Constructor - ASP.NET Core injects all three dependencies via DI.
        public UsersController(
            UserRepository userRepository,
            AiMaturityAssessmentRepository assessmentRepository,
            ScoringService scoringService)
        {
            UserRepository       = userRepository;
            AssessmentRepository = assessmentRepository;
            ScoringService       = scoringService;
        }

        // Login - POST /api/users/login
        // Body: { "username": "Alice" }
        //
        // Returns a LoginResult:
        //   New user:       { "isNew": true,  "lastResult": null }
        //   Returning user: { "isNew": false, "lastResult": { ... } }
        //
        // The frontend uses isNew to decide whether to show the form
        // or the previous result panel directly.
        [HttpPost("login")]
        public ActionResult<LoginResult> Login([FromBody] AppUser user)
        {
            // Basic validation - reject empty or whitespace-only usernames.
            if (user == null || string.IsNullOrWhiteSpace(user.Username))
            {
                return BadRequest("Username is required.");
            }

            var username = user.Username.Trim();

            // Check whether this username already exists in the appuser table.
            var existingUser = UserRepository.GetByUsername(username);

            if (existingUser != null)
            {
                // Returning user - fetch their most recent assessment.
                var latest = AssessmentRepository.GetLatestByUsername(username);

                if (latest == null)
                {
                    // User exists but has no assessment yet - treat as new.
                    return Ok(new LoginResult { IsNew = true });
                }

                // Rebuild the full result from the stored maturity level so we
                // can return explanation and recommendations without recalculating.
                var result = new AssessmentResult
                {
                    Assessment      = latest,
                    Explanation     = ScoringService.GetExplanation(latest.MaturityLevel),
                    Recommendations = ScoringService.GetRecommendations(latest.MaturityLevel)
                };
                return Ok(new LoginResult { IsNew = false, LastResult = result });
            }

            // New user - insert them. UserRepository enforces the 10-user cap.
            UserRepository.InsertUser(username);
            return Ok(new LoginResult { IsNew = true });
        }
    }
}
