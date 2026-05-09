using System.Security.Claims;
using PARKit.Backend.DTOs;

using PARKit.Backend.DTOs.UserDtin;


namespace PARKit.Backend.Services.AuthServices
{
    public interface IAuthServices
    {
        Task<string> Login(LoginDtin loginDtin);
        Task<string> Register(UserDtin userDtin);

        public string GenerateToken(UserDto userDto);
        public bool HasAccessToResource(int requestedUserId, ClaimsPrincipal user);
    }
}