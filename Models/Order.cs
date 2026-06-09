using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CateringAnalyticsSystem.Models;

public class Order
{
    public int Id { get; set; }

    public int DiningTableId { get; set; }
    public DiningTable? DiningTable { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "New";

    public List<OrderItem> OrderItems { get; set; } = new();
}
