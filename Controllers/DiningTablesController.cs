using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CateringAnalyticsSystem.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Waiter")]
[Route("api/[controller]")]
public class DiningTablesController : ControllerBase
{
    private readonly IDiningTableService _diningTableService;

    public DiningTablesController(IDiningTableService diningTableService)
    {
        _diningTableService = diningTableService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DiningTableDto>>> GetAll() => Ok(await _diningTableService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DiningTableDto>> GetById(int id)
    {
        var table = await _diningTableService.GetByIdAsync(id);
        return table is null ? NotFound() : Ok(table);
    }

    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<List<DiningTableDto>>> GetByStatus(string status)
    {
        return Ok(await _diningTableService.GetByStatusAsync(status));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<DiningTableDto>> Create(CreateDiningTableDto dto)
    {
        try
        {
            var created = await _diningTableService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateDiningTableDto dto)
    {
        try
        {
            var updated = await _diningTableService.UpdateAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, ChangeDiningTableStatusRequest request)
    {
        try
        {
            var updated = await _diningTableService.ChangeStatusAsync(id, request.Status);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _diningTableService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class ChangeDiningTableStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
