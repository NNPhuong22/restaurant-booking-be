using Microsoft.EntityFrameworkCore;
using TableOrder_Hust.Data;
using TableOrder_Hust.Models;

namespace TableOrder_Hust.Services
{
    public interface IUserService
    {
        Task<User?> RegisterAsync(string email, string password);
        Task<User?> LoginAsync(string email, string password);

        Task<User> GoogleLoginAsync(string googleId, string email);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<User?> RegisterAsync(string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return null;

            var user = new User
            {
                Email = email,
                Password = password // ⚠️ Chỉ lưu plain text theo yêu cầu
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.Password != password)
                return null;

            return user;
        }
        public async Task<User> GoogleLoginAsync(string googleId, string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

            if (user == null)
            {
                // tạo mới user từ Google
                user = new User
                {
                    Email = email,
                    GoogleId = googleId
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return user;
        }

    }
}
