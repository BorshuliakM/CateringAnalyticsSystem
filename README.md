# Інформаційна аналітична система закладу громадського харчування

Навчальний ASP.NET Core Web API проєкт для дипломної роботи на тему: **«Розробка інформаційної аналітичної системи закладу громадського харчування»**.

Система автоматизує облік меню, категорій страв, працівників, столиків, замовлень і позицій замовлень, а також формує аналітичні JSON-звіти для оцінки роботи закладу.

## Технології

- ASP.NET Core Web API
- C#
- Entity Framework Core
- Microsoft SQL Server / LocalDB
- Swagger / OpenAPI
- JSON
- HTML, CSS, JavaScript для frontend-панелі
- Багатошарова структура Controller-Service-Repository

## Основна логіка

У типовому закладі громадського харчування не завжди збирають персональні дані гостей, тому замовлення прив'язуються не до клієнта, а до столика.

Столик має статус:

- `Free`
- `Occupied`
- `Reserved`
- `Cleaning`

Під час створення активного замовлення столик автоматично переходить у статус `Occupied`. Якщо замовлення завершується зі статусом `Completed`, столик переходить у `Cleaning`. Якщо замовлення скасоване, столик переходить у `Free`.

## Структура проєкту

```text
CateringAnalyticsSystem/
  Controllers/       API-контролери
  Models/            EF Core моделі сутностей
  DTOs/              DTO для створення, оновлення та звітів
  Data/              ApplicationDbContext
  Services/          Бізнес-логіка
  Repositories/      Репозиторії для доступу до даних
  Seed/              Початкові тестові дані
  Migrations/        EF Core міграції
  DatabaseScripts/   SQL-структура бази та seed-дані
  wwwroot/           Frontend-панель адміністратора
```



## База даних

Підключення за замовчуванням:

```json
"Server=(localdb)\\MSSQLLocalDB;Database=CateringAnalyticsTablesDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Під час першого запуску застосунок створює базу через `EnsureCreatedAsync()` і додає тестові дані з `SeedData`.

Якщо на комп'ютері вже була створена стара версія бази з попередньою структурою, її потрібно видалити або застосувати міграцію.

## Команди міграції

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add ReplaceCustomersWithDiningTables
dotnet ef database update
```

У проєкті також є ручний SQL-скрипт:

```text
DatabaseScripts/schema-and-seed.sql
```

## Основні endpoint

- `GET /api/diningtables`
- `GET /api/diningtables/{id}`
- `POST /api/diningtables`
- `PUT /api/diningtables/{id}`
- `DELETE /api/diningtables/{id}`
- `PATCH /api/diningtables/{id}/status`
- `GET /api/diningtables/by-status/{status}`
- `GET /api/employees`
- `POST /api/employees`
- `GET /api/categories`
- `POST /api/categories`
- `GET /api/dishes`
- `POST /api/dishes`
- `PUT /api/dishes/{id}`
- `GET /api/orders`
- `GET /api/orders/{id}`
- `POST /api/orders`
- `PATCH /api/orders/{id}/status`
- `DELETE /api/orders/{id}`

## Приклад створення столика

```json
{
  "number": 9,
  "seatsCount": 4,
  "status": "Free"
}
```

## Приклад створення замовлення

```json
{
  "diningTableId": 1,
  "employeeId": 1,
  "items": [
    {
      "dishId": 3,
      "quantity": 2
    },
    {
      "dishId": 12,
      "quantity": 1
    }
  ]
}
```

`TotalAmount` розраховується автоматично на основі ціни страв і кількості. Після створення замовлення столик отримує статус `Occupied`.

## Фільтрація замовлень

```text
GET /api/orders?from=2026-06-01&to=2026-06-30
GET /api/orders?diningTableId=1
GET /api/orders?employeeId=2
GET /api/orders?status=Completed
```

Параметри можна комбінувати.

## Аналітичні endpoint

- `GET /api/analytics/summary`  
  Загальна кількість замовлень, сума продажів, середній чек.

- `GET /api/analytics/sales-by-period?from=2026-06-01&to=2026-06-30`  
  Продажі за період.

- `GET /api/analytics/popular-dishes`  
  Найпопулярніші страви за кількістю проданих позицій.

- `GET /api/analytics/sales-by-employee`  
  Сума продажів і кількість замовлень по працівниках.

- `GET /api/analytics/orders-count-by-day`  
  Кількість замовлень по днях.

- `GET /api/analytics/revenue-by-category`  
  Виручка за категоріями страв.

- `GET /api/analytics/revenue-by-table`  
  Кількість замовлень і сума продажів по кожному столику.

- `GET /api/analytics/table-occupancy`  
  Поточний статус столика та кількість замовлень по ньому.

## Валідація

- ціна страви має бути більшою за 0;
- кількість позицій у замовленні має бути більшою за 0;
- ім'я працівника, назва страви та категорії не можуть бути порожніми;
- номер столика та кількість місць мають бути більшими за 0;
- замовлення не може бути створене без позицій;
- замовлення не можна створити для неіснуючого або зайнятого столика;
- замовлення не можна створити для неіснуючого працівника або страви.
