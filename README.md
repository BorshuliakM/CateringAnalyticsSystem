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

Під час створення активного замовлення столик автоматично переходить у статус `Occupied`. Якщо замовлення завершується зі статусом `Completed`, столик переходить у `Free`. Якщо замовлення скасоване, столик також переходить у `Free`, якщо для нього немає інших активних замовлень.

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

JWT-налаштування в `appsettings.json`:

```json
"Jwt": {
  "Key": "Diploma_CateringAnalyticsSystem_Development_Key_Change_Me_12345",
  "Issuer": "CateringAnalyticsSystem",
  "Audience": "CateringAnalyticsSystemUsers",
  "ExpireMinutes": 120
}
```

Під час першого запуску застосунок створює базу через `EnsureCreatedAsync()` і додає тестові дані з `SeedData`. Для вже створеної навчальної бази SeedData також додає відсутнє поле `Users.EmployeeId`, індекс username та зовнішній ключ до `Employees`, щоб нова авторизація працювала без ручного видалення бази.

Якщо на комп'ютері вже була створена стара версія бази з попередньою структурою, її потрібно видалити або застосувати міграцію.

## Команди міграції

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add ReplaceCustomersWithDiningTables
dotnet ef database update
```

Для нової міграції після додавання авторизації можна використати:

```powershell
dotnet ef migrations add AddJwtAuthorizationAndReports
dotnet ef database update
```

У проєкті також є ручний SQL-скрипт:

```text
DatabaseScripts/schema-and-seed.sql
```

## Основні endpoint

- `POST /api/auth/login`
- `POST /api/auth/register`
- `GET /api/auth/me`
- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users`
- `PUT /api/users/{id}`
- `DELETE /api/users/{id}`
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

## Авторизація

Система використовує JWT Bearer authentication і ролі:

| Роль | Доступ |
| --- | --- |
| `Admin` | Повний доступ: користувачі, працівники, столики, категорії, страви, замовлення, аналітика, звіти, видалення там, де воно підтримується. |
| `Waiter` | Тільки сторінка/функціональність замовлень: перегляд власних замовлень, створення замовлень, вибір столика і страв для замовлення, зміна статусу власних замовлень. Немає доступу до аналітики, звітів, експорту, користувачів, працівників і керування довідниками. |

Користувачі за замовчуванням:

| Username | Password | Role |
| --- | --- | --- |
| `admin` | `Admin123!` | `Admin` |
| `waiter` | `Waiter123!` | `Waiter` |

Паролі зберігаються у вигляді PBKDF2-хешів, не у відкритому тексті.

Приклад login-запиту:

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "username": "admin",
  "password": "Admin123!"
}
```

Приклад відповіді:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "username": "admin",
  "role": "Admin",
  "employeeId": null
}
```

Приклад створення користувача:

```json
{
  "username": "waiter2",
  "password": "Waiter123!",
  "role": "Waiter",
  "employeeId": 2
}
```

### JWT у Swagger

1. Виконати `POST /api/auth/login`.
2. Скопіювати значення `token`.
3. Натиснути `Authorize` у Swagger.
4. Вставити токен у Bearer authorizer.
5. Тестувати захищені endpoint.

Frontend має сторінку `/login.html`. Після входу JWT зберігається в `localStorage`, додається до API-запитів як `Authorization: Bearer <token>`, а меню приховується відповідно до ролі.

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

## Детальні звіти

Звіти доступні через `ReportsController`. Усі звіти та всі export endpoint доступні тільки ролі `Admin`. Якщо `Waiter` звертається до `/api/reports/*` або `/api/reports/*/export`, API повертає `403 Forbidden`.

- `GET /api/reports/orders?from=2026-06-01&to=2026-06-30`
- `GET /api/reports/orders?from=2026-06-01&to=2026-06-30&employeeId=1&status=Completed&minTotal=100&maxTotal=1000`
- `GET /api/reports/sales?from=2026-06-01&to=2026-06-30`
- `GET /api/reports/dishes?from=2026-06-01&to=2026-06-30&categoryId=2`
- `GET /api/reports/employees?from=2026-06-01&to=2026-06-30`
- `GET /api/reports/tables?from=2026-06-01&to=2026-06-30`
- `GET /api/reports/daily?from=2026-06-01&to=2026-06-30`
- `GET /api/reports/orders/export?from=2026-06-01&to=2026-06-30&format=csv`
- `GET /api/reports/dishes/export?from=2026-06-01&to=2026-06-30&categoryId=2&format=csv`
- `GET /api/reports/tables/export?from=2026-06-01&to=2026-06-30&format=csv`

Для виручки у звітах враховуються переважно замовлення зі статусом `Completed`. Скасовані замовлення (`Cancelled`) не додаються до revenue, але окремо рахуються у `SalesReportDto`.

CSV export повертає UTF-8 файл із BOM, заголовком, розділювачем `;` і назвою виду `orders_report_2026-06-01_2026-06-30.csv`.

## Фільтрована аналітика

Усі endpoint `/api/analytics/*` доступні тільки `Admin`.

Приклади:

```text
GET /api/analytics/summary?from=2026-01-01&to=2026-01-31
GET /api/analytics/summary?from=2026-01-01&to=2026-01-31&employeeId=1&diningTableId=2
GET /api/analytics/sales-by-period?from=2026-01-01&to=2026-01-31&groupBy=day
GET /api/analytics/sales-by-period?from=2026-01-01&to=2026-12-31&groupBy=month
GET /api/analytics/popular-dishes?from=2026-01-01&to=2026-01-31&categoryId=2&limit=10
GET /api/analytics/revenue-by-category?from=2026-01-01&to=2026-01-31
GET /api/analytics/revenue-by-table?from=2026-01-01&to=2026-01-31
GET /api/analytics/table-occupancy?from=2026-01-01&to=2026-01-31
```

Фільтри `from` і `to` включні. Якщо `from` не передано, використовується найраніша доступна дата. Якщо `to` не передано, використовується поточний день. Виручка рахується тільки за `Completed`; `Cancelled` не додається до revenue; `New` та `InProgress` рахуються як активні.

## Валідація

- ціна страви має бути більшою за 0;
- кількість позицій у замовленні має бути більшою за 0;
- ім'я працівника, назва страви та категорії не можуть бути порожніми;
- номер столика та кількість місць мають бути більшими за 0;
- замовлення не може бути створене без позицій;
- замовлення не можна створити для неіснуючого або зайнятого столика;
- замовлення не можна створити для неіснуючого працівника або страви.
- офіціант не може створити замовлення для іншого працівника;
- офіціант бачить і змінює тільки замовлення, пов'язані з його `EmployeeId`.
- офіціант не має доступу до AnalyticsController, ReportsController та export endpoint.

## Оновлення старої бази

Якщо в існуючій базі вже є старий статус столика, виконайте SQL:

```sql
UPDATE DiningTables SET Status = 'Free' WHERE Status = 'Cleaning';
```

Після зміни моделі можна створити міграцію:

```powershell
dotnet ef migrations add RemoveCleaningStatusAndAddReportExports
dotnet ef database update
```

## Перевірка

```powershell
dotnet build
dotnet run --urls http://localhost:5178
```

У Swagger:

1. Залогінитися як `admin` через `/api/auth/login`.
2. Авторизуватися токеном і перевірити `/api/users`, `/api/reports/sales`, `/api/reports/orders/export`, `/api/analytics/summary`.
3. Залогінитися як `waiter`.
4. Перевірити, що `/api/users` повертає `403`.
5. Перевірити, що `/api/analytics/summary` і `/api/reports/orders` повертають `403`.
6. Перевірити, що `GET /api/orders` повертає тільки замовлення цього офіціанта.
7. Створити замовлення від імені waiter: `employeeId` буде взято з JWT, а спроба передати іншого працівника завершиться `403`.
8. Завершити замовлення через `PATCH /api/orders/{id}/status` з `{ "status": "Completed" }` і перевірити, що столик став `Free`.
