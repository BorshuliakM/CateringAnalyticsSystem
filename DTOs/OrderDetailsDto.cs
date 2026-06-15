namespace CateringAnalyticsSystem.DTOs;

public class OrderDetailsDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DiningTableId { get; set; }
    public int DiningTableNumber { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public List<OrderItemDetailsDto> Items { get; set; } = new();
}

public class OrderItemDetailsDto
{
    public int Id { get; set; }
    public int DishId { get; set; }
    public string DishName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal LineTotal { get; set; }
}
