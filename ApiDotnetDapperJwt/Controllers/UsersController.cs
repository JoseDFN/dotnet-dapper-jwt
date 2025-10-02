using Application.DTOs.Users;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;

namespace ApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // POST /users → crear un usuario (rol por defecto "user")
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        // Validación básica
        if (string.IsNullOrWhiteSpace(dto.Username))
            throw new ValidationException("Username", "Username is required");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ValidationException("Password", "Password is required");

        if (dto.Password.Length < 6)
            throw new ValidationException("Password", "Password must be at least 6 characters long");

        var role = await _unitOfWork.Roles.GetByNameAsync("user");
        if (role == null)
            throw new BusinessException("DEFAULT_ROLE_NOT_FOUND", "Default user role not found in system");

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = dto.RoleId ?? role.Id,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        var userId = await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveAsync();

        return Ok(new { userId });
    }

    // GET /users/{id} → obtener información del usuario
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(int id)
    {
        if (id <= 0)
            throw new ValidationException("Id", "User ID must be greater than 0");

        var user = await _unitOfWork.Users.GetUserWithRoleAsync(id);
        if (user == null)
            throw new NotFoundException("User", id);

        var response = new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username!,
            Role = user.Role?.Name ?? "user"
        };

        return Ok(response);
    }
}