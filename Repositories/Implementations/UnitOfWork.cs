using ECommerceAPI.Data;
using ECommerceAPI.Repositories.Interfaces;

namespace ECommerceAPI.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IProductRepository Products { get; }
        public ICategoryRepository Categories { get; }
        public IUserRepository Users { get; }
        public ICartRepository Carts { get; }
        public IOrderRepository Orders { get; }
        public IRecommendationRepository Recommendations { get; }

        public UnitOfWork(
            AppDbContext context,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IUserRepository userRepository,
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            IRecommendationRepository recommendationRepository)
        {
            _context = context;
            Products = productRepository;
            Categories = categoryRepository;
            Users = userRepository;
            Carts = cartRepository;
            Orders = orderRepository;
            Recommendations = recommendationRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}