CREATE DATABASE POS_CAFETERIA;
GO

USE POS_CAFETERIA;
GO

-- =========================
-- TABLA ROLES (ADMIN / CAJERO)
-- =========================
CREATE TABLE ROLE (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(50) NOT NULL
);

INSERT INTO ROLE (Name) VALUES ('ADMIN'), ('CAJERO');


-- =========================
-- USUARIOS
-- =========================
CREATE TABLE USERS (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    RoleId INT,
    FOREIGN KEY (RoleId) REFERENCES ROLE(RoleId)
);

-- =========================
-- CATEGORIAS (CAFÉ, BEBIDAS, ETC)
-- =========================
CREATE TABLE CATEGORY (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(100) NOT NULL
);

-- =========================
-- PRODUCTOS
-- =========================
CREATE TABLE PRODUCT (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(100) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    Stock INT NOT NULL,
    CategoryId INT,
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (CategoryId) REFERENCES CATEGORY(CategoryId)
);

-- =========================
-- CLIENTES (OPCIONAL PERO PRO)
-- =========================
CREATE TABLE CLIENT (
    ClientId INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(100),
    Phone VARCHAR(20)
);

-- =========================
-- VENTAS (FACTURA)
-- =========================
CREATE TABLE SALE (
    SaleId INT PRIMARY KEY IDENTITY(1,1),
    Date DATETIME DEFAULT GETDATE(),
    Total DECIMAL(10,2),
    UserId INT,
    ClientId INT NULL,
    FOREIGN KEY (UserId) REFERENCES USERS(UserId),
    FOREIGN KEY (ClientId) REFERENCES CLIENT(ClientId)
);

-- =========================
-- DETALLE DE VENTA
-- =========================
CREATE TABLE SALE_DETAIL (
    SaleDetailId INT PRIMARY KEY IDENTITY(1,1),
    SaleId INT,
    ProductId INT,
    Quantity INT,
    Price DECIMAL(10,2),

    FOREIGN KEY (SaleId) REFERENCES SALE(SaleId),
    FOREIGN KEY (ProductId) REFERENCES PRODUCT(ProductId)
);

-- =========================
-- TRIGGER PARA DESCONTAR STOCK AUTOMÁTICO
-- =========================
GO
CREATE TRIGGER TR_DISCOUNT_STOCK
ON SALE_DETAIL
AFTER INSERT
AS
BEGIN
    UPDATE P
    SET P.Stock = P.Stock - I.Quantity
    FROM PRODUCT P
    INNER JOIN inserted I ON P.ProductId = I.ProductId;
END;
GO

-- =========================
-- PROCEDIMIENTO PARA REGISTRAR VENTA COMPLETA
-- =========================
GO
CREATE PROCEDURE REGISTER_SALE
    @UserId INT,
    @ClientId INT = NULL,
    @Total DECIMAL(10,2)
AS
BEGIN
    INSERT INTO SALE (UserId, ClientId, Total)
    VALUES (@UserId, @ClientId, @Total);

    SELECT SCOPE_IDENTITY() AS SaleId;
END;
GO

-- =========================
-- REPORTES
-- =========================

-- Ventas del día
SELECT * FROM SALE
WHERE CAST(Date AS DATE) = CAST(GETDATE() AS DATE);

-- Total del día
SELECT SUM(Total) AS TotalHoy
FROM SALE
WHERE CAST(Date AS DATE) = CAST(GETDATE() AS DATE);

-- Productos más vendidos
SELECT P.Name, SUM(SD.Quantity) AS TotalVendido
FROM SALE_DETAIL SD
JOIN PRODUCT P ON SD.ProductId = P.ProductId
GROUP BY P.Name
ORDER BY TotalVendido DESC;

-- Inventario bajo
SELECT * FROM PRODUCT
WHERE Stock < 5;