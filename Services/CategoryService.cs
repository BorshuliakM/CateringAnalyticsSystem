using CateringAnalyticsSystem.Models;
using CateringAnalyticsSystem.Repositories;

namespace CateringAnalyticsSystem.Services;

public class CategoryService : CrudService<Category>, ICategoryService
{
    public CategoryService(IGenericRepository<Category> repository) : base(repository)
    {
    }

    public Task<bool> UpdateAsync(int id, Category category) => UpdateAsync(id, category, (entity, entityId) => entity.Id = entityId);

    protected override void Validate(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            throw new ArgumentException("Category name cannot be empty.");
        }
    }
}
