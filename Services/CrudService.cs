using CateringAnalyticsSystem.Repositories;

namespace CateringAnalyticsSystem.Services;

public abstract class CrudService<T> where T : class
{
    protected readonly IGenericRepository<T> Repository;

    protected CrudService(IGenericRepository<T> repository)
    {
        Repository = repository;
    }

    public Task<List<T>> GetAllAsync() => Repository.GetAllAsync();

    public Task<T?> GetByIdAsync(int id) => Repository.GetByIdAsync(id);

    public async Task<T> CreateAsync(T entity)
    {
        Validate(entity);
        await Repository.AddAsync(entity);
        await Repository.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> UpdateAsync(int id, T entity, Action<T, int> setId)
    {
        var existing = await Repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        setId(entity, id);
        Validate(entity);
        Repository.Update(entity);
        await Repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await Repository.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        Repository.Delete(entity);
        await Repository.SaveChangesAsync();
        return true;
    }

    protected virtual void Validate(T entity)
    {
    }
}
