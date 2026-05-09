using System.Security.Claims;
using System.Text;
using PARKit.Backend.DTOs;
using PARKit.Backend.DTOs.UserDtin;
using PARKit.Backend.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace PARKit.Backend.Services.AuthServices
{
    public class AuthServices : IAuthServices
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _repository;

    public AuthServices(IConfiguration configuration, IUserRepository userRepository)
    {
        _configuration = configuration;
        _repository = userRepository;
    }

    public async Task<string> Login(LoginDtin loginDtin) 
    {
        var usuario = await _repository.GetUserFromCredentialsAsync(loginDtin);
        if (usuario == null) throw new UnauthorizedAccessException("Credenciales incorrectas");
        
        return GenerateToken(usuario);
    }

    public async Task<string> Register(UserDtin userDtin) // Marcado como async Task
    {
        var existingUser = await _repository.GetUserByEmailAsync(userDtin.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("El email ya está registrado");
        }

        var usuarioDtoOut = await _repository.AddUserFromCredentialsAsync(userDtin);
        return GenerateToken(usuarioDtoOut);
    }

       public string GenerateToken(UserDto userDto)
        {
            var keyStr = _configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(keyStr)) throw new Exception("JWT Secret Key no configurada");
            
            var key = Encoding.UTF8.GetBytes(keyStr);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["Jwt:ValidIssuer"],
                Audience = _configuration["Jwt:ValidAudience"],
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                    new Claim(ClaimTypes.Name, userDto.Name),
                    new Claim(ClaimTypes.Email, userDto.Email),
                    new Claim(ClaimTypes.Role, userDto.Role),
                    new Claim("CreatedAt", userDto.CreatedAT.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool HasAccessToResource(int requestUsuarioId, ClaimsPrincipal usuario)
        {
            var usuarioIdClaim = usuario.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim.Value, out int usuarioId))
            {
                return false;
            }

            var isOwnResource = usuarioId == requestUsuarioId;
            var roleClaim = usuario.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            
            bool isAdmin = roleClaim != null && roleClaim.Value == "Admin"; 

            return isOwnResource || isAdmin;
        }
    }
}