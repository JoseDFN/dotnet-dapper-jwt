namespace Application.Interfaces;

public interface IUnitOfWork
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    IOrderItemRepository OrderItems { get; }
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    
    Task<int> SaveAsync();
}