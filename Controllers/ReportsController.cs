using System.Globalization;
using System.Text;
using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CateringAnalyticsSystem.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;

    public ReportsController(IReportsService reportsService)
    {
        _reportsService = reportsService;
    }

    [HttpGet("orders")]
    public async Task<ActionResult<List<OrderReportDto>>> GetOrders([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateDateRange(filter);
        return validation ?? Ok(await _reportsService.GetOrdersAsync(filter));
    }

    [HttpGet("sales")]
    public async Task<ActionResult<SalesReportDto>> GetSales([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateDateRange(filter);
        return validation ?? Ok(await _reportsService.GetSalesAsync(filter));
    }

    [HttpGet("dishes")]
    public async Task<ActionResult<List<DishReportDto>>> GetDishes([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateDateRange(filter);
        return validation ?? Ok(await _reportsService.GetDishesAsync(filter));
    }

    [HttpGet("employees")]
    public async Task<ActionResult<List<EmployeeReportDto>>> GetEmployees([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateDateRange(filter);
        return validation ?? Ok(await _reportsService.GetEmployeesAsync(filter));
    }

    [HttpGet("tables")]
    public async Task<ActionResult<List<TableReportDto>>> GetTables([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateDateRange(filter);
        return validation ?? Ok(await _reportsService.GetTablesAsync(filter));
    }

    [HttpGet("daily")]
    public async Task<ActionResult<List<DailyReportDto>>> GetDaily([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateDateRange(filter);
        return validation ?? Ok(await _reportsService.GetDailyAsync(filter));
    }

    [HttpGet("orders/export")]
    public async Task<IActionResult> ExportOrders([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateExport(filter);
        if (validation is not null)
        {
            return validation;
        }

        var rows = await _reportsService.GetOrdersAsync(filter);
        var csv = BuildCsv(
            "OrderId;OrderDate;TableNumber;EmployeeName;Status;TotalAmount;ItemsCount",
            rows.Select(row => string.Join(';',
                row.OrderId,
                row.OrderDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                row.DiningTableNumber,
                Escape(row.EmployeeName),
                row.Status,
                Money(row.TotalAmount),
                row.ItemsCount)));

        return CsvFile(csv, "orders_report", filter);
    }

    [HttpGet("sales/export")]
    public async Task<IActionResult> ExportSales([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateExport(filter);
        if (validation is not null)
        {
            return validation;
        }

        var row = await _reportsService.GetSalesAsync(filter);
        var csv = BuildCsv(
            "TotalOrders;CompletedOrders;CancelledOrders;ActiveOrders;TotalRevenue;AverageCheck",
            new[]
            {
                string.Join(';', row.TotalOrders, row.CompletedOrders, row.CancelledOrders, row.ActiveOrders, Money(row.TotalRevenue), Money(row.AverageCheck))
            });

        return CsvFile(csv, "sales_report", filter);
    }

    [HttpGet("dishes/export")]
    public async Task<IActionResult> ExportDishes([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateExport(filter);
        if (validation is not null)
        {
            return validation;
        }

        var rows = await _reportsService.GetDishesAsync(filter);
        var csv = BuildCsv(
            "DishId;DishName;CategoryName;QuantitySold;TotalRevenue",
            rows.Select(row => string.Join(';', row.DishId, Escape(row.DishName), Escape(row.CategoryName), row.QuantitySold, Money(row.TotalRevenue))));

        return CsvFile(csv, "dishes_report", filter);
    }

    [HttpGet("employees/export")]
    public async Task<IActionResult> ExportEmployees([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateExport(filter);
        if (validation is not null)
        {
            return validation;
        }

        var rows = await _reportsService.GetEmployeesAsync(filter);
        var csv = BuildCsv(
            "EmployeeId;EmployeeName;OrdersCount;TotalRevenue;AverageCheck",
            rows.Select(row => string.Join(';', row.EmployeeId, Escape(row.EmployeeName), row.OrdersCount, Money(row.TotalRevenue), Money(row.AverageCheck))));

        return CsvFile(csv, "employees_report", filter);
    }

    [HttpGet("tables/export")]
    public async Task<IActionResult> ExportTables([FromQuery] ReportFilterDto filter)
    {
        var validation = ValidateExport(filter);
        if (validation is not null)
        {
            return validation;
        }

        var rows = await _reportsService.GetTablesAsync(filter);
        var csv = BuildCsv(
            "DiningTableId;TableNumber;OrdersCount;TotalRevenue;AverageCheck;CurrentStatus",
            rows.Select(row => string.Join(';', row.DiningTableId, row.TableNumber, row.OrdersCount, Money(row.TotalRevenue), Money(row.AverageCheck), row.CurrentStatus)));

        return CsvFile(csv, "tables_report", filter);
    }

    private ActionResult? ValidateDateRange(ReportFilterDto filter)
    {
        if (filter.From == default || filter.To == default)
        {
            return BadRequest(new { message = "Parameters 'from' and 'to' are required." });
        }

        if (filter.From > filter.To)
        {
            return BadRequest(new { message = "Parameter 'from' cannot be later than parameter 'to'." });
        }

        return null;
    }

    private ActionResult? ValidateExport(ReportFilterDto filter)
    {
        var validation = ValidateDateRange(filter);
        if (validation is not null)
        {
            return validation;
        }

        if (!string.IsNullOrWhiteSpace(filter.Format) && !string.Equals(filter.Format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Only CSV export is supported." });
        }

        return null;
    }

    private FileContentResult CsvFile(string csv, string reportType, ReportFilterDto filter)
    {
        var bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(csv);
        var fileName = $"{reportType}_{filter.From:yyyy-MM-dd}_{filter.To:yyyy-MM-dd}.csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    private static string BuildCsv(string header, IEnumerable<string> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine(header);
        foreach (var row in rows)
        {
            builder.AppendLine(row);
        }

        return builder.ToString();
    }

    private static string Escape(string value)
    {
        if (!value.Contains(';') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static string Money(decimal value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }
}
