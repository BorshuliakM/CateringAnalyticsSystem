using CateringAnalyticsSystem.Data;
using CateringAnalyticsSystem.DTOs;
using CateringAnalyticsSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringAnalyticsSystem.Services;

public class DiningTableService : IDiningTableService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Free",
        "Occupied",
        "Reserved"
    };

    private readonly ApplicationDbContext _context;

    public DiningTableService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DiningTableDto>> GetAllAsync()
    {
        return await _context.DiningTables
            .AsNoTracking()
            .OrderBy(table => table.Number)
            .Select(table => ToDto(table))
            .ToListAsync();
    }

    public async Task<DiningTableDto?> GetByIdAsync(int id)
    {
        var table = await _context.DiningTables.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        return table is null ? null : ToDto(table);
    }

    public async Task<List<DiningTableDto>> GetByStatusAsync(string status)
    {
        return await _context.DiningTables
            .AsNoTracking()
            .Where(table => table.Status == status)
            .OrderBy(table => table.Number)
            .Select(table => ToDto(table))
            .ToListAsync();
    }

    public async Task<DiningTableDto> CreateAsync(CreateDiningTableDto dto)
    {
        Validate(dto.Number, dto.SeatsCount, dto.Status);
        await EnsureNumberIsUniqueAsync(dto.Number);

        var table = new DiningTable
        {
            Number = dto.Number,
            SeatsCount = dto.SeatsCount,
            Status = NormalizeStatus(dto.Status)
        };

        await _context.DiningTables.AddAsync(table);
        await _context.SaveChangesAsync();
        return ToDto(table);
    }

    public async Task<bool> UpdateAsync(int id, UpdateDiningTableDto dto)
    {
        Validate(dto.Number, dto.SeatsCount, dto.Status);

        var table = await _context.DiningTables.FindAsync(id);
        if (table is null)
        {
            return false;
        }

        var numberExists = await _context.DiningTables.AnyAsync(t => t.Id != id && t.Number == dto.Number);
        if (numberExists)
        {
            throw new ArgumentException("Dining table with this number already exists.");
        }

        table.Number = dto.Number;
        table.SeatsCount = dto.SeatsCount;
        table.Status = NormalizeStatus(dto.Status);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangeStatusAsync(int id, string status)
    {
        ValidateStatus(status);

        var table = await _context.DiningTables.FindAsync(id);
        if (table is null)
        {
            return false;
        }

        table.Status = NormalizeStatus(status);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var table = await _context.DiningTables.FindAsync(id);
        if (table is null)
        {
            return false;
        }

        var hasOrders = await _context.Orders.AnyAsync(order => order.DiningTableId == id);
        if (hasOrders)
        {
            throw new ArgumentException("Dining table cannot be deleted because it has orders.");
        }

        _context.DiningTables.Remove(table);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task EnsureNumberIsUniqueAsync(int number)
    {
        var exists = await _context.DiningTables.AnyAsync(table => table.Number == number);
        if (exists)
        {
            throw new ArgumentException("Dining table with this number already exists.");
        }
    }

    private static void Validate(int number, int seatsCount, string status)
    {
        if (number <= 0)
        {
            throw new ArgumentException("Dining table number must be greater than 0.");
        }

        if (seatsCount <= 0)
        {
            throw new ArgumentException("Seats count must be greater than 0.");
        }

        ValidateStatus(status);
    }

    private static void ValidateStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status) || !AllowedStatuses.Contains(status))
        {
            throw new ArgumentException("Dining table status must be Free, Occupied or Reserved.");
        }
    }

    private static string NormalizeStatus(string status)
    {
        return AllowedStatuses.First(allowed => string.Equals(allowed, status, StringComparison.OrdinalIgnoreCase));
    }

    private static DiningTableDto ToDto(DiningTable table)
    {
        return new DiningTableDto
        {
            Id = table.Id,
            Number = table.Number,
            SeatsCount = table.SeatsCount,
            Status = table.Status
        };
    }
}
