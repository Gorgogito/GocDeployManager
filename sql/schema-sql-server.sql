-- GocDeployManager — esquema de base de datos para SQL Server.
-- Ejecutar una sola vez contra la base destino antes de usar la aplicación
-- (Desarrollo/Testing/Producción tienen bases independientes). El script es
-- idempotente: puede volver a ejecutarse sin error si las tablas ya existen.
--
-- La aplicación NUNCA crea ni modifica estas tablas — solo lee/escribe datos.
-- Si el login que usa la app no tiene permisos de CREATE TABLE, este script
-- debe ejecutarlo alguien con permisos (DBA / administrador del servidor).

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AppUser')
BEGIN
    CREATE TABLE AppUser (
        NombreUsuario                  NVARCHAR(100)   NOT NULL PRIMARY KEY,
        NombreVisible                  NVARCHAR(200)   NOT NULL,
        Rol                             NVARCHAR(50)    NOT NULL,
        Activo                          BIT             NOT NULL,
        HashContrasena                  NVARCHAR(200)   NOT NULL,
        SalContrasena                   NVARCHAR(200)   NOT NULL,
        UsuarioBitbucket                NVARCHAR(200)   NULL,
        ContrasenaBitbucketProtegida    NVARCHAR(MAX)   NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DeployHistory')
BEGIN
    CREATE TABLE DeployHistory (
        Id                              INT             IDENTITY(1,1) PRIMARY KEY,
        FechaHora                       DATETIME2       NOT NULL,
        UsuarioAplicacion               NVARCHAR(100)   NOT NULL,
        UsuarioWindows                  NVARCHAR(100)   NOT NULL,
        Equipo                          NVARCHAR(100)   NOT NULL,
        Goc                             NVARCHAR(50)    NOT NULL,
        Rama                            NVARCHAR(200)   NOT NULL,
        Ambiente                        NVARCHAR(100)   NOT NULL,
        Sistemas                        NVARCHAR(500)   NOT NULL,
        TiempoCompilacionSegundos       FLOAT           NOT NULL,
        TiempoDespliegueSegundos        FLOAT           NOT NULL,
        Resultado                       NVARCHAR(50)    NOT NULL,
        Errores                         NVARCHAR(MAX)   NULL,
        Observaciones                   NVARCHAR(MAX)   NULL
    );
END
GO
