using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    // Para login
    Task<User?> GetByUsernameAsync(string username);

    // Para devolver información con rol incluido
    Task<User?> GetUserWithRoleAsync(int id);

    // Para refresh token
    Task<User?> GetByRefreshTokenAsync(string refreshToken);

    Task<(int UserId, string RoleName)?> AuthenticateAsync(string username, string passwordHash);
}