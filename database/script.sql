/* =============================================================
   Task Management System — SQL Server schema + JSON queries
   Compatible con SQL Server 2016+ (funciones JSON nativas)
   Idempotente: se puede ejecutar múltiples veces sin error
   ============================================================= */

-- 1)Base de datos -------------------------------------------------
IF DB_ID('TaskDB') IS NULL
BEGIN
    CREATE DATABASE TaskDB;
END
GO

USE TaskDB;
GO

-- 2)Tabla Users -------------------------------------------------
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        Id    INT IDENTITY(1,1) NOT NULL,
        Name  NVARCHAR(100)     NOT NULL,
        Email NVARCHAR(100)     NOT NULL,
        CONSTRAINT PK_Users        PRIMARY KEY (Id),
        CONSTRAINT UQ_Users_Email  UNIQUE (Email)
    );
END
GO

-- 3)Tabla Tasks ------------------------------------------------
IF OBJECT_ID('dbo.Tasks', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tasks (
        Id             INT IDENTITY(1,1) NOT NULL,
        Title          NVARCHAR(200)     NOT NULL,
        UserId         INT               NOT NULL,
        Status         NVARCHAR(20)      NOT NULL CONSTRAINT DF_Tasks_Status    DEFAULT ('Pending'),
        CreatedAt      DATETIME2(0)      NOT NULL CONSTRAINT DF_Tasks_CreatedAt DEFAULT (SYSUTCDATETIME()),
        AdditionalInfo NVARCHAR(MAX)     NULL,
        CONSTRAINT PK_Tasks
            PRIMARY KEY (Id),
        CONSTRAINT FK_Tasks_Users
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
        CONSTRAINT CHK_Tasks_Status
            CHECK (Status IN ('Pending', 'InProgress', 'Done')),
        CONSTRAINT CHK_Tasks_AdditionalInfo_IsJson
            CHECK (AdditionalInfo IS NULL OR ISJSON(AdditionalInfo) = 1)
    );
END
GO

-- 4)Índices -------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tasks_User_Status' AND object_id = OBJECT_ID('dbo.Tasks'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Tasks_User_Status ON dbo.Tasks (UserId, Status);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tasks_CreatedAt' AND object_id = OBJECT_ID('dbo.Tasks'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Tasks_CreatedAt ON dbo.Tasks (CreatedAt DESC);
END
GO

-- 5)Datos sembrados (idempotentes) -------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'admin@task.com')
    INSERT INTO dbo.Users (Name, Email) VALUES ('Admin', 'admin@task.com');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'ana@task.com')
    INSERT INTO dbo.Users (Name, Email) VALUES ('Ana Gomez', 'ana@task.com');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'luis@task.com')
    INSERT INTO dbo.Users (Name, Email) VALUES ('Luis Perez', 'luis@task.com');
GO

-- Tareas de ejemplo con AdditionalInfo en JSON
IF NOT EXISTS (SELECT 1 FROM dbo.Tasks)
BEGIN
    DECLARE @adminId INT = (SELECT Id FROM dbo.Users WHERE Email = 'admin@task.com');
    DECLARE @anaId   INT = (SELECT Id FROM dbo.Users WHERE Email = 'ana@task.com');
    DECLARE @luisId  INT = (SELECT Id FROM dbo.Users WHERE Email = 'luis@task.com');

    INSERT INTO dbo.Tasks (Title, UserId, Status, AdditionalInfo) VALUES
        ('Configurar entorno de desarrollo', @adminId, 'Done',
         N'{"priority":"High","dueDate":"2026-05-10","tags":["setup","infra"],"estimatedHours":4}'),
        ('Diseñar pantalla de tareas', @anaId, 'InProgress',
         N'{"priority":"Medium","dueDate":"2026-05-15","tags":["ui","design"],"estimatedHours":8}'),
        ('Refactor del módulo de usuarios', @luisId, 'Pending',
         N'{"priority":"Low","dueDate":"2026-05-22","tags":["refactor"],"estimatedHours":3}'),
        ('Implementar índice de búsqueda', @anaId, 'Pending',
         N'{"priority":"High","dueDate":"2026-05-12","tags":["backend","performance"],"estimatedHours":6}');
END
GO

/* =============================================================
   CONSULTA REQUERIDA POR EL ENUNCIADO (punto 6):
   Tareas por usuario, filtradas por estado, ordenadas por fecha.
   Esta misma consulta es la que usa el repositorio del backend.
   ============================================================= */

-- Ejemplo: tareas de Ana en estado Pending o InProgress
DECLARE @UserId INT = (SELECT Id FROM dbo.Users WHERE Email = 'ana@task.com');
DECLARE @Status NVARCHAR(20) = 'InProgress'; -- pasa NULL para todos los estados

SELECT  Id, Title, UserId, Status, CreatedAt, AdditionalInfo
FROM    dbo.Tasks
WHERE   UserId = @UserId
   AND  (@Status IS NULL OR Status = @Status)
ORDER BY CreatedAt DESC;
GO

/* =============================================================
   FUNCIONES JSON NATIVAS DE SQL SERVER (punto 10)
   Demuestran ISJSON, JSON_VALUE, JSON_QUERY, OPENJSON y JSON_MODIFY
   ============================================================= */

-- 10.1 ISJSON ─ validar contenido (también lo enforza el CHECK constraint)
SELECT  Id, Title,
        ISJSON(AdditionalInfo) AS IsValidJson
FROM    dbo.Tasks;

-- 10.2 JSON_VALUE ─ leer un campo escalar dentro del JSON
SELECT  Id, Title,
        JSON_VALUE(AdditionalInfo, '$.priority')        AS Priority,
        JSON_VALUE(AdditionalInfo, '$.dueDate')         AS DueDate,
        TRY_CAST(JSON_VALUE(AdditionalInfo, '$.estimatedHours') AS INT) AS EstimatedHours
FROM    dbo.Tasks
WHERE   AdditionalInfo IS NOT NULL;

-- 10.3 JSON_QUERY ─ extraer un sub-objeto o array (etiquetas)
SELECT  Id, Title,
        JSON_QUERY(AdditionalInfo, '$.tags') AS Tags
FROM    dbo.Tasks
WHERE   AdditionalInfo IS NOT NULL;

-- 10.4 Filtrar tareas por un valor dentro del JSON (priority = 'High')
SELECT  Id, Title, UserId,
        JSON_VALUE(AdditionalInfo, '$.priority') AS Priority
FROM    dbo.Tasks
WHERE   JSON_VALUE(AdditionalInfo, '$.priority') = 'High'
ORDER BY CreatedAt DESC;

-- 10.5 OPENJSON ─ descomponer el array de etiquetas en filas
SELECT  t.Id, t.Title, tag.value AS Tag
FROM    dbo.Tasks t
CROSS APPLY OPENJSON(JSON_QUERY(t.AdditionalInfo, '$.tags')) AS tag
WHERE   t.AdditionalInfo IS NOT NULL;

-- 10.6 OPENJSON con esquema ─ proyectar el JSON como tabla tipada
SELECT  t.Id, t.Title, info.Priority, info.DueDate, info.EstimatedHours
FROM    dbo.Tasks t
CROSS APPLY OPENJSON(t.AdditionalInfo)
    WITH (
        Priority       NVARCHAR(50) '$.priority',
        DueDate        DATE         '$.dueDate',
        EstimatedHours INT          '$.estimatedHours'
    ) AS info
WHERE   t.AdditionalInfo IS NOT NULL;

-- 10.7 (Opcional) JSON_MODIFY ─ actualizar un campo dentro del JSON
-- UPDATE dbo.Tasks
-- SET    AdditionalInfo = JSON_MODIFY(AdditionalInfo, '$.priority', 'Critical')
-- WHERE  Id = 1;
GO
