using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.UserDtin;
using PARKit.Backend.Models;

namespace PARKit.Backend.Repositories
{
public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<UserDto> AddUserFromCredentialsAsync(UserDtin userDtin);
        Task<UserDto?> GetUserFromCredentialsAsync(LoginDtin loginDtin);
        Task<User?> GetByIdAsync(int id);
    }
}