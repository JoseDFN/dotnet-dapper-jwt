using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Domain.Exceptions;

namespace ApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
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

    // Endpoints para probar UnitOfWork
    [HttpGet("unitofwork-test")]
    public IActionResult TestUnitOfWork()
    {
        return Ok(new { message = "UnitOfWork test endpoint - check logs for transaction details" });
    }

    // Endpoint para verificar autorización Admin
    [HttpGet("admin-only")]
    public IActionResult AdminOnly()
    {
        var user = User.Identity?.Name ?? "Unknown";
        var roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        
        return Ok(new { 
            message = "This endpoint is only accessible by Admin users",
            currentUser = user,
            userRoles = roles,
            timestamp = DateTime.UtcNow
        });
    }

    // Endpoint público para probar autorización (debe devolver 401 sin token)
    [HttpGet("public-test")]
    [AllowAnonymous]
    public IActionResult PublicTest()
    {
        return Ok(new { 
            message = "This is a public endpoint - no authentication required",
            timestamp = DateTime.UtcNow
        });
    }

    // Endpoint público para probar excepciones
    [HttpGet("public-exception")]
    [AllowAnonymous]
    public IActionResult PublicException()
    {
        throw new ValidationException("TestField", "This is a test validation exception");
    }

    // Endpoint para probar validaciones de AuthController
    [HttpPost("test-auth-validation")]
    [AllowAnonymous]
    public IActionResult TestAuthValidation([FromBody] object request)
    {
        return Ok(new { 
            message = "Auth validation test endpoint - check logs for validation details",
            request = request,
            timestamp = DateTime.UtcNow
        });
    }

    // Endpoint para probar validaciones de OrdersController
    [HttpPost("test-orders-validation")]
    [AllowAnonymous]
    public IActionResult TestOrdersValidation([FromBody] object request)
    {
        return Ok(new { 
            message = "Orders validation test endpoint - check logs for validation details",
            request = request,
            timestamp = DateTime.UtcNow
        });
    }
}
