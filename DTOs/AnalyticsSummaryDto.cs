namespace CateringAnalyticsSystem.DTOs;

public class AnalyticsSummaryDto
{
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int ActiveOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageCheck { get; set; }
    public decimal MinOrderTotal { get; set; }
    public decimal MaxOrderTotal { get; set; }
}
