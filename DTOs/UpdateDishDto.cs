using System.ComponentModel.DataAnnotations;

namespace CateringAnalyticsSystem.DTOs;

public class UpdateDishDto
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(700)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Dish price must be greater than 0.")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    public bool IsAvailable { get; set; }
}
