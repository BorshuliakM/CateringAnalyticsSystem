using CateringAnalyticsSystem.Data;
using CateringAnalyticsSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringAnalyticsSystem.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync() || await context.DiningTables.AnyAsync())
        {
            return;
        }

        var categories = new List<Category>
        {
            new() { Name = "Сніданки", Description = "Страви для ранкового меню" },
            new() { Name = "Перші страви", Description = "Супи та бульйони" },
            new() { Name = "Основні страви", Description = "Гарячі страви ресторану" },
            new() { Name = "Салати", Description = "Легкі та сезонні салати" },
            new() { Name = "Десерти", Description = "Солодкі страви" },
            new() { Name = "Напої", Description = "Гарячі та холодні напої" }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        var dishes = new List<Dish>
        {
            new() { Name = "Омлет із сиром", Description = "Омлет з твердим сиром та зеленню", Price = 95, CategoryId = categories[0].Id },
            new() { Name = "Сирники зі сметаною", Description = "Домашні сирники з ягідним соусом", Price = 120, CategoryId = categories[0].Id },
            new() { Name = "Борщ український", Description = "Борщ зі сметаною та пампушками", Price = 135, CategoryId = categories[1].Id },
            new() { Name = "Курячий бульйон", Description = "Легкий бульйон з локшиною", Price = 105, CategoryId = categories[1].Id },
            new() { Name = "Котлета по-київськи", Description = "Класична котлета з картопляним пюре", Price = 230, CategoryId = categories[2].Id },
            new() { Name = "Паста карбонара", Description = "Паста з беконом, вершками та пармезаном", Price = 210, CategoryId = categories[2].Id },
            new() { Name = "Стейк курячий", Description = "Куряче філе на грилі з овочами", Price = 195, CategoryId = categories[2].Id },
            new() { Name = "Цезар з куркою", Description = "Салат з куркою, сухариками та соусом цезар", Price = 175, CategoryId = categories[3].Id },
            new() { Name = "Грецький салат", Description = "Овочі, фета та оливки", Price = 150, CategoryId = categories[3].Id },
            new() { Name = "Наполеон", Description = "Листковий десерт із заварним кремом", Price = 110, CategoryId = categories[4].Id },
            new() { Name = "Чизкейк", Description = "Сирний десерт з ягідним топінгом", Price = 130, CategoryId = categories[4].Id },
            new() { Name = "Еспресо", Description = "Класична кава", Price = 45, CategoryId = categories[5].Id },
            new() { Name = "Капучино", Description = "Кава з молочною пінкою", Price = 65, CategoryId = categories[5].Id },
            new() { Name = "Лимонад", Description = "Домашній лимонад", Price = 75, CategoryId = categories[5].Id },
            new() { Name = "Узвар", Description = "Традиційний напій із сухофруктів", Price = 55, CategoryId = categories[5].Id }
        };

        await context.Dishes.AddRangeAsync(dishes);

        var diningTables = new List<DiningTable>
        {
            new() { Number = 1, SeatsCount = 2, Status = "Free" },
            new() { Number = 2, SeatsCount = 2, Status = "Free" },
            new() { Number = 3, SeatsCount = 4, Status = "Free" },
            new() { Number = 4, SeatsCount = 4, Status = "Free" },
            new() { Number = 5, SeatsCount = 6, Status = "Free" },
            new() { Number = 6, SeatsCount = 6, Status = "Reserved" },
            new() { Number = 7, SeatsCount = 8, Status = "Free" },
            new() { Number = 8, SeatsCount = 4, Status = "Cleaning" }
        };

        var employees = new List<Employee>
        {
            new() { FullName = "Наталія Романюк", Position = "Офіціант", Phone = "+380661010101", Email = "nataliia.romaniuk@restaurant.local" },
            new() { FullName = "Сергій Литвин", Position = "Офіціант", Phone = "+380672020202", Email = "serhii.lytvyn@restaurant.local" },
            new() { FullName = "Марія Ткаченко", Position = "Адміністратор", Phone = "+380633030303", Email = "mariia.tkachenko@restaurant.local" }
        };

        await context.DiningTables.AddRangeAsync(diningTables);
        await context.Employees.AddRangeAsync(employees);
        await context.SaveChangesAsync();

        var orders = new List<Order>
        {
            CreateOrder(diningTables[0].Id, employees[0].Id, DateTime.UtcNow.AddDays(-5), "Completed", dishes[2], 1, dishes[4], 1, dishes[11], 2),
            CreateOrder(diningTables[1].Id, employees[1].Id, DateTime.UtcNow.AddDays(-4), "Completed", dishes[7], 2, dishes[12], 2),
            CreateOrder(diningTables[2].Id, employees[0].Id, DateTime.UtcNow.AddDays(-3), "Completed", dishes[5], 1, dishes[9], 1, dishes[13], 1),
            CreateOrder(diningTables[3].Id, employees[2].Id, DateTime.UtcNow.AddDays(-2), "InProgress", dishes[0], 2, dishes[1], 2, dishes[12], 2),
            CreateOrder(diningTables[4].Id, employees[1].Id, DateTime.UtcNow.AddDays(-1), "New", dishes[4], 1, dishes[8], 1, dishes[14], 2),
            CreateOrder(diningTables[2].Id, employees[2].Id, DateTime.UtcNow, "Completed", dishes[6], 2, dishes[10], 2),
            CreateOrder(diningTables[6].Id, employees[0].Id, DateTime.UtcNow, "Cancelled", dishes[3], 1, dishes[13], 2)
        };

        diningTables[3].Status = "Occupied";
        diningTables[4].Status = "Occupied";
        diningTables[7].Status = "Cleaning";

        await context.Orders.AddRangeAsync(orders);

        await context.Users.AddRangeAsync(
            new User { Username = "admin", PasswordHash = "demo-admin-hash", Role = "Admin" },
            new User { Username = "manager", PasswordHash = "demo-manager-hash", Role = "Manager" });

        await context.SaveChangesAsync();
    }

    private static Order CreateOrder(int diningTableId, int employeeId, DateTime date, string status, params object[] dishQuantityPairs)
    {
        var order = new Order
        {
            DiningTableId = diningTableId,
            EmployeeId = employeeId,
            OrderDate = date,
            Status = status
        };

        for (var i = 0; i < dishQuantityPairs.Length; i += 2)
        {
            var dish = (Dish)dishQuantityPairs[i];
            var quantity = (int)dishQuantityPairs[i + 1];

            order.OrderItems.Add(new OrderItem
            {
                DishId = dish.Id,
                Quantity = quantity,
                Price = dish.Price
            });
        }

        order.TotalAmount = order.OrderItems.Sum(item => item.Price * item.Quantity);
        return order;
    }
}
