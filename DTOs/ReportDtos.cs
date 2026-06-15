namespace CateringAnalyticsSystem.DTOs;

public class ReportFilterDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int? DiningTableId { get; set; }
    public int? EmployeeId { get; set; }
    public string? Status { get; set; }
    public decimal? MinTotal { get; set; }
    public decimal? MaxTotal { get; set; }
    public int? CategoryId { get; set; }
    public int? DishId { get; set; }
    public string? GroupBy { get; set; }
    public string? Format { get; set; }
    public int? Limit { get; set; }
}

public class OrderReportDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public int DiningTableNumber { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemsCount { get; set; }
    public List<OrderReportItemDto> Items { get; set; } = new();
}

public class OrderReportItemDto
{
    public string DishName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
}

public class SalesReportDto
{
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int ActiveOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageCheck { get; set; }
}

public class DishReportDto
{
    public int DishId { get; set; }
    public string DishName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class EmployeeReportDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageCheck { get; set; }
}

public class TableReportDto
{
    public int DiningTableId { get; set; }
    public int TableNumber { get; set; }
    public int OrdersCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageCheck { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
}

public class DailyReportDto
{
    public DateTime Date { get; set; }
    public int OrdersCount { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageCheck { get; set; }
}
