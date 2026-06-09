using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Models;
using CateringAnalyticsSystem.Repositories;

namespace CateringAnalyticsSystem.Services;

public class DishService : IDishService
{
    private readonly IGenericRepository<Dish> _dishRepository;
    private readonly IGenericRepository<Category> _categoryRepository;

    public DishService(IGenericRepository<Dish> dishRepository, IGenericRepository<Category> categoryRepository)
    {
        _dishRepository = dishRepository;
        _categoryRepository = categoryRepository;
    }

    public Task<List<Dish>> GetAllAsync() => _dishRepository.GetAllAsync();

    public Task<Dish?> GetByIdAsync(int id) => _dishRepository.GetByIdAsync(id);

    public async Task<Dish> CreateAsync(CreateDishDto dto)
    {
        await ValidateDtoAsync(dto.Name, dto.Price, dto.CategoryId);

        var dish = new Dish
        {
            Name = dto.Name.Trim(),
            Description = dto.Description,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            IsAvailable = dto.IsAvailable
        };

        await _dishRepository.AddAsync(dish);
        await _dishRepository.SaveChangesAsync();
        return dish;
    }

    public async Task<bool> UpdateAsync(int id, UpdateDishDto dto)
    {
        var dish = await _dishRepository.GetByIdAsync(id);
        if (dish is null)
        {
            return false;
        }

        await ValidateDtoAsync(dto.Name, dto.Price, dto.CategoryId);

        dish.Name = dto.Name.Trim();
        dish.Description = dto.Description;
        dish.Price = dto.Price;
        dish.CategoryId = dto.CategoryId;
        dish.IsAvailable = dto.IsAvailable;

        _dishRepository.Update(dish);
        await _dishRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var dish = await _dishRepository.GetByIdAsync(id);
        if (dish is null)
        {
            return false;
        }

        _dishRepository.Delete(dish);
        await _dishRepository.SaveChangesAsync();
        return true;
    }

    private async Task ValidateDtoAsync(string name, decimal price, int categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Dish name cannot be empty.");
        }

        if (price <= 0)
        {
            throw new ArgumentException("Dish price must be greater than 0.");
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category is null)
        {
            throw new ArgumentException("Selected category does not exist.");
        }
    }
}
