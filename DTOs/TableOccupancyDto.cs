namespace CateringAnalyticsSystem.DTOs;

public class TableOccupancyDto
{
    public int TableNumber { get; set; }
    public int OrdersCount { get; set; }
    public string Status { get; set; } = string.Empty;
}
