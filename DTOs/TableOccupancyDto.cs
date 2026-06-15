namespace CateringAnalyticsSystem.DTOs;

public class TableOccupancyDto
{
    public int DiningTableId { get; set; }
    public int TableNumber { get; set; }
    public int OrdersCount { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
}
