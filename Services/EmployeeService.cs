using CateringAnalyticsSystem.Models;
using CateringAnalyticsSystem.Repositories;

namespace CateringAnalyticsSystem.Services;

public class EmployeeService : CrudService<Employee>, IEmployeeService
{
    public EmployeeService(IGenericRepository<Employee> repository) : base(repository)
    {
    }

    public Task<bool> UpdateAsync(int id, Employee employee) => UpdateAsync(id, employee, (entity, entityId) => entity.Id = entityId);

    protected override void Validate(Employee employee)
    {
        if (string.IsNullOrWhiteSpace(employee.FullName))
        {
            throw new ArgumentException("Employee full name cannot be empty.");
        }
    }
}
