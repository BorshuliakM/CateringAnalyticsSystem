using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CateringAnalyticsSystem.Data;
using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CateringAnalyticsSystem.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserService _userService;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration,
        IPasswordHasher passwordHasher,
        IUserService userService)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _userService = userService;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Username == dto.Username.Trim());
        if (user is null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            return null;
        }

        return CreateAuthResponse(user);
    }

    public async Task<UserDto> RegisterAsync(RegisterUserDto dto)
    {
        return await _userService.CreateAsync(new CreateUserDto
        {
            Username = dto.Username,
            Password = dto.Password,
            Role = dto.Role,
            EmployeeId = dto.EmployeeId
        });
    }

    public AuthResponseDto CreateAuthResponse(User user)
    {
        var token = CreateToken(user);
        return new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Role = user.Role,
            EmployeeId = user.EmployeeId
        };
    }

    private string CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpireMinutes"));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        if (user.EmployeeId.HasValue)
        {
            claims.Add(new("employeeId", user.EmployeeId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
