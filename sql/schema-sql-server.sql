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

-- Ambientes y Sistemas (2026-07-31) — antes vivían en Ambientes.json/
-- Sistemas.json bajo Data\Config, un archivo por laptop. Al ser datos que
-- deben ser iguales para todos los usuarios (no una preferencia personal),
-- pasan a la base compartida igual que el resto. Los datos que ya existan
-- en esos JSON se migran una única vez con
-- "GocDeployManager.UI.exe --migrar-configuracion" (ver manual del usuario).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Ambiente')
BEGIN
    CREATE TABLE Ambiente (
        Nombre                      NVARCHAR(100)   NOT NULL PRIMARY KEY,
        NotificacionesHabilitadas   BIT             NOT NULL DEFAULT 1
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AmbienteSistema')
BEGIN
    CREATE TABLE AmbienteSistema (
        Id              INT             IDENTITY(1,1) PRIMARY KEY,
        AmbienteNombre  NVARCHAR(100)   NOT NULL,
        SistemaCodigo   NVARCHAR(50)    NOT NULL,
        SistemaNombre   NVARCHAR(100)   NOT NULL,
        RutaDestino     NVARCHAR(500)   NOT NULL,
        CONSTRAINT FK_AmbienteSistema_Ambiente
            FOREIGN KEY (AmbienteNombre) REFERENCES Ambiente (Nombre) ON DELETE CASCADE,
        CONSTRAINT UQ_AmbienteSistema_AmbienteYSistema UNIQUE (AmbienteNombre, SistemaCodigo)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ConfiguracionSistema')
BEGIN
    CREATE TABLE ConfiguracionSistema (
        Codigo              NVARCHAR(50)    NOT NULL PRIMARY KEY,
        Nombre              NVARCHAR(100)   NOT NULL,
        RepositorioUrl      NVARCHAR(500)   NOT NULL,
        CarpetaPrecompilada NVARCHAR(500)   NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PasoDeBuild')
BEGIN
    CREATE TABLE PasoDeBuild (
        Id                  INT             IDENTITY(1,1) PRIMARY KEY,
        SistemaCodigo       NVARCHAR(50)    NOT NULL,
        Orden               INT             NOT NULL,
        CarpetaProyecto     NVARCHAR(200)   NOT NULL,
        ParametrosMsBuild   NVARCHAR(500)   NULL,
        CONSTRAINT FK_PasoDeBuild_ConfiguracionSistema
            FOREIGN KEY (SistemaCodigo) REFERENCES ConfiguracionSistema (Codigo) ON DELETE CASCADE,
        CONSTRAINT UQ_PasoDeBuild_SistemaYOrden UNIQUE (SistemaCodigo, Orden)
    );
END
GO
