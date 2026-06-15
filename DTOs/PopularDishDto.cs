namespace CateringAnalyticsSystem.DTOs;

public class PopularDishDto
{
    public int DishId { get; set; }
    public string DishName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}
