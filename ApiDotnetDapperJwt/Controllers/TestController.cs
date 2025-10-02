using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("exception")]
    public IActionResult ThrowException()
    {
        throw new Exception("This is a test exception to verify global exception handling");
    }

    [HttpGet("argument-exception")]
    public IActionResult ThrowArgumentException()
    {
        throw new ArgumentException("Invalid argument provided");
    }

    [HttpGet("not-found")]
    public IActionResult ThrowNotFoundException()
    {
        throw new KeyNotFoundException("Resource not found");
    }

    [HttpGet("unauthorized")]
    public IActionResult ThrowUnauthorizedException()
    {
        throw new UnauthorizedAccessException("Access denied");
    }
}
