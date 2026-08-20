using Core.Entities;

namespace Common.IRepository
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> ExistsByUsernameAsync(string username);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteByUsernameAsync(string username, string? deletedBy);
    }
}