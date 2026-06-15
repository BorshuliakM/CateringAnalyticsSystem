using System.Security.Claims;

namespace CateringAnalyticsSystem.Controllers;

public static class CurrentUserExtensions
{
    public static int? GetEmployeeId(this ClaimsPrincipal user)
    {
        var employeeIdClaim = user.FindFirst("employeeId")?.Value;
        return int.TryParse(employeeIdClaim, out var employeeId) ? employeeId : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole("Admin");
    }
}
