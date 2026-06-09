# Entity Framework Core migrations

У папці є навчальна міграція `ReplaceCustomersWithDiningTables`, яка показує перехід від клієнтів до столиків.

Команди для створення нової міграції локально після `dotnet restore`:

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add ReplaceCustomersWithDiningTables
dotnet ef database update
```

У цьому навчальному проєкті також додано SQL-скрипт `DatabaseScripts/schema-and-seed.sql`, щоб структуру бази можна було переглянути або створити вручну.
