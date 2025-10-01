using Application.DTOs.Users;
using Application.Interfaces;
using Domain.Entities;
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
        var role = await _unitOfWork.Roles.GetByNameAsync("user");

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = dto.RoleId ?? role?.Id ?? 0,
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
        var user = await _unitOfWork.Users.GetUserWithRoleAsync(id);
        if (user == null) return NotFound();

        var response = new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username!,
            Role = user.Role?.Name ?? "user"
        };

        return Ok(response);
    }
}