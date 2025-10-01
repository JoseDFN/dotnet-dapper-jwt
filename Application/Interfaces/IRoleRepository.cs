using Domain.Entities;

namespace Application.Interfaces;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string roleName);
}