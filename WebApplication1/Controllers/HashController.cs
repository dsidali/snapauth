using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HashController : ControllerBase
{
    [HttpPost("sha256")]
    public IActionResult ComputeSha256([FromBody] HashRequest? request)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Request body is required." });
        }

        if (request.Input is null)
        {
            return BadRequest(new { error = "Input cannot be null." });
        }

        var normalizedFormat = string.IsNullOrWhiteSpace(request.OutputFormat)
            ? "hex"
            : request.OutputFormat.Trim().ToLowerInvariant();

        return normalizedFormat switch
        {
            "hex" => Ok(new
            {
                format = normalizedFormat,
                hash = Sha256.HashToHex(request.Input).ToLower()
            }),
            "base64" => Ok(new
            {
                format = normalizedFormat,
                hash = Sha256.HashToBase64(request.Input).ToLower()
            }),
            _ => BadRequest(new { error = "Invalid output format. Use 'hex' or 'base64'." })
        };
    }

    [HttpPost("sha256/list")]
    public IActionResult ComputeSha256List([FromBody] HashListRequest? request)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Request body is required." });
        }

        if (request.Inputs is null || request.Inputs.Count == 0)
        {
            return BadRequest(new { error = "Inputs list cannot be null or empty." });
        }

        // var normalizedFormat = string.IsNullOrWhiteSpace(request.OutputFormat)
        //     ? "hex"
        //     : request.OutputFormat.Trim().ToLowerInvariant();
        var normalizedFormat = "hex";

        if (normalizedFormat != "hex")
        {
            return BadRequest(new { error = "Only 'hex' format is supported for list hashing." });
        }

        try
        {
            var hashedValues = Sha256.HashListToHex(request.Inputs);
            // Convert all hashes to lowercase
            var lowerCaseHashes = hashedValues.ToList();

            return Ok(new
            {
                format = normalizedFormat,
                count = lowerCaseHashes.Count,
                hashes = lowerCaseHashes
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }
}
