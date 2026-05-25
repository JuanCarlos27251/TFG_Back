using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.UserDtin;

namespace PARKit.Backend.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto> CreateUserAsync(UserDtin dtin);
        Task<bool> UpdateUserAsync(int id, UserDtin dtin);
        Task<bool> DeleteUserAsync(int id);
    }
}