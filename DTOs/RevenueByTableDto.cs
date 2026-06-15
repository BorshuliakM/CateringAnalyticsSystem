namespace CateringAnalyticsSystem.DTOs;

public class RevenueByTableDto
{
    public int DiningTableId { get; set; }
    public int TableNumber { get; set; }
    public int OrdersCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageCheck { get; set; }
}
