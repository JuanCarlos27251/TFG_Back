using System.Security.Claims;
using System.Text;
using PARKit.Backend.DTOs.LoginDto;
using PARKit.Backend.DTOs.UserDtin;
using PARKit.Backend.DTOs.UserDto;
using PArRKit.Backend.Services.IAuthService;

namespace PARKit.Backend.Services.AuthServices
{
    public class AuthServices : IAuthServices
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _repository;

        public AuthServices (IConfiguration configuration,IUserRepository userRepository )
        {
            _configuration = configuration;
            _repository = _repository;
        }

          public string Login(LoginDtin loginDtin)
        {
            var usuario = _repository.GetUserFromCredentials(loginDtin);
            return GenerateToken(user);
        }

        public string Register(UserDtin userDtin)
        {
            try
            {
                // Verificar si el usuario ya existe
                var existingUser = _repository.GetUserByEmail(UserDtin.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException("El email ya está registrado");
                }

                // Añadir el nuevo usuario y obtener el DTO de salida
                var usuarioDtoOut = _repository.AddUserFromCredentials(UserDtin);

                // Generar y devolver el token
                return GenerateToken(usuarioDtoOut);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en el registro: {ex.Message}");
            }
        }

        public string GenerateToken(UserDto userDto)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]);
            var tokenDescrptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["Jwt:ValidIssuer"],
                Audience = _configuration["Jwt:ValidAudience"],
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Convert.ToString(userDto.Id)),
                    new Claim(ClaimTypes.Name, userDto.Name),
                    new Claim(ClaimTypes.Email, userDto.Email),
                    new Claim(ClaimTypes.Role, userDto.Role),
                    new Claim("CreatedAT", userDto.CreatedAT.ToString()),
                    new Claim("myCustomClaim", "myCustomClaimValue")
                }),
                Expires = DateTime.UtcNow.AddDays(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokendHandler = new JwtSecurityTokenHandler();
            var token = tokendHandler.CreateToken(tokenDescrptor);
            var tokenString = tokendHandler.WriteToken(token);
            return tokenString;
        }

        public bool HasAccessToResource(int requestUsuarioId, ClaimsPrincipal usuario)
        {
            var usuarioIdClaim = usuario.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (usuarioIdClaim is null || !int.TryParse(usuarioIdClaim.Value, out int usuarioId))
            {
                return false;
            }
            var isOwnResource = usuarioId == requestUsuarioId;
            var roleClaim = usuario.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (roleClaim != null) return false;
            var isAdmin = roleClaim!.Value == Role.Admin;

            var hasAccess = isOwnResource || isAdmin;
            return hasAccess;

        }
    }
}GCNotificationStatus 