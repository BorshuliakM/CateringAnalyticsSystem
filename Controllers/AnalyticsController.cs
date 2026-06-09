using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace CateringAnalyticsSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<AnalyticsSummaryDto>> GetSummary() => Ok(await _analyticsService.GetSummaryAsync());

    [HttpGet("sales-by-period")]
    public async Task<ActionResult<SalesByPeriodDto>> GetSalesByPeriod([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        if (from > to)
        {
            return BadRequest(new { message = "Parameter 'from' cannot be later than parameter 'to'." });
        }

        return Ok(await _analyticsService.GetSalesByPeriodAsync(from, to));
    }

    [HttpGet("popular-dishes")]
    public async Task<ActionResult<List<PopularDishDto>>> GetPopularDishes() => Ok(await _analyticsService.GetPopularDishesAsync());

    [HttpGet("sales-by-employee")]
    public async Task<ActionResult<List<SalesByEmployeeDto>>> GetSalesByEmployee() => Ok(await _analyticsService.GetSalesByEmployeeAsync());

    [HttpGet("orders-count-by-day")]
    public async Task<ActionResult<List<OrdersCountByDayDto>>> GetOrdersCountByDay() => Ok(await _analyticsService.GetOrdersCountByDayAsync());

    [HttpGet("revenue-by-category")]
    public async Task<ActionResult<List<RevenueByCategoryDto>>> GetRevenueByCategory() => Ok(await _analyticsService.GetRevenueByCategoryAsync());

    [HttpGet("revenue-by-table")]
    public async Task<ActionResult<List<RevenueByTableDto>>> GetRevenueByTable() => Ok(await _analyticsService.GetRevenueByTableAsync());

    [HttpGet("table-occupancy")]
    public async Task<ActionResult<List<TableOccupancyDto>>> GetTableOccupancy() => Ok(await _analyticsService.GetTableOccupancyAsync());
}
