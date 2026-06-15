using CateringAnalyticsSystem.Data;
using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringAnalyticsSystem.Services;

public class ReportsService : IReportsService
{
    private readonly ApplicationDbContext _context;

    public ReportsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderReportDto>> GetOrdersAsync(ReportFilterDto filter)
    {
        var query = ApplyOrderFilters(BaseOrdersQuery(filter), filter);

        return await query
            .OrderByDescending(order => order.OrderDate)
            .Select(order => new OrderReportDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                DiningTableNumber = order.DiningTable!.Number,
                EmployeeName = order.Employee!.FullName,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                ItemsCount = order.OrderItems.Sum(item => item.Quantity),
                Items = order.OrderItems.Select(item => new OrderReportItemDto
                {
                    DishName = item.Dish!.Name,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Total = item.Price * item.Quantity
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<SalesReportDto> GetSalesAsync(ReportFilterDto filter)
    {
        var query = ApplyOrderFilters(BaseOrdersQuery(filter), filter);
        var completedQuery = query.Where(order => order.Status == "Completed");
        var completedOrders = await completedQuery.CountAsync();
        var totalRevenue = await completedQuery.SumAsync(order => (decimal?)order.TotalAmount) ?? 0;

        return new SalesReportDto
        {
            TotalOrders = await query.CountAsync(),
            CompletedOrders = completedOrders,
            CancelledOrders = await query.CountAsync(order => order.Status == "Cancelled"),
            ActiveOrders = await query.CountAsync(order => order.Status == "New" || order.Status == "InProgress"),
            TotalRevenue = totalRevenue,
            AverageCheck = completedOrders == 0 ? 0 : Math.Round(totalRevenue / completedOrders, 2)
        };
    }

    public async Task<List<DishReportDto>> GetDishesAsync(ReportFilterDto filter)
    {
        var query = BaseOrderItemsQuery(filter);

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(item => item.Dish!.CategoryId == filter.CategoryId.Value);
        }

        if (filter.DishId.HasValue)
        {
            query = query.Where(item => item.DishId == filter.DishId.Value);
        }

        return await query
            .GroupBy(item => new
            {
                item.DishId,
                DishName = item.Dish!.Name,
                CategoryName = item.Dish.Category!.Name
            })
            .Select(group => new DishReportDto
            {
                DishId = group.Key.DishId,
                DishName = group.Key.DishName,
                CategoryName = group.Key.CategoryName,
                QuantitySold = group.Sum(item => item.Quantity),
                TotalRevenue = group.Sum(item => item.Price * item.Quantity)
            })
            .OrderByDescending(result => result.TotalRevenue)
            .ToListAsync();
    }

    public async Task<List<EmployeeReportDto>> GetEmployeesAsync(ReportFilterDto filter)
    {
        var query = ApplyOrderFilters(BaseOrdersQuery(filter), filter)
            .Where(order => order.Status == "Completed");

        if (filter.EmployeeId.HasValue)
        {
            query = query.Where(order => order.EmployeeId == filter.EmployeeId.Value);
        }

        return await query
            .GroupBy(order => new { order.EmployeeId, EmployeeName = order.Employee!.FullName })
            .Select(group => new EmployeeReportDto
            {
                EmployeeId = group.Key.EmployeeId,
                EmployeeName = group.Key.EmployeeName,
                OrdersCount = group.Count(),
                TotalRevenue = group.Sum(order => order.TotalAmount),
                AverageCheck = group.Count() == 0 ? 0 : Math.Round(group.Sum(order => order.TotalAmount) / group.Count(), 2)
            })
            .OrderByDescending(result => result.TotalRevenue)
            .ToListAsync();
    }

    public async Task<List<TableReportDto>> GetTablesAsync(ReportFilterDto filter)
    {
        var completedOrders = ApplyOrderFilters(BaseOrdersQuery(filter), filter)
            .Where(order => order.Status == "Completed");

        if (filter.DiningTableId.HasValue)
        {
            completedOrders = completedOrders.Where(order => order.DiningTableId == filter.DiningTableId.Value);
        }

        var report = await completedOrders
            .GroupBy(order => new { order.DiningTableId, TableNumber = order.DiningTable!.Number, CurrentStatus = order.DiningTable.Status })
            .Select(group => new TableReportDto
            {
                DiningTableId = group.Key.DiningTableId,
                TableNumber = group.Key.TableNumber,
                OrdersCount = group.Count(),
                TotalRevenue = group.Sum(order => order.TotalAmount),
                AverageCheck = group.Count() == 0 ? 0 : Math.Round(group.Sum(order => order.TotalAmount) / group.Count(), 2),
                CurrentStatus = group.Key.CurrentStatus
            })
            .OrderBy(result => result.TableNumber)
            .ToListAsync();

        return report;
    }

    public async Task<List<DailyReportDto>> GetDailyAsync(ReportFilterDto filter)
    {
        var orders = ApplyOrderFilters(BaseOrdersQuery(filter), filter);

        return await orders
            .GroupBy(order => order.OrderDate.Date)
            .Select(group => new DailyReportDto
            {
                Date = group.Key,
                OrdersCount = group.Count(),
                CompletedOrders = group.Count(order => order.Status == "Completed"),
                CancelledOrders = group.Count(order => order.Status == "Cancelled"),
                TotalRevenue = group.Where(order => order.Status == "Completed").Sum(order => order.TotalAmount),
                AverageCheck = group.Count(order => order.Status == "Completed") == 0
                    ? 0
                    : Math.Round(group.Where(order => order.Status == "Completed").Sum(order => order.TotalAmount) / group.Count(order => order.Status == "Completed"), 2)
            })
            .OrderBy(result => result.Date)
            .ToListAsync();
    }

    private IQueryable<Order> BaseOrdersQuery(ReportFilterDto filter)
    {
        var from = filter.From == default ? DateTime.MinValue : filter.From;
        var to = filter.To == default ? DateTime.UtcNow.Date.AddDays(1).AddTicks(-1) : filter.To.Date.AddDays(1).AddTicks(-1);

        return _context.Orders
            .AsNoTracking()
            .Include(order => order.DiningTable)
            .Include(order => order.Employee)
            .Include(order => order.OrderItems)
            .ThenInclude(item => item.Dish)
            .Where(order => order.OrderDate >= from && order.OrderDate <= to);
    }

    private IQueryable<OrderItem> BaseOrderItemsQuery(ReportFilterDto filter)
    {
        var orderIds = ApplyOrderFilters(BaseOrdersQuery(filter), filter)
            .Where(order => order.Status == "Completed")
            .Select(order => order.Id);

        return _context.OrderItems
            .AsNoTracking()
            .Include(item => item.Dish)
            .ThenInclude(dish => dish!.Category)
            .Where(item => orderIds.Contains(item.OrderId));
    }

    private static IQueryable<Order> ApplyOrderFilters(IQueryable<Order> query, ReportFilterDto filter)
    {
        if (filter.DiningTableId.HasValue)
        {
            query = query.Where(order => order.DiningTableId == filter.DiningTableId.Value);
        }

        if (filter.EmployeeId.HasValue)
        {
            query = query.Where(order => order.EmployeeId == filter.EmployeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(order => order.Status == filter.Status);
        }

        if (filter.MinTotal.HasValue)
        {
            query = query.Where(order => order.TotalAmount >= filter.MinTotal.Value);
        }

        if (filter.MaxTotal.HasValue)
        {
            query = query.Where(order => order.TotalAmount <= filter.MaxTotal.Value);
        }

        return query;
    }
}
