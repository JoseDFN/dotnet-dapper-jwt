using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _config = config;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        // obtener usuario
        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);

        if (user == null)
            return null;

        // 🔑 validación simple, en un real project usarías BCrypt o Argon2
        if (user.PasswordHash != request.Password)
            return null;

        // obtener rol
        var role = (await _unitOfWork.Roles.GetByNameAsync("user"))?.Name ?? "user";
        if (user.RoleId != 0)
        {
            var userWithRole = await _unitOfWork.Users.GetUserWithRoleAsync(user.Id);
            if (userWithRole?.Role?.Name != null)
                role = userWithRole.Role.Name;
        }

        // generar token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["JWT:Key"]!);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username!),
            new Claim(ClaimTypes.Role, role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["JWT:DurationInMinutes"]!)),
            Issuer = _config["JWT:Issuer"],
            Audience = _config["JWT:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new LoginResponseDto
        {
            Token = tokenHandler.WriteToken(token),
            Username = user.Username!,
            Role = role
        };
    }
}