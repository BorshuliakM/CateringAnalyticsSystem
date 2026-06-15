using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Models;

namespace CateringAnalyticsSystem.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<UserDto> RegisterAsync(RegisterUserDto dto);
    AuthResponseDto CreateAuthResponse(User user);
}
