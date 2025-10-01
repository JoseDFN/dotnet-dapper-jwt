using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    // Para login
    Task<User?> GetByUsernameAsync(string username);

    // Para devolver información con rol incluido
    Task<User?> GetUserWithRoleAsync(int id);

    Task<(int UserId, string RoleName)?> AuthenticateAsync(string username, string passwordHash);
}