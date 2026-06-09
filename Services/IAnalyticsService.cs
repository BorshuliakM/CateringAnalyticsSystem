using CateringAnalyticsSystem.DTOs;

namespace CateringAnalyticsSystem.Services;

public interface IAnalyticsService
{
    Task<AnalyticsSummaryDto> GetSummaryAsync();
    Task<SalesByPeriodDto> GetSalesByPeriodAsync(DateTime from, DateTime to);
    Task<List<PopularDishDto>> GetPopularDishesAsync();
    Task<List<SalesByEmployeeDto>> GetSalesByEmployeeAsync();
    Task<List<OrdersCountByDayDto>> GetOrdersCountByDayAsync();
    Task<List<RevenueByCategoryDto>> GetRevenueByCategoryAsync();
    Task<List<RevenueByTableDto>> GetRevenueByTableAsync();
    Task<List<TableOccupancyDto>> GetTableOccupancyAsync();
}
