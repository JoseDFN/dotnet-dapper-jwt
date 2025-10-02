using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        _logger.LogInformation("Login attempt for username: {Username}", request?.Username ?? "null");

        // Validaciones de entrada
        if (request == null)
        {
            _logger.LogWarning("Login attempt with null request");
            throw new ValidationException("Request", "Login request cannot be null");
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            _logger.LogWarning("Login attempt with empty username");
            throw new ValidationException("Username", "Username is required");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Login attempt with empty password for user: {Username}", request.Username);
            throw new ValidationException("Password", "Password is required");
        }

        if (request.Username.Length < 2)
        {
            _logger.LogWarning("Login attempt with short username: {Username}", request.Username);
            throw new ValidationException("Username", "Username must be at least 2 characters long");
        }

        if (request.Password.Length < 4)
        {
            _logger.LogWarning("Login attempt with short password for user: {Username}", request.Username);
            throw new ValidationException("Password", "Password must be at least 4 characters long");
        }

        var response = await _authService.LoginAsync(request);
        if (response == null)
        {
            _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
            throw new UnauthorizedException("Invalid username or password");
        }

        _logger.LogInformation("Successful login for user: {Username} with role: {Role}", request.Username, response.Role);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        _logger.LogInformation("Refresh token attempt");

        // Validaciones de entrada
        if (request == null)
        {
            _logger.LogWarning("Refresh token attempt with null request");
            throw new ValidationException("Request", "Refresh token request cannot be null");
        }

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            _logger.LogWarning("Refresh token attempt with empty token");
            throw new ValidationException("RefreshToken", "Refresh token is required");
        }

        var response = await _authService.RefreshTokenAsync(request);
        if (response == null)
        {
            _logger.LogWarning("Failed refresh token attempt");
            throw new UnauthorizedException("Invalid or expired refresh token");
        }

        _logger.LogInformation("Successful token refresh for user: {Username}", response.Username);
        return Ok(response);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDto request)
    {
        _logger.LogInformation("Token revoke attempt");

        // Validaciones de entrada
        if (request == null)
        {
            _logger.LogWarning("Token revoke attempt with null request");
            throw new ValidationException("Request", "Revoke token request cannot be null");
        }

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            _logger.LogWarning("Token revoke attempt with empty token");
            throw new ValidationException("RefreshToken", "Refresh token is required");
        }

        var result = await _authService.RevokeTokenAsync(request.RefreshToken);
        if (!result)
        {
            _logger.LogWarning("Failed token revoke attempt");
            throw new UnauthorizedException("Invalid refresh token");
        }

        _logger.LogInformation("Token revoked successfully");
        return Ok(new { message = "Token revoked successfully" });
    }
}