namespace CateringAnalyticsSystem.DTOs;

public class SalesByPeriodDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int OrdersCount { get; set; }
    public decimal TotalSales { get; set; }
}
