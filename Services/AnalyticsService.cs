using CateringAnalyticsSystem.Data;
using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringAnalyticsSystem.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _context;

    public AnalyticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AnalyticsSummaryDto> GetSummaryAsync(ReportFilterDto filter)
    {
        var query = ApplyOrderFilters(BaseOrdersQuery(filter), filter);
        var completedQuery = query.Where(order => order.Status == "Completed");
        var completedOrders = await completedQuery.CountAsync();
        var totalRevenue = await completedQuery.SumAsync(order => (decimal?)order.TotalAmount) ?? 0;

        return new AnalyticsSummaryDto
        {
            TotalOrders = await query.CountAsync(),
            CompletedOrders = completedOrders,
            CancelledOrders = await query.CountAsync(order => order.Status == "Cancelled"),
            ActiveOrders = await query.CountAsync(order => order.Status == "New" || order.Status == "InProgress"),
            TotalRevenue = totalRevenue,
            AverageCheck = completedOrders == 0 ? 0 : Math.Round(totalRevenue / completedOrders, 2),
            MinOrderTotal = await completedQuery.MinAsync(order => (decimal?)order.TotalAmount) ?? 0,
            MaxOrderTotal = await completedQuery.MaxAsync(order => (decimal?)order.TotalAmount) ?? 0
        };
    }

    public async Task<List<SalesByPeriodDto>> GetSalesByPeriodAsync(ReportFilterDto filter)
    {
        var groupBy = string.IsNullOrWhiteSpace(filter.GroupBy) ? "day" : filter.GroupBy.Trim().ToLowerInvariant();
        var orders = await ApplyOrderFilters(BaseOrdersQuery(filter), filter)
            .Where(order => order.Status == "Completed")
            .ToListAsync();

        var grouped = groupBy switch
        {
            "month" => orders.GroupBy(order => new DateTime(order.OrderDate.Year, order.OrderDate.Month, 1).ToString("yyyy-MM")),
            "week" => orders.GroupBy(order => $"{order.OrderDate.Year}-W{GetIsoWeek(order.OrderDate):00}"),
            _ => orders.GroupBy(order => order.OrderDate.Date.ToString("yyyy-MM-dd"))
        };

        return grouped
            .Select(group =>
            {
                var revenue = group.Sum(order => order.TotalAmount);
                var count = group.Count();
                return new SalesByPeriodDto
                {
                    Period = group.Key,
                    OrdersCount = count,
                    TotalRevenue = revenue,
                    AverageCheck = count == 0 ? 0 : Math.Round(revenue / count, 2)
                };
            })
            .OrderBy(item => item.Period)
            .ToList();
    }

    public async Task<List<PopularDishDto>> GetPopularDishesAsync(ReportFilterDto filter)
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

        var limit = filter.Limit ?? 10;
        if (limit <= 0)
        {
            limit = 10;
        }

        return await query
            .GroupBy(item => new
            {
                item.DishId,
                DishName = item.Dish!.Name,
                CategoryName = item.Dish.Category!.Name
            })
            .Select(group => new PopularDishDto
            {
                DishId = group.Key.DishId,
                DishName = group.Key.DishName,
                CategoryName = group.Key.CategoryName,
                QuantitySold = group.Sum(item => item.Quantity),
                TotalRevenue = group.Sum(item => item.Price * item.Quantity)
            })
            .OrderByDescending(result => result.QuantitySold)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<SalesByEmployeeDto>> GetSalesByEmployeeAsync(ReportFilterDto filter)
    {
        return await ApplyOrderFilters(BaseOrdersQuery(filter), filter)
            .Where(order => order.Status == "Completed")
            .GroupBy(order => new { order.EmployeeId, EmployeeName = order.Employee!.FullName })
            .Select(group => new SalesByEmployeeDto
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

    public async Task<List<RevenueByCategoryDto>> GetRevenueByCategoryAsync(ReportFilterDto filter)
    {
        return await BaseOrderItemsQuery(filter)
            .GroupBy(item => new
            {
                CategoryId = item.Dish!.CategoryId,
                CategoryName = item.Dish.Category!.Name
            })
            .Select(group => new RevenueByCategoryDto
            {
                CategoryId = group.Key.CategoryId,
                CategoryName = group.Key.CategoryName,
                QuantitySold = group.Sum(item => item.Quantity),
                TotalRevenue = group.Sum(item => item.Price * item.Quantity)
            })
            .OrderByDescending(result => result.TotalRevenue)
            .ToListAsync();
    }

    public async Task<List<RevenueByTableDto>> GetRevenueByTableAsync(ReportFilterDto filter)
    {
        return await ApplyOrderFilters(BaseOrdersQuery(filter), filter)
            .Where(order => order.Status == "Completed")
            .GroupBy(order => new { order.DiningTableId, TableNumber = order.DiningTable!.Number })
            .Select(group => new RevenueByTableDto
            {
                DiningTableId = group.Key.DiningTableId,
                TableNumber = group.Key.TableNumber,
                OrdersCount = group.Count(),
                TotalRevenue = group.Sum(order => order.TotalAmount),
                AverageCheck = group.Count() == 0 ? 0 : Math.Round(group.Sum(order => order.TotalAmount) / group.Count(), 2)
            })
            .OrderBy(result => result.TableNumber)
            .ToListAsync();
    }

    public async Task<List<TableOccupancyDto>> GetTableOccupancyAsync(ReportFilterDto filter)
    {
        var completedOrders = ApplyOrderFilters(BaseOrdersQuery(filter), filter)
            .Where(order => order.Status == "Completed");

        return await _context.DiningTables
            .AsNoTracking()
            .GroupJoin(
                completedOrders,
                table => table.Id,
                order => order.DiningTableId,
                (table, orders) => new TableOccupancyDto
                {
                    DiningTableId = table.Id,
                    TableNumber = table.Number,
                    OrdersCount = orders.Count(),
                    CurrentStatus = table.Status,
                    TotalRevenue = orders.Sum(order => order.TotalAmount)
                })
            .OrderBy(result => result.TableNumber)
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
            .Where(order => order.OrderDate >= from && order.OrderDate <= to);
    }

    private IQueryable<OrderItem> BaseOrderItemsQuery(ReportFilterDto filter)
    {
        var orderQuery = ApplyOrderFilters(BaseOrdersQuery(filter), filter)
            .Where(order => order.Status == "Completed")
            .Select(order => order.Id);

        return _context.OrderItems
            .AsNoTracking()
            .Include(item => item.Dish)
            .ThenInclude(dish => dish!.Category)
            .Where(item => orderQuery.Contains(item.OrderId));
    }

    private static IQueryable<Order> ApplyOrderFilters(IQueryable<Order> query, ReportFilterDto filter)
    {
        if (filter.EmployeeId.HasValue)
        {
            query = query.Where(order => order.EmployeeId == filter.EmployeeId.Value);
        }

        if (filter.DiningTableId.HasValue)
        {
            query = query.Where(order => order.DiningTableId == filter.DiningTableId.Value);
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

    private static int GetIsoWeek(DateTime date)
    {
        return System.Globalization.ISOWeek.GetWeekOfYear(date);
    }
}
