using System.Security.Claims;
using CateringAnalyticsSystem.Data;
using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CateringAnalyticsSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ApplicationDbContext _context;

    public AuthController(IAuthService authService, ApplicationDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var response = await _authService.LoginAsync(dto);
        return response is null ? Unauthorized(new { message = "Invalid username or password." }) : Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterUserDto dto)
    {
        try
        {
            return Ok(await _authService.RegisterAsync(dto));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .AsNoTracking()
            .Include(item => item.Employee)
            .Where(item => item.Id == userId)
            .Select(item => new UserDto
            {
                Id = item.Id,
                Username = item.Username,
                Role = item.Role,
                EmployeeId = item.EmployeeId,
                EmployeeName = item.Employee != null ? item.Employee.FullName : null
            })
            .FirstOrDefaultAsync();

        return user is null ? Unauthorized() : Ok(user);
    }
}
