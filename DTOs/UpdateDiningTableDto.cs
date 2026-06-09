using System.ComponentModel.DataAnnotations;

namespace CateringAnalyticsSystem.DTOs;

public class UpdateDiningTableDto
{
    [Range(1, int.MaxValue)]
    public int Number { get; set; }

    [Range(1, 50)]
    public int SeatsCount { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Free";
}
