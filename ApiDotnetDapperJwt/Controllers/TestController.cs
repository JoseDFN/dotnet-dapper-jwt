using Microsoft.AspNetCore.Mvc;
using Domain.Exceptions;

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

    // Nuevos endpoints para probar excepciones personalizadas
    [HttpGet("custom-not-found")]
    public IActionResult ThrowCustomNotFoundException()
    {
        throw new NotFoundException("User", 999);
    }

    [HttpGet("custom-validation")]
    public IActionResult ThrowCustomValidationException()
    {
        throw new ValidationException("Email", "Email format is invalid");
    }

    [HttpGet("custom-business")]
    public IActionResult ThrowCustomBusinessException()
    {
        throw new BusinessException("INSUFFICIENT_STOCK", "Not enough stock available for this order");
    }

    [HttpGet("custom-unauthorized")]
    public IActionResult ThrowCustomUnauthorizedException()
    {
        throw new UnauthorizedException("Invalid credentials provided");
    }
}
