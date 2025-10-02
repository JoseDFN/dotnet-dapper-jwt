using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
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

        // generar refresh token
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7); // 7 días de expiración

        // guardar refresh token en la base de datos
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveAsync();

        return new LoginResponseDto
        {
            Token = tokenHandler.WriteToken(token),
            RefreshToken = refreshToken,
            Username = user.Username!,
            Role = role
        };
    }

    public async Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken);

        if (user == null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
            return null;

        // obtener rol
        var role = (await _unitOfWork.Roles.GetByNameAsync("user"))?.Name ?? "user";
        if (user.RoleId != 0)
        {
            var userWithRole = await _unitOfWork.Users.GetUserWithRoleAsync(user.Id);
            if (userWithRole?.Role?.Name != null)
                role = userWithRole.Role.Name;
        }

        // generar nuevo token
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

        // generar nuevo refresh token
        var newRefreshToken = GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        // actualizar refresh token en la base de datos
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveAsync();

        return new LoginResponseDto
        {
            Token = tokenHandler.WriteToken(token),
            RefreshToken = newRefreshToken,
            Username = user.Username!,
            Role = role
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(refreshToken);

        if (user == null)
            return false;

        // revocar refresh token
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveAsync();

        return true;
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}