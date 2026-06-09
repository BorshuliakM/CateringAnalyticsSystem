namespace CateringAnalyticsSystem.DTOs;

public class RevenueByCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}
