using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Models;
using CateringAnalyticsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace CateringAnalyticsSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DishesController : ControllerBase
{
    private readonly IDishService _dishService;

    public DishesController(IDishService dishService)
    {
        _dishService = dishService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Dish>>> GetAll() => Ok(await _dishService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Dish>> GetById(int id)
    {
        var dish = await _dishService.GetByIdAsync(id);
        return dish is null ? NotFound() : Ok(dish);
    }

    [HttpPost]
    public async Task<ActionResult<Dish>> Create(CreateDishDto dto)
    {
        try
        {
            var created = await _dishService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateDishDto dto)
    {
        try
        {
            var updated = await _dishService.UpdateAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _dishService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
