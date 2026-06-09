using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Models;

namespace CateringAnalyticsSystem.Services;

public interface IDishService
{
    Task<List<Dish>> GetAllAsync();
    Task<Dish?> GetByIdAsync(int id);
    Task<Dish> CreateAsync(CreateDishDto dto);
    Task<bool> UpdateAsync(int id, UpdateDishDto dto);
    Task<bool> DeleteAsync(int id);
}
