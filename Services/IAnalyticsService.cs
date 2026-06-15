using CateringAnalyticsSystem.DTOs;

namespace CateringAnalyticsSystem.Services;

public interface IAnalyticsService
{
    Task<AnalyticsSummaryDto> GetSummaryAsync(ReportFilterDto filter);
    Task<List<SalesByPeriodDto>> GetSalesByPeriodAsync(ReportFilterDto filter);
    Task<List<PopularDishDto>> GetPopularDishesAsync(ReportFilterDto filter);
    Task<List<SalesByEmployeeDto>> GetSalesByEmployeeAsync(ReportFilterDto filter);
    Task<List<OrdersCountByDayDto>> GetOrdersCountByDayAsync();
    Task<List<RevenueByCategoryDto>> GetRevenueByCategoryAsync(ReportFilterDto filter);
    Task<List<RevenueByTableDto>> GetRevenueByTableAsync(ReportFilterDto filter);
    Task<List<TableOccupancyDto>> GetTableOccupancyAsync(ReportFilterDto filter);
}
