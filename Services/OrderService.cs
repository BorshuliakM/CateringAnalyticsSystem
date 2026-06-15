using CateringAnalyticsSystem.Data;
using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringAnalyticsSystem.Services;

public class OrderService : IOrderService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "New",
        "InProgress",
        "Completed",
        "Cancelled"
    };

    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderDetailsDto>> GetAllAsync(DateTime? from, DateTime? to, int? diningTableId, int? employeeId, string? status)
    {
        var query = _context.Orders
            .Include(o => o.DiningTable)
            .Include(o => o.Employee)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Dish)
            .AsNoTracking()
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(o => o.OrderDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(o => o.OrderDate <= to.Value);
        }

        if (diningTableId.HasValue)
        {
            query = query.Where(o => o.DiningTableId == diningTableId.Value);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(o => o.EmployeeId == employeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o => o.Status == status);
        }

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(ToDetailsDto).ToList();
    }

    public async Task<OrderDetailsDto?> GetByIdAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.DiningTable)
            .Include(o => o.Employee)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Dish)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        return order is null ? null : ToDetailsDto(order);
    }

    public async Task<OrderDetailsDto> CreateAsync(CreateOrderDto dto, bool isAdmin, int? currentEmployeeId)
    {
        if (dto.Items.Count == 0)
        {
            throw new ArgumentException("Order cannot be created without items.");
        }

        var employeeId = dto.EmployeeId;
        if (!isAdmin)
        {
            if (currentEmployeeId is null)
            {
                throw new UnauthorizedAccessException("Waiter account is not connected to an employee.");
            }

            if (dto.EmployeeId != 0 && dto.EmployeeId != currentEmployeeId.Value)
            {
                throw new UnauthorizedAccessException("Waiter cannot create orders for another employee.");
            }

            employeeId = currentEmployeeId.Value;
        }

        var table = await _context.DiningTables.FirstOrDefaultAsync(t => t.Id == dto.DiningTableId);
        var employeeExists = await _context.Employees.AnyAsync(e => e.Id == employeeId);

        if (table is null)
        {
            throw new ArgumentException("Selected dining table does not exist.");
        }

        if (table.Status != "Free")
        {
            throw new ArgumentException("Cannot create a new active order for a dining table that is not free.");
        }

        if (!employeeExists)
        {
            throw new ArgumentException("Selected employee does not exist.");
        }

        var order = new Order
        {
            DiningTableId = dto.DiningTableId,
            EmployeeId = employeeId,
            OrderDate = DateTime.UtcNow,
            Status = "New"
        };

        foreach (var itemDto in dto.Items)
        {
            if (itemDto.Quantity <= 0)
            {
                throw new ArgumentException("Order item quantity must be greater than 0.");
            }

            var dish = await _context.Dishes.FirstOrDefaultAsync(d => d.Id == itemDto.DishId);
            if (dish is null)
            {
                throw new ArgumentException($"Dish with id {itemDto.DishId} does not exist.");
            }

            if (!dish.IsAvailable)
            {
                throw new ArgumentException($"Dish '{dish.Name}' is not available.");
            }

            order.OrderItems.Add(new OrderItem
            {
                DishId = dish.Id,
                Quantity = itemDto.Quantity,
                Price = dish.Price
            });
        }

        order.TotalAmount = order.OrderItems.Sum(item => item.Price * item.Quantity);
        table.Status = "Occupied";

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var created = await GetByIdAsync(order.Id);
        return created!;
    }

    public async Task<bool> ChangeStatusAsync(int id, string status, bool isAdmin, int? currentEmployeeId)
    {
        if (string.IsNullOrWhiteSpace(status) || !AllowedStatuses.Contains(status))
        {
            throw new ArgumentException("Order status must be New, InProgress, Completed or Cancelled.");
        }

        var order = await _context.Orders.FindAsync(id);
        if (order is null)
        {
            return false;
        }

        if (!isAdmin && order.EmployeeId != currentEmployeeId)
        {
            throw new UnauthorizedAccessException("Waiter can update only own orders.");
        }

        order.Status = NormalizeStatus(status);
        await UpdateDiningTableStatusAfterOrderStatusChangeAsync(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null)
        {
            return false;
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }

    private static OrderDetailsDto ToDetailsDto(Order order)
    {
        return new OrderDetailsDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            DiningTableId = order.DiningTableId,
            DiningTableNumber = order.DiningTable?.Number ?? 0,
            EmployeeId = order.EmployeeId,
            EmployeeName = order.Employee?.FullName ?? string.Empty,
            Items = order.OrderItems.Select(item => new OrderItemDetailsDto
            {
                Id = item.Id,
                DishId = item.DishId,
                DishName = item.Dish?.Name ?? string.Empty,
                Quantity = item.Quantity,
                Price = item.Price,
                LineTotal = item.Price * item.Quantity
            }).ToList()
        };
    }

    private async Task UpdateDiningTableStatusAfterOrderStatusChangeAsync(Order order)
    {
        var normalizedStatus = order.Status.Trim();
        if (normalizedStatus is not ("Completed" or "Cancelled"))
        {
            return;
        }

        var table = await _context.DiningTables.FindAsync(order.DiningTableId);
        if (table is null)
        {
            return;
        }

        var hasOtherActiveOrders = await _context.Orders.AnyAsync(existingOrder =>
            existingOrder.Id != order.Id &&
            existingOrder.DiningTableId == order.DiningTableId &&
            existingOrder.Status != "Completed" &&
            existingOrder.Status != "Cancelled");

        if (!hasOtherActiveOrders)
        {
            table.Status = "Free";
        }
    }

    private static string NormalizeStatus(string status)
    {
        return AllowedStatuses.First(allowed => string.Equals(allowed, status, StringComparison.OrdinalIgnoreCase));
    }
}
