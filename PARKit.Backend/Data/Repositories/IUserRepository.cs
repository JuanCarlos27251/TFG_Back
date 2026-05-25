using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.UserDtin;
using PARKit.Backend.Models;

namespace PARKit.Backend.Repositories
{
public interface IUserRepository
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<User?> GetUserByEmailAsync(string email);
        Task<UserDto> AddUserFromCredentialsAsync(UserDtin userDtin);
        Task<UserDto?> GetUserFromCredentialsAsync(LoginDtin loginDtin);
        Task<User?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(int id, UserDtin userDtin);
        Task<bool> UpdatePasswordAsync(int id, string newPassword);
        Task<bool> DeleteAsync(int id);
    }
}