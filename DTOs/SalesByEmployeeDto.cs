namespace CateringAnalyticsSystem.DTOs;

public class SalesByEmployeeDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageCheck { get; set; }
}
