using Domain.Entities;

namespace Application.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IEnumerable<Product>> GetAllAsync(string? category = null, string? name = null);
}