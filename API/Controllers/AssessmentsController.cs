// AssessmentsController.cs - REST controller for the assessment endpoints.
// Maps to the URL prefix /api/assessments.
// The frontend calls these endpoints with fetch(); the controller delegates
// to the repository (data access) and ScoringService (business logic).
using AiMaturityApp.Model.Entities;
using AiMaturityApp.Model.Repositories;
using AiMaturityApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiMaturityApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssessmentsController : ControllerBase
    {
        protected AiMaturityAssessmentRepository Repository { get; }
        protected ScoringService ScoringService { get; }

        public AssessmentsController(
            AiMaturityAssessmentRepository repository,
            ScoringService scoringService)
        {
            Repository     = repository;
            ScoringService = scoringService;
        }

        // GET /api/assessments/{id}
        // Returns one assessment by primary key, or 404 if not found.
        [HttpGet("{id}")]
        public ActionResult<AiMaturityAssessment> GetAssessment([FromRoute] int id)
        {
            AiMaturityAssessment assessment = Repository.GetAssessmentById(id);
            if (assessment == null)
            {
                return NotFound($"Assessment with id {id} not found.");
            }
            return Ok(assessment);
        }

        // GET /api/assessments
        // Returns all stored assessments, newest first.
        [HttpGet]
        public ActionResult<IEnumerable<AiMaturityAssessment>> GetAssessments()
        {
            return Ok(Repository.GetAssessments());
        }

        // POST /api/assessments
        // Receives the seven 1-5 ratings from the frontend, calculates the
        // score and maturity level on the server, stores the row, and returns
        // a full AssessmentResult.
        // We always compute TotalScore and MaturityLevel here - never from client input.
        [HttpPost]
        public ActionResult<AssessmentResult> Post([FromBody] AiMaturityAssessment assessment)
        {
            if (assessment == null)
            {
                return BadRequest("Assessment body is missing or malformed.");
            }

            assessment.TotalScore    = ScoringService.CalculateScore(assessment);
            assessment.MaturityLevel = ScoringService.GetMaturityLevel(assessment.TotalScore);

            bool status = Repository.InsertAssessment(assessment);
            if (status)
            {
                var result = new AssessmentResult
                {
                    Assessment      = assessment,
                    Explanation     = ScoringService.GetExplanation(assessment.MaturityLevel),
                    Recommendations = ScoringService.GetRecommendations(assessment.MaturityLevel)
                };
                return Ok(result);
            }
            return BadRequest("Failed to save assessment to the database.");
        }

        // PUT /api/assessments
        // Updates an existing row and re-scores it.
        [HttpPut]
        public ActionResult UpdateAssessment([FromBody] AiMaturityAssessment assessment)
        {
            if (assessment == null)
            {
                return BadRequest("Assessment body is missing or malformed.");
            }

            AiMaturityAssessment existing = Repository.GetAssessmentById(assessment.Id);
            if (existing == null)
            {
                return NotFound($"Assessment with id {assessment.Id} not found.");
            }

            assessment.TotalScore    = ScoringService.CalculateScore(assessment);
            assessment.MaturityLevel = ScoringService.GetMaturityLevel(assessment.TotalScore);

            bool status = Repository.UpdateAssessment(assessment);
            if (status) return Ok();
            return BadRequest("Failed to update the assessment.");
        }

        // DELETE /api/assessments/{id}
        // Removes a row. Returns 204 No Content on success.
        [HttpDelete("{id}")]
        public ActionResult DeleteAssessment([FromRoute] int id)
        {
            AiMaturityAssessment existing = Repository.GetAssessmentById(id);
            if (existing == null)
            {
                return NotFound($"Assessment with id {id} not found.");
            }

            bool status = Repository.DeleteAssessment(id);
            if (status) return NoContent();
            return BadRequest($"Failed to delete assessment with id {id}.");
        }
    }
}
