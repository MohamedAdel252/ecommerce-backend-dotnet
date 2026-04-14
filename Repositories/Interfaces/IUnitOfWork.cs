using ECommerceAPI.Repositories.Interfaces;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        IUserRepository Users { get; }
        ICartRepository Carts { get; }
        IOrderRepository Orders { get; }
        IRecommendationRepository Recommendations { get; }

        Task<int> SaveChangesAsync();
    }
}