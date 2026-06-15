using CateringAnalyticsSystem.DTOs;

namespace CateringAnalyticsSystem.Services;

public interface IOrderService
{
    Task<List<OrderDetailsDto>> GetAllAsync(DateTime? from, DateTime? to, int? diningTableId, int? employeeId, string? status);
    Task<OrderDetailsDto?> GetByIdAsync(int id);
    Task<OrderDetailsDto> CreateAsync(CreateOrderDto dto, bool isAdmin, int? currentEmployeeId);
    Task<bool> ChangeStatusAsync(int id, string status, bool isAdmin, int? currentEmployeeId);
    Task<bool> DeleteAsync(int id);
}
