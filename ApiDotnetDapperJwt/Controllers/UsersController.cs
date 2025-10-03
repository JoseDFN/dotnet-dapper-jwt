using Application.DTOs.Users;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System.Linq;

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
        // Check if model validation passed
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );
            throw new ValidationException(errors);
        }

        var role = await _unitOfWork.Roles.GetByNameAsync("User");
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