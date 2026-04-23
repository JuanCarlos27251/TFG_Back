using System.Security.Claims;
using PARKit.Backend.DTOs.LoginDto;
using PARKit.Backend.DTOs.UserDto;


namespace PArRKit.Backend.Services.IAuthService
{
    public interface IAuthServices
    {
        public string Login(LoginDtin userDtin);
        public string Register(UserDto userDto);

        public string GenerateToken(UserDto userDto);
        public bool HasAccessToResource(int requestedUserId, ClaimsPrincipal user);
    }
}