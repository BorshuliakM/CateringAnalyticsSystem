using CateringAnalyticsSystem.Data;
using CateringAnalyticsSystem.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CateringAnalyticsSystem.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _context;

    public AnalyticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AnalyticsSummaryDto> GetSummaryAsync()
    {
        var orders = await _context.Orders.AsNoTracking().ToListAsync();
        var totalSales = orders.Sum(o => o.TotalAmount);

        return new AnalyticsSummaryDto
        {
            TotalOrders = orders.Count,
            TotalSales = totalSales,
            AverageCheck = orders.Count == 0 ? 0 : Math.Round(totalSales / orders.Count, 2)
        };
    }

    public async Task<SalesByPeriodDto> GetSalesByPeriodAsync(DateTime from, DateTime to)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.OrderDate >= from && o.OrderDate <= to)
            .ToListAsync();

        return new SalesByPeriodDto
        {
            From = from,
            To = to,
            OrdersCount = orders.Count,
            TotalSales = orders.Sum(o => o.TotalAmount)
        };
    }

    public async Task<List<PopularDishDto>> GetPopularDishesAsync()
    {
        return await _context.OrderItems
            .AsNoTracking()
            .Include(oi => oi.Dish)
            .GroupBy(oi => new { oi.DishId, DishName = oi.Dish!.Name })
            .Select(group => new PopularDishDto
            {
                DishId = group.Key.DishId,
                DishName = group.Key.DishName,
                TotalQuantity = group.Sum(item => item.Quantity),
                TotalRevenue = group.Sum(item => item.Price * item.Quantity)
            })
            .OrderByDescending(result => result.TotalQuantity)
            .ToListAsync();
    }

    public async Task<List<SalesByEmployeeDto>> GetSalesByEmployeeAsync()
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Employee)
            .GroupBy(o => new { o.EmployeeId, EmployeeName = o.Employee!.FullName })
            .Select(group => new SalesByEmployeeDto
            {
                EmployeeId = group.Key.EmployeeId,
                EmployeeName = group.Key.EmployeeName,
                TotalSales = group.Sum(order => order.TotalAmount),
                OrdersCount = group.Count()
            })
            .OrderByDescending(result => result.TotalSales)
            .ToListAsync();
    }

    public async Task<List<OrdersCountByDayDto>> GetOrdersCountByDayAsync()
    {
        return await _context.Orders
            .AsNoTracking()
            .GroupBy(o => o.OrderDate.Date)
            .Select(group => new OrdersCountByDayDto
            {
                Date = group.Key,
                OrdersCount = group.Count()
            })
            .OrderBy(result => result.Date)
            .ToListAsync();
    }

    public async Task<List<RevenueByCategoryDto>> GetRevenueByCategoryAsync()
    {
        return await _context.OrderItems
            .AsNoTracking()
            .Include(oi => oi.Dish)
            .ThenInclude(d => d!.Category)
            .GroupBy(oi => new
            {
                CategoryId = oi.Dish!.CategoryId,
                CategoryName = oi.Dish.Category!.Name
            })
            .Select(group => new RevenueByCategoryDto
            {
                CategoryId = group.Key.CategoryId,
                CategoryName = group.Key.CategoryName,
                Revenue = group.Sum(item => item.Price * item.Quantity)
            })
            .OrderByDescending(result => result.Revenue)
            .ToListAsync();
    }

    public async Task<List<RevenueByTableDto>> GetRevenueByTableAsync()
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(order => order.DiningTable)
            .GroupBy(order => new
            {
                TableNumber = order.DiningTable!.Number
            })
            .Select(group => new RevenueByTableDto
            {
                TableNumber = group.Key.TableNumber,
                OrdersCount = group.Count(),
                TotalRevenue = group.Sum(order => order.TotalAmount)
            })
            .OrderBy(result => result.TableNumber)
            .ToListAsync();
    }

    public async Task<List<TableOccupancyDto>> GetTableOccupancyAsync()
    {
        return await _context.DiningTables
            .AsNoTracking()
            .Select(table => new TableOccupancyDto
            {
                TableNumber = table.Number,
                OrdersCount = table.Orders.Count,
                Status = table.Status
            })
            .OrderBy(result => result.TableNumber)
            .ToListAsync();
    }
}
