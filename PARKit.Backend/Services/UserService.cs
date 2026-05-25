using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.UserDtin;
using PARKit.Backend.Repositories;
using PARKit.Backend.Services.Interfaces;

namespace PARKit.Backend.Services
{
    public class UserService : IUserService
    {
private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            // Asumiendo que el repositorio tiene un método GetAllAsync
            return await _userRepository.GetAllAsync();
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAT = user.CreatedAT
            };
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("El email no puede estar vacío.");
            }

            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAT = user.CreatedAT
            };
        }

        public async Task<UserDto> CreateUserAsync(UserDtin dtin)
        {
            if (string.IsNullOrWhiteSpace(dtin.Email))
            {
                throw new ArgumentException("El email es obligatorio.");
            }
            if (string.IsNullOrWhiteSpace(dtin.Password))
            {
                throw new ArgumentException("La contraseña es obligatoria.");
            }
            if (string.IsNullOrWhiteSpace(dtin.Name))
            {
                throw new ArgumentException("El nombre es obligatorio.");
            }

            var existingUser = await _userRepository.GetUserByEmailAsync(dtin.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("El email ya está registrado.");
            }

            return await _userRepository.AddUserFromCredentialsAsync(dtin);
        }

        public async Task<bool> UpdateUserAsync(int id, UserDtin dtin)
        {
            if (string.IsNullOrWhiteSpace(dtin.Name))
            {
                throw new ArgumentException("El nombre no puede estar vacío.");
            }

            return await _userRepository.UpdateAsync(id, dtin);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {

            return await _userRepository.DeleteAsync(id);
        }
    }
}