using System.ComponentModel.DataAnnotations;

namespace CateringAnalyticsSystem.DTOs;

public class LoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
}

public class RegisterUserDto
{
    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Role { get; set; } = "Waiter";

    public int? EmployeeId { get; set; }
}

public class CreateUserDto : RegisterUserDto
{
}

public class UpdateUserDto
{
    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [MinLength(6)]
    public string? Password { get; set; }

    [Required, MaxLength(50)]
    public string Role { get; set; } = "Waiter";

    public int? EmployeeId { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
}
