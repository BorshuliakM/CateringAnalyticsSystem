using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CateringAnalyticsSystem.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Waiter")]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderDetailsDto>>> GetAll(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? diningTableId,
        [FromQuery] int? employeeId,
        [FromQuery] string? status)
    {
        if (!User.IsAdmin())
        {
            employeeId = User.GetEmployeeId();
        }

        return Ok(await _orderService.GetAllAsync(from, to, diningTableId, employeeId, status));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDetailsDto>> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order is not null && !User.IsAdmin() && order.EmployeeId != User.GetEmployeeId())
        {
            return Forbid();
        }

        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDetailsDto>> Create(CreateOrderDto dto)
    {
        try
        {
            var created = await _orderService.CreateAsync(dto, User.IsAdmin(), User.GetEmployeeId());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeOrderStatusRequest request)
    {
        try
        {
            var updated = await _orderService.ChangeStatusAsync(id, request.Status, User.IsAdmin(), User.GetEmployeeId());
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _orderService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

public class ChangeOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
