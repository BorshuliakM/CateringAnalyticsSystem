using System.ComponentModel.DataAnnotations;

namespace CateringAnalyticsSystem.Models;

public class DiningTable
{
    public int Id { get; set; }

    public int Number { get; set; }

    public int SeatsCount { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Free";

    public List<Order> Orders { get; set; } = new();
}
