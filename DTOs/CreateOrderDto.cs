using System.ComponentModel.DataAnnotations;

namespace CateringAnalyticsSystem.DTOs;

public class CreateOrderDto
{
    [Range(1, int.MaxValue)]
    public int DiningTableId { get; set; }

    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [MinLength(1, ErrorMessage = "Order cannot be created without items.")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}
