using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Generic;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    }
}