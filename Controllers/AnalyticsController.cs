using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CateringAnalyticsSystem.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<AnalyticsSummaryDto>> GetSummary([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateDateRange(filter);
        return validation ?? Ok(await _analyticsService.GetSummaryAsync(filter));
    }

    [HttpGet("sales-by-period")]
    public async Task<ActionResult<List<SalesByPeriodDto>>> GetSalesByPeriod([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateDateRange(filter);
        return validation ?? Ok(await _analyticsService.GetSalesByPeriodAsync(filter));
    }

    [HttpGet("popular-dishes")]
    public async Task<ActionResult<List<PopularDishDto>>> GetPopularDishes([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateDateRange(filter);
        return validation ?? Ok(await _analyticsService.GetPopularDishesAsync(filter));
    }

    [HttpGet("sales-by-employee")]
    public async Task<ActionResult<List<SalesByEmployeeDto>>> GetSalesByEmployee([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateDateRange(filter);
        return validation ?? Ok(await _analyticsService.GetSalesByEmployeeAsync(filter));
    }

    [HttpGet("orders-count-by-day")]
    public async Task<ActionResult<List<OrdersCountByDayDto>>> GetOrdersCountByDay() => Ok(await _analyticsService.GetOrdersCountByDayAsync());

    [HttpGet("revenue-by-category")]
    public async Task<ActionResult<List<RevenueByCategoryDto>>> GetRevenueByCategory([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateDateRange(filter);
        return validation ?? Ok(await _analyticsService.GetRevenueByCategoryAsync(filter));
    }

    [HttpGet("revenue-by-table")]
    public async Task<ActionResult<List<RevenueByTableDto>>> GetRevenueByTable([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateDateRange(filter);
        return validation ?? Ok(await _analyticsService.GetRevenueByTableAsync(filter));
    }

    [HttpGet("table-occupancy")]
    public async Task<ActionResult<List<TableOccupancyDto>>> GetTableOccupancy([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateDateRange(filter);
        return validation ?? Ok(await _analyticsService.GetTableOccupancyAsync(filter));
    }

    private ActionResult? ValidateDateRange(ReportFilterDto filter)
    {
        if (filter.From != default && filter.To != default && filter.From > filter.To)
        {
            return BadRequest(new { message = "Parameter 'from' cannot be later than parameter 'to'." });
        }

        return null;
    }
}
