# GocDeployManager — Historial y contexto del proyecto

> Documento de referencia para retomar el trabajo sin re-explorar el código.
> Última revisión: 2026-10-02 (sobre el commit `2564687 Versions`).

## 1. Qué es

Aplicación de escritorio Windows (**WPF, .NET Framework 4.8**) que despliega sistemas del GOC en un ambiente
(Desarrollo/Testing/Producción) a partir de una **rama de Bitbucket**. Por cada sistema seleccionado ejecuta:

`Validación → git clone/fetch → MSBuild (Clean,Build) → copia de archivos al destino`

y registra siempre el resultado (exitoso o fallido) en un historial en SQL Server.

- La rama se deriva del número de GOC (`GOC-00000` → `feature/GOC-00000`) o se elige una rama estándar / libre (ver sección 11).
- **Sin cancelación** una vez iniciado el despliegue (decisión del cliente).
- Reemplaza los `.bat` manuales (`compilacion.bat`, `despliegue.bat`) de los sistemas SIT/IDI/ProPag.

## 2. Arquitectura (Clean Architecture)

| Proyecto | Rol |
|---|---|
| `Domain` | Entidades (`Goc`, `Ambiente`, `AmbienteSistema`, `Sistema`, `ConfiguracionSistema`, `SecuenciaDeBuild`, `PasoDeBuild`, `Despliegue`, `AppUser`, `MensajeSalidaDespliegue`…) e interfaces (`IGitClient`, `IMsBuildRunner`, `IFileDeployer`, repositorios, `IAppLogger`, `IPasswordHasher`, `ICredentialProtector`) |
| `Common` | `Guard` (validaciones) y `Result` / `Result<T>` (fallas esperadas sin excepciones) |
| `Application` | Casos de uso: `DeploymentOrchestrator`, `AuthenticationService`, `UserManagementService`, `AmbienteManagementService`, `SistemaManagementService`, `HistorialQueryService` |
| `Infrastructure` | `GitClient`, `MsBuildRunner`, `FileDeployer`, repos `SqlServer*`, `JsonExclusionRulesRepository` |
| `Services` | `NLogAppLogger`, `DpapiCredentialProtector`, `Pbkdf2PasswordHasher` |
| `UI.Wpf` | App WPF (Material Design 3 con `MaterialDesignThemes` 4.9). `Bootstrapper` = composition root |
| `tests/` | `Domain.Tests`, `Application.Tests`, `Infrastructure.Tests` (~72 tests; no hay tests de UI) |

Idioma del código: **español** (nombres de clases, métodos, mensajes). Mantener ese estilo.

## 3. Flujo de despliegue (detalle técnico)

Núcleo: [DeploymentOrchestrator.cs](../src/GocDeployManager.Application/Deploy/DeploymentOrchestrator.cs) `EjecutarDespliegue(solicitud, IProgress<MensajeSalidaDespliegue>)`.

1. **Validación** por sistema: obtiene `ConfiguracionSistema` (URL repo, carpeta precompilada, secuencia de build) y verifica que el ambiente tenga ruta destino para el sistema.
2. **Clonado** (`GitClient`): si existe `.git` en `RutaClonado\<codigoSistema>\<GOC>` hace `fetch` + `checkout` + `reset --hard origin/<rama>`; si no, `clone --branch <rama> --single-branch`. Credenciales Bitbucket por variables de entorno + `GIT_ASKPASS` (script `.cmd` estático en `RutaTemporales`), nunca por línea de comandos; `GIT_TERMINAL_PROMPT=0`.
3. **Compilación** (`MsBuildRunner`): por cada `PasoDeBuild` ejecuta `MSBuild.exe /t:Clean,Build <ParametrosMsBuild>` con `WorkingDirectory` = carpeta del proyecto. Stdout/stderr en streaming.
4. **Despliegue** (`FileDeployer`): **espejo** de `<ruta clonado>\<CarpetaPrecompilada>` hacia `RutaDestino` (copia sobrescribiendo, **borra** archivos/carpetas del destino que no están en el origen), salvo archivos que coincidan con patrones de exclusión (`ExclusionRules.json`, p. ej. `web.config`): esos nunca se pisan ni se borran si ya existen.
5. **Finalización**: registra `Despliegue.RegistrarExitoso/RegistrarFallido` en `DeployHistory` (con tiempos de compilación y despliegue, usuario app/Windows, equipo).

Ante el primer error se aborta todo el despliegue (los sistemas restantes no se procesan) y se registra el fallo.
Los mensajes de proceso (git/MSBuild) se clasifican `Error` si contienen "fatal:" o la palabra `error`, si no `Debug`; las líneas en blanco se descartan.

## 4. UI (WPF)

- `LoginWindow` → `MainWindow` (principal: GOC, ambiente, checkboxes de sistemas, botón Iniciar, panel de salida en tiempo real, barra de progreso por etapas = `N sistemas × 4 + 1`, reloj de tiempo transcurrido, snackbar).
- `ConfiguracionWindow` (solo Administrador): 4 pestañas — ambientes/rutas destino, sistemas/secuencia de build, usuarios, rutas generales + diálogos (credenciales Bitbucket, nuevo usuario, resetear contraseña).
- `HistorialWindow`: filtros, detalle (`DetalleDespliegueDialog`) y exportación Excel (ClosedXML) / PDF (PdfSharp) vía `ExportadorHistorial`.
- Tema Claro/Oscuro en vivo (`TemaManager`, persistido en `App.config` clave `TemaBase`). `VentanaBase` = ventana sin marco con barra propia.
- El despliegue corre en `Task.Run`; el progreso llega por `Progress<T>` al hilo de UI.

## 5. Seguridad y roles

- Roles: `Administrador` (despliega + administra), `Operador` (despliega), `Consulta` (solo ve).
- Contraseñas de la app: PBKDF2 (hash + sal). Contraseña de Bitbucket por usuario: protegida con **DPAPI** y guardada en `AppUser`; se descifra al hacer login y vive en `SesionUsuario`.

## 6. Persistencia

- **SQL Server compartido** (cadena `CadenaConexionSqlServer` en `App.config`/`GocDeployManager.exe.config`): tablas `AppUser`, `DeployHistory`, `Ambiente`, `AmbienteSistema`, `ConfiguracionSistema`, `PasoDeBuild`.
- Esquema en [sql/schema-sql-server.sql](../sql/schema-sql-server.sql), idempotente, **lo ejecuta un DBA**; la app nunca crea/modifica tablas (`SqlServerEsquema` solo verifica).
- Archivo local: `Data\Config\ExclusionRules.json`. Carpetas `Data\Clonado`, `Data\Temp`, `Data\Logs` (NLog) junto al exe.
- Rutas/EXEs configurables en `App.config`: `RutaClonado`, `RutaTemporales`, `RutaLogs`, `RutaConfiguracion`, `RutaMsBuildExe` (por defecto `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe`), `RutaGitExe` (`git.exe` por PATH).

## 7. Estructura del repo

- `src/`, `tests/`, `sql/`, `docs/` (`analisis-tecnico.html`, `manual-usuario.html` v2 con capturas reales, este historial).
- `version/` — binarios publicados listos para usar (exe + `.config`). `GocDeployManager Test/` — build de pruebas antigua (WinForms/SQLite, contiene `.db` y logs; obsoleta).
- `fix/` — capturas de un bug de borde de tema. `.claude/worktrees/` — worktree de agente (submódulo sucio, ignorar).
- Soluciones: `GocDeployManager.sln` (compatible VS2019) y `.slnx`.

## 8. Línea de tiempo (git)

| Fecha | Hito |
|---|---|
| 2026-07-24 | Versión inicial (WinForms + SQLite + JSON), análisis técnico y manual |
| 2026-07-25 | Migración de persistencia SQLite → SQL Server; esquema `.sql`; `.sln` para VS2019 |
| ~2026-07 | Fixes: login con credencial Bitbucket no desprotegible; rutas con caracteres inválidos; recorte de espacios |
| ~2026-07 | Ambientes/Sistemas migrados de JSON a SQL Server; se eliminan repos JSON obsoletos (queda solo ExclusionRules) |
| ~2026-08 | Panel de salida en tiempo real (streaming git/MSBuild), scroll horizontal, fix de líneas en blanco, spinner no tapa el panel |
| ~2026-08 | `DesktopComponents` (controles propios Metro/Material 3) y refinamiento visual |
| 2026-08 | **Migración WPF** por fases: base+Login+Main → Configuración → Historial → tema persistente y eliminación de proyectos WinForms (`DesktopComponents` ya no existe) |
| 2026-08 | Ajustes visuales (botones, overlay, barra de progreso real, etiquetas de estado), icono, Enter en login |
| 2026-08-25 | Manual de usuario v2 con screenshots reales; último commit `Versions` |

## 9. Observaciones / deuda conocida (a la fecha)

- `App.config` (versionado en git) contiene una cadena de conexión con usuario `sa` y **contraseña en texto plano**; conviene moverla fuera del repo / usar autenticación integrada.
- `GitClient`/`MsBuildRunner` usan `WaitForExit()` sin timeout; no hay cancelación (intencional).
- `FileDeployer` borra en destino todo lo que no esté en el origen (comportamiento intencional, equivalente a `del /s` + `xcopy`); sin respaldo previo.
- Primer error aborta el despliegue completo; los sistemas ya desplegados antes del fallo no se revierten.
- Mensajes de error muestran en `MainWindow` solo la primera línea; el detalle completo queda en historial/logs.
- No hay tests de UI; los de Infrastructure con SQL Server/git/MSBuild reales requieren entorno.
- Binarios (`version/`, `GocDeployManager Test/`) están versionados en git.

## 10. Cómo trabajar con este proyecto

- Compilar: abrir `GocDeployManager.sln` (VS 2019+) o `msbuild`; target `net48`, `LangVersion latest`.
- Al agregar funcionalidad: lógica en `Application`/`Domain` con `Result<T>` (sin excepciones para fallas esperadas), implementación en `Infrastructure`, cableado solo en `Bootstrapper`, UI en `UI.Wpf`. Comentarios y nombres en español.
- Actualizar este archivo (sección 8 y 9) con cada mejora relevante.

## 11. Registro de mejoras posteriores

### 2026-10-02 — Despliegue de cualquier rama (no solo GOC)

- **Qué**: la pantalla principal tiene un selector con 3 modos: **GOC** (`feature/GOC-00000`, como antes), **Rama estándar** (lista configurable) y **Otra rama** (texto libre, validado con reglas de nombres de git).
- **Dominio**: nueva `ReferenciaDespliegue` (`Etiqueta`, `Rama`, `CarpetaTrabajo`); `SolicitudDespliegue.Goc` pasó a `Referencia`. Carpeta de clonado: número de GOC (igual que antes) o la rama con `/` → `_`. En historial, columna `Goc` = etiqueta (GOC o nombre de rama, máx. 50) y `Rama` = rama real; sin cambios de esquema en `DeployHistory`.
- **Ramas estándar**: tabla `RamaConfigurada` (Orden, Nombre) vía `SqlServerRamaRepository` + `RamaManagementService`; pestaña **Ramas** en Configuración (solo Administrador). La tabla se **crea sola** con `master` y `develop` la primera vez que se usa (también está en `sql/schema-sql-server.sql`); si el login no puede crear tablas, el error indica ejecutar el script. `SqlServerEsquema.Verificar` no la exige (no rompe instalaciones existentes).
- **Sin confirmación extra** al desplegar ramas no-GOC (decisión del usuario).
- **Tests**: `ReferenciaDespliegueTests`, `RamaManagementServiceTests`, `SqlServerRamaRepositoryTests` (LocalDB). Manual de usuario actualizado (secciones 3, 4, 5.5 y FAQ; sin captura de pantalla nueva).
