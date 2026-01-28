CREATE DATABASE MasterSkladDB;
GO

USE MasterSkladDB;
GO

-- 1. Таблица должностей
CREATE TABLE positions (
    ID_position INT PRIMARY KEY IDENTITY(1,1),
    title NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- 2. Таблица сотрудников
CREATE TABLE employees (
    ID_employee INT PRIMARY KEY IDENTITY(1,1),
    lastName NVARCHAR(50) NOT NULL,
    firstName NVARCHAR(50) NOT NULL,
    middleName NVARCHAR(50),
    email NVARCHAR(100) NOT NULL UNIQUE CHECK(email LIKE '%_@__%.__%'),
    isDeleted BIT NOT NULL DEFAULT 0,
    position_ID INT NOT NULL FOREIGN KEY REFERENCES positions(ID_position)
);
GO

-- 3. Таблица ролей
CREATE TABLE roles (
    ID_role INT PRIMARY KEY IDENTITY(1,1),
    title NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- 4. Таблица пользователей
CREATE TABLE users (
    ID_user INT PRIMARY KEY IDENTITY(1,1),
    login NVARCHAR(30) NOT NULL UNIQUE CHECK(LEN(login) >= 5),
    password NVARCHAR(255) NOT NULL,
    isDeleted BIT NOT NULL DEFAULT 0,
    role_ID INT NOT NULL FOREIGN KEY REFERENCES roles(ID_role),
    employee_ID INT NOT NULL FOREIGN KEY REFERENCES employees(ID_employee)
);
GO

-- 5. Таблица статусов склада
CREATE TABLE warehouse_statuses (
    ID_status INT PRIMARY KEY IDENTITY(1,1),
    title NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- 6. Таблица объектов (зданий)
CREATE TABLE objects (
    ID_object INT PRIMARY KEY IDENTITY(1,1),
    address NVARCHAR(255) NOT NULL UNIQUE,
    description NVARCHAR(MAX)
);
GO

-- 7. Таблица складов
CREATE TABLE warehouses (
    ID_warehouse INT PRIMARY KEY IDENTITY(1,1),
    warehouse_number VARCHAR(20) NOT NULL,
    area DECIMAL(10,2) NOT NULL CHECK(area > 0),
    monthlyPrice DECIMAL(10,2) NOT NULL CHECK(monthlyPrice > 0),
    isDeleted BIT NOT NULL DEFAULT 0,
    status_ID INT NOT NULL FOREIGN KEY REFERENCES warehouse_statuses(ID_status),
    object_ID INT NOT NULL FOREIGN KEY REFERENCES objects(ID_object),
);
GO

-- 8. Таблица клиентов
CREATE TABLE clients (
    ID_client INT PRIMARY KEY IDENTITY(1,1),
    companyName NVARCHAR(150) NOT NULL,
    lastNamePerson NVARCHAR(50) NOT NULL,
    firstNamePerson NVARCHAR(50) NOT NULL,
    middleNamePerson NVARCHAR(50),
    phoneNumber CHAR(11) NOT NULL UNIQUE CHECK(phoneNumber LIKE '8__________'),
    email NVARCHAR(100) NOT NULL UNIQUE CHECK(email LIKE '%_@__%.__%'),
    requisites NVARCHAR(MAX),
    contractDate DATE DEFAULT GETDATE(),
    isDeleted BIT NOT NULL DEFAULT 0,
);
GO

-- 9. Таблица статусов оплаты
CREATE TABLE payment_statuses (
    ID_pstatus INT PRIMARY KEY IDENTITY(1,1),
    title NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- 10. Таблица аренды (договоры аренды)
CREATE TABLE leases (
    ID_lease INT PRIMARY KEY IDENTITY(1,1),
    contractNumber NVARCHAR(50) NOT NULL UNIQUE,
    startDate DATE NOT NULL,
    endDate DATE NOT NULL,
    totalPrice DECIMAL(10,2) NOT NULL CHECK(totalPrice > 0),
    client_ID INT NOT NULL FOREIGN KEY REFERENCES clients(ID_client),
    warehouse_ID INT NOT NULL FOREIGN KEY REFERENCES warehouses(ID_warehouse),
    manager_ID INT NOT NULL FOREIGN KEY REFERENCES employees(ID_employee),
    pstatus_ID INT NOT NULL FOREIGN KEY REFERENCES payment_statuses(ID_pstatus),
    isDeleted BIT NOT NULL DEFAULT 0,
    CHECK(startDate <= endDate),
    CHECK(endDate >= startDate)
);
GO

-- 11. Таблица услуг
CREATE TABLE services (
    ID_service INT PRIMARY KEY IDENTITY(1,1),
    title NVARCHAR(100) NOT NULL,
    description NVARCHAR(MAX),
    price DECIMAL(10,2) NOT NULL CHECK(price > 0),
    isDeleted BIT NOT NULL DEFAULT 0
);
GO

-- 12. Таблица оказанных услуг (журнал)
CREATE TABLE provided_services (
    ID_provided_service INT PRIMARY KEY IDENTITY(1,1),
    serviceDate DATE NOT NULL,
    quantity INT DEFAULT 1 CHECK(quantity >= 1),
    isDeleted BIT NOT NULL DEFAULT 0,
    lease_ID INT NOT NULL FOREIGN KEY REFERENCES leases(ID_lease) ON DELETE CASCADE,
    service_ID INT NOT NULL FOREIGN KEY REFERENCES services(ID_service),
);
GO

-- Заполнение начальными данными
INSERT INTO roles (title)
VALUES
    (N'Администратор'), (N'Менеджер'), (N'Директор');
GO

INSERT INTO positions (title)
VALUES
    (N'Генеральный директор'), (N'Старший менеджер'), (N'Администратор');;
GO

INSERT INTO warehouse_statuses (title)
VALUES
    (N'Свободен'), (N'Занят');
GO

INSERT INTO payment_statuses (title)
VALUES
    (N'Оплачен'), (N'Ожидает оплаты');
GO

INSERT INTO employees (lastName, firstName, middleName, email, position_ID)
VALUES
    (N'Иванов', N'Алексей', N'Николаевич', 'ivanov@sklad.ru', 1),
    (N'Романов', N'Дмитрий', N'Андреевич', 'romanov@mail.ru', 2),
    (N'Огалев', N'Лев', N'Евгеньевич', 'ogalev@mail.ru', 3);
GO

INSERT INTO users (login, password, role_ID, employee_ID)
VALUES
    (N'admin', N'qwerty', 1, 3),
    (N'manager', N'123456', 2, 2),
    (N'director', N'123qwe', 3, 1);
GO

INSERT INTO objects (address, description) 
VALUES (N'ул. Ленина, д. 10', N'Центральный складской комплекс, ангар А');
GO

INSERT INTO warehouses (warehouse_number, area, monthlyPrice, status_ID, object_ID)
VALUES ('A-101', 25.50, 15000, 2, 1);
GO

INSERT INTO clients (companyName, lastNamePerson, firstNamePerson, middleNamePerson, phoneNumber, email, requisites)
VALUES (N'ООО "Ромашка"', N'Петров', N'Петр', N'Андреевич', '89001112233', 'romashka@mail.ru', N'ИНН 7701234567');
GO

INSERT INTO leases (contractNumber, startDate, endDate, totalPrice, client_ID, warehouse_ID, manager_ID, pstatus_ID)
VALUES ('zum-2026/01', '2026-02-01', '2026-12-31', 15000, 1, 1, 1, 1);
GO

INSERT INTO services (title, description, price)
VALUES (N'Погрузка', N'Работа грузчиков (за 1 час)', 1000);
GO

INSERT INTO provided_services (serviceDate, quantity, lease_ID, service_ID)
VALUES ('2026-01-26', 2, 1, 1);
GO

