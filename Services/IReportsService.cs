using CateringAnalyticsSystem.DTOs;

namespace CateringAnalyticsSystem.Services;

public interface IReportsService
{
    Task<List<OrderReportDto>> GetOrdersAsync(ReportFilterDto filter);
    Task<SalesReportDto> GetSalesAsync(ReportFilterDto filter);
    Task<List<DishReportDto>> GetDishesAsync(ReportFilterDto filter);
    Task<List<EmployeeReportDto>> GetEmployeesAsync(ReportFilterDto filter);
    Task<List<TableReportDto>> GetTablesAsync(ReportFilterDto filter);
    Task<List<DailyReportDto>> GetDailyAsync(ReportFilterDto filter);
}
