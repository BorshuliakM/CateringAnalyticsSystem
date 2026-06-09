using System.ComponentModel.DataAnnotations;

namespace CateringAnalyticsSystem.DTOs;

public class CreateOrderItemDto
{
    [Range(1, int.MaxValue)]
    public int DishId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Order item quantity must be greater than 0.")]
    public int Quantity { get; set; }
}
