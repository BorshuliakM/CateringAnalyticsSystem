CREATE DATABASE CateringAnalyticsTablesDb;
GO

USE CateringAnalyticsTablesDb;
GO

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(50) NOT NULL
);

CREATE TABLE Employees (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Position NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(30) NOT NULL,
    Email NVARCHAR(120) NOT NULL
);

CREATE TABLE DiningTables (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Number INT NOT NULL,
    SeatsCount INT NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CONSTRAINT UQ_DiningTables_Number UNIQUE (Number)
);

CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NOT NULL
);

CREATE TABLE Dishes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Description NVARCHAR(700) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CategoryId INT NOT NULL,
    IsAvailable BIT NOT NULL,
    CONSTRAINT FK_Dishes_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DiningTableId INT NOT NULL,
    EmployeeId INT NOT NULL,
    OrderDate DATETIME2 NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_Orders_DiningTables FOREIGN KEY (DiningTableId) REFERENCES DiningTables(Id),
    CONSTRAINT FK_Orders_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
);

CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    DishId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Dishes FOREIGN KEY (DishId) REFERENCES Dishes(Id)
);

INSERT INTO Categories (Name, Description) VALUES
(N'Сніданки', N'Страви для ранкового меню'),
(N'Перші страви', N'Супи та бульйони'),
(N'Основні страви', N'Гарячі страви ресторану'),
(N'Салати', N'Легкі та сезонні салати'),
(N'Десерти', N'Солодкі страви'),
(N'Напої', N'Гарячі та холодні напої');

INSERT INTO Dishes (Name, Description, Price, CategoryId, IsAvailable) VALUES
(N'Омлет із сиром', N'Омлет з твердим сиром та зеленню', 95, 1, 1),
(N'Сирники зі сметаною', N'Домашні сирники з ягідним соусом', 120, 1, 1),
(N'Борщ український', N'Борщ зі сметаною та пампушками', 135, 2, 1),
(N'Курячий бульйон', N'Легкий бульйон з локшиною', 105, 2, 1),
(N'Котлета по-київськи', N'Класична котлета з картопляним пюре', 230, 3, 1),
(N'Паста карбонара', N'Паста з беконом, вершками та пармезаном', 210, 3, 1),
(N'Стейк курячий', N'Куряче філе на грилі з овочами', 195, 3, 1),
(N'Цезар з куркою', N'Салат з куркою, сухариками та соусом цезар', 175, 4, 1),
(N'Грецький салат', N'Овочі, фета та оливки', 150, 4, 1),
(N'Наполеон', N'Листковий десерт із заварним кремом', 110, 5, 1),
(N'Чизкейк', N'Сирний десерт з ягідним топінгом', 130, 5, 1),
(N'Еспресо', N'Класична кава', 45, 6, 1),
(N'Капучино', N'Кава з молочною пінкою', 65, 6, 1),
(N'Лимонад', N'Домашній лимонад', 75, 6, 1),
(N'Узвар', N'Традиційний напій із сухофруктів', 55, 6, 1);

INSERT INTO DiningTables (Number, SeatsCount, Status) VALUES
(1, 2, N'Free'),
(2, 2, N'Free'),
(3, 4, N'Free'),
(4, 4, N'Occupied'),
(5, 6, N'Occupied'),
(6, 6, N'Reserved'),
(7, 8, N'Free'),
(8, 4, N'Free');

INSERT INTO Employees (FullName, Position, Phone, Email) VALUES
(N'Наталія Романюк', N'Офіціант', N'+380661010101', N'nataliia.romaniuk@restaurant.local'),
(N'Сергій Литвин', N'Офіціант', N'+380672020202', N'serhii.lytvyn@restaurant.local'),
(N'Марія Ткаченко', N'Адміністратор', N'+380633030303', N'mariia.tkachenko@restaurant.local');

INSERT INTO Orders (DiningTableId, EmployeeId, OrderDate, TotalAmount, Status) VALUES
(1, 1, DATEADD(day, -5, SYSUTCDATETIME()), 455, N'Completed'),
(2, 2, DATEADD(day, -4, SYSUTCDATETIME()), 480, N'Completed'),
(3, 1, DATEADD(day, -3, SYSUTCDATETIME()), 395, N'Completed'),
(4, 3, DATEADD(day, -2, SYSUTCDATETIME()), 560, N'InProgress'),
(5, 2, DATEADD(day, -1, SYSUTCDATETIME()), 490, N'New'),
(3, 3, SYSUTCDATETIME(), 650, N'Completed'),
(7, 1, SYSUTCDATETIME(), 255, N'Cancelled');

INSERT INTO OrderItems (OrderId, DishId, Quantity, Price) VALUES
(1, 3, 1, 135), (1, 5, 1, 230), (1, 12, 2, 45),
(2, 8, 2, 175), (2, 13, 2, 65),
(3, 6, 1, 210), (3, 10, 1, 110), (3, 14, 1, 75),
(4, 1, 2, 95), (4, 2, 2, 120), (4, 13, 2, 65),
(5, 5, 1, 230), (5, 9, 1, 150), (5, 15, 2, 55),
(6, 7, 2, 195), (6, 11, 2, 130),
(7, 4, 1, 105), (7, 14, 2, 75);

INSERT INTO Users (Username, PasswordHash, Role) VALUES
(N'admin', N'demo-admin-hash', N'Admin'),
(N'manager', N'demo-manager-hash', N'Manager');
GO
