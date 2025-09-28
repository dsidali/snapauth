using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmailGeneratorController : ControllerBase
{
    private readonly EmailGeneratorService _emailGeneratorService;

    public EmailGeneratorController(EmailGeneratorService emailGeneratorService)
    {
        _emailGeneratorService = emailGeneratorService;
    }

    [HttpPost("generate")]
    public IActionResult GenerateEmails([FromBody] EmailGenerationRequest? request)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Request body is required." });
        }

        if (request.Count <= 0)
        {
            return BadRequest(new { error = "Count must be a positive integer." });
        }

        try
        {
            var emails = _emailGeneratorService.GenerateRandomEmails(request.Count);
            return Ok(new { count = emails.Count, emails });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }
}
