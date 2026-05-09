using Microsoft.EntityFrameworkCore;
using PARKit.Backend.Data;
using PARKit.Backend.Models;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.UserDtin;
using PARKit.Backend.Enums;


namespace PARKit.Backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {

            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserDto> AddUserFromCredentialsAsync(UserDtin userDtin)
        {
            var user = new User
            {
                Name = userDtin.Name,
                Email = userDtin.Email,
                Phone = userDtin.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDtin.Password),
                Role = UserRole.User.ToString(), 
                IsActive = true,
                CreatedAT = DateTime.UtcNow,
                CompanyId = null 
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return new UserDto 
            { 
                Id = user.Id, 
                Name = user.Name, 
                Email = user.Email, 
                Role = user.Role,
                Phone = user.Phone,
                CreatedAT = user.CreatedAT,
                IsActive = user.IsActive
            };
        }

        public async Task<UserDto?> GetUserFromCredentialsAsync(LoginDtin loginDtin)
        {
            var user = await GetUserByEmailAsync(loginDtin.Email);
            
            // Verificamos si existe y si la password coincide con el hash
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDtin.Password, user.PasswordHash))
            {
                return null;
            }

            return new UserDto 
            { 
                Id = user.Id, 
                Name = user.Name, 
                Email = user.Email, 
                Role = user.Role,
                Phone = user.Phone,
                CreatedAT = user.CreatedAT,
                IsActive = user.IsActive
            };
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }
    }
}