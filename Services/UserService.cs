using CateringAnalyticsSystem.Data;
using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringAnalyticsSystem.Services;

public class UserService : IUserService
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "Waiter"
    };

    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Include(user => user.Employee)
            .OrderBy(user => user.Username)
            .Select(user => new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                EmployeeId = user.EmployeeId,
                EmployeeName = user.Employee != null ? user.Employee.FullName : null
            })
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(item => item.Employee)
            .FirstOrDefaultAsync(item => item.Id == id);

        return user is null ? null : ToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        Validate(dto.Username, dto.Password, dto.Role, dto.EmployeeId);
        await EnsureUsernameIsUniqueAsync(dto.Username);
        await ValidateEmployeeLinkAsync(dto.Role, dto.EmployeeId);

        var user = new User
        {
            Username = dto.Username.Trim(),
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Role = NormalizeRole(dto.Role),
            EmployeeId = NormalizeRole(dto.Role) == "Waiter" ? dto.EmployeeId : null
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(user.Id))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserDto dto)
    {
        Validate(dto.Username, dto.Password, dto.Role, dto.EmployeeId, passwordRequired: false);

        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return false;
        }

        var usernameExists = await _context.Users.AnyAsync(item => item.Id != id && item.Username == dto.Username.Trim());
        if (usernameExists)
        {
            throw new ArgumentException("User with this username already exists.");
        }

        await ValidateEmployeeLinkAsync(dto.Role, dto.EmployeeId);

        user.Username = dto.Username.Trim();
        user.Role = NormalizeRole(dto.Role);
        user.EmployeeId = user.Role == "Waiter" ? dto.EmployeeId : null;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = _passwordHasher.Hash(dto.Password);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task EnsureUsernameIsUniqueAsync(string username)
    {
        var exists = await _context.Users.AnyAsync(user => user.Username == username.Trim());
        if (exists)
        {
            throw new ArgumentException("User with this username already exists.");
        }
    }

    private async Task ValidateEmployeeLinkAsync(string role, int? employeeId)
    {
        var normalizedRole = NormalizeRole(role);
        if (normalizedRole == "Waiter" && employeeId is null)
        {
            throw new ArgumentException("Waiter user must be connected to an employee.");
        }

        if (employeeId.HasValue && !await _context.Employees.AnyAsync(employee => employee.Id == employeeId.Value))
        {
            throw new ArgumentException("Selected employee does not exist.");
        }
    }

    private static void Validate(string username, string? password, string role, int? employeeId, bool passwordRequired = true)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be empty.");
        }

        if (passwordRequired && string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty.");
        }

        if (!string.IsNullOrWhiteSpace(password) && password.Length < 6)
        {
            throw new ArgumentException("Password must contain at least 6 characters.");
        }

        if (string.IsNullOrWhiteSpace(role) || !AllowedRoles.Contains(role))
        {
            throw new ArgumentException("Role must be Admin or Waiter.");
        }
    }

    private static string NormalizeRole(string role)
    {
        return AllowedRoles.First(allowed => string.Equals(allowed, role, StringComparison.OrdinalIgnoreCase));
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            EmployeeId = user.EmployeeId,
            EmployeeName = user.Employee?.FullName
        };
    }
}
