namespace CateringAnalyticsSystem.DTOs;

public class SalesByEmployeeDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int OrdersCount { get; set; }
}
