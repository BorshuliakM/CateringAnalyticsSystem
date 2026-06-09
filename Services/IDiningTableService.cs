using CateringAnalyticsSystem.DTOs;

namespace CateringAnalyticsSystem.Services;

public interface IDiningTableService
{
    Task<List<DiningTableDto>> GetAllAsync();
    Task<DiningTableDto?> GetByIdAsync(int id);
    Task<List<DiningTableDto>> GetByStatusAsync(string status);
    Task<DiningTableDto> CreateAsync(CreateDiningTableDto dto);
    Task<bool> UpdateAsync(int id, UpdateDiningTableDto dto);
    Task<bool> ChangeStatusAsync(int id, string status);
    Task<bool> DeleteAsync(int id);
}
