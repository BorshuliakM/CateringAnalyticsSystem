namespace CateringAnalyticsSystem.DTOs;

public class SalesByPeriodDto
{
    public string Period { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageCheck { get; set; }
}
