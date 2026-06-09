using System.ComponentModel.DataAnnotations;

namespace CateringAnalyticsSystem.Models;

public class Employee
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Position { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Email { get; set; } = string.Empty;

    public List<Order> Orders { get; set; } = new();
}
