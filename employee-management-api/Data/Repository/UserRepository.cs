using Data.Context;
using Common.IRepository;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByUsernameAsync(string username, string? deletedBy)
        {
            var user = await GetByUsernameAsync(username);
            if (user != null)
            {
                user.IsDeleted = true;
                user.ModifiedBy = deletedBy;
                user.ModificationDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}