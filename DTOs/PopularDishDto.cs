namespace CateringAnalyticsSystem.DTOs;

public class PopularDishDto
{
    public int DishId { get; set; }
    public string DishName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}
