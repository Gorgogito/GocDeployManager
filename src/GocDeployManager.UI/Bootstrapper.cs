using System;
using System.Configuration;
using System.IO;
using GocDeployManager.Application.Ambientes;
using GocDeployManager.Application.Auth;
using GocDeployManager.Application.Deploy;
using GocDeployManager.Application.Historial;
using GocDeployManager.Application.Sistemas;
using GocDeployManager.Application.Usuarios;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Infrastructure.Build;
using GocDeployManager.Infrastructure.Deploy;
using GocDeployManager.Infrastructure.Git;
using GocDeployManager.Infrastructure.Json;
using GocDeployManager.Infrastructure.Sqlite;
using GocDeployManager.Services;

namespace GocDeployManager.UI
{
    /// <summary>
    /// Composition root: única clase que conoce Infrastructure y Services a la
    /// vez que Application — el resto de la UI solo ve los servicios ya armados.
    /// Lee las rutas desde App.config (sección 16 del análisis) y crea las
    /// carpetas de trabajo si no existen.
    /// </summary>
    public sealed class Bootstrapper
    {
        public AuthenticationService Autenticacion { get; }
        public UserManagementService Usuarios { get; }
        public AmbienteManagementService Ambientes { get; }
        public SistemaManagementService Sistemas { get; }
        public HistorialQueryService Historial { get; }
        public DeploymentOrchestrator Orquestador { get; }
        public IAppLogger Logger { get; }

        public string RutaClonado { get; private set; }
        public string RutaTemporales { get; private set; }
        public string RutaLogs { get; private set; }
        public string RutaConfiguracion { get; private set; }
        public string RutaBaseDatos { get; private set; }

        public Bootstrapper()
        {
            var rutaBase = AppDomain.CurrentDomain.BaseDirectory;

            RutaClonado = ResolverRuta(rutaBase, "RutaClonado", @".\Data\Clonado");
            RutaTemporales = ResolverRuta(rutaBase, "RutaTemporales", @".\Data\Temp");
            RutaLogs = ResolverRuta(rutaBase, "RutaLogs", @".\Data\Logs");
            RutaConfiguracion = ResolverRuta(rutaBase, "RutaConfiguracion", @".\Data\Config");
            RutaBaseDatos = ResolverRuta(rutaBase, "RutaBaseDatos", @".\Data\GocDeployManager.db");
            var rutaMsBuildExe = ConfigurationManager.AppSettings["RutaMsBuildExe"]
                ?? @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe";
            var rutaGitExe = ConfigurationManager.AppSettings["RutaGitExe"] ?? "git.exe";

            Directory.CreateDirectory(RutaClonado);
            Directory.CreateDirectory(RutaTemporales);
            Directory.CreateDirectory(RutaLogs);
            Directory.CreateDirectory(RutaConfiguracion);
            Directory.CreateDirectory(Path.GetDirectoryName(RutaBaseDatos) ?? rutaBase);

            var cadenaConexion = $"Data Source={RutaBaseDatos}";

            var appUserRepo = new SqliteAppUserRepository(cadenaConexion);
            var historialRepo = new SqliteDeployHistoryRepository(cadenaConexion);
            var ambienteRepo = new JsonAmbienteRepository(Path.Combine(RutaConfiguracion, "Ambientes.json"));
            var sistemaRepo = new JsonSistemaRepository(Path.Combine(RutaConfiguracion, "Sistemas.json"));
            var exclusionRepo = new JsonExclusionRulesRepository(Path.Combine(RutaConfiguracion, "ExclusionRules.json"));

            var hasher = new Pbkdf2PasswordHasher();
            var protector = new DpapiCredentialProtector();
            var git = new GitClient(rutaGitExe, RutaTemporales);
            var msbuild = new MsBuildRunner(rutaMsBuildExe);
            var deployer = new FileDeployer();
            Logger = new NLogAppLogger(RutaLogs);

            Autenticacion = new AuthenticationService(appUserRepo, hasher, protector, Logger);
            Usuarios = new UserManagementService(appUserRepo, hasher, protector, Logger);
            Ambientes = new AmbienteManagementService(ambienteRepo);
            Sistemas = new SistemaManagementService(sistemaRepo);
            Historial = new HistorialQueryService(historialRepo);
            Orquestador = new DeploymentOrchestrator(git, msbuild, deployer, sistemaRepo, exclusionRepo, historialRepo, Logger);
        }

        /// <summary>
        /// Escribe las rutas en App.config para la próxima vez que se abra la
        /// aplicación — los servicios ya armados en este Bootstrapper conservan
        /// las rutas con las que arrancaron (sección 16 del análisis: solo
        /// MSBuild/Git quedan fuera, son fijos y no se editan desde esta pantalla).
        /// </summary>
        public void GuardarRutasGenerales(string rutaClonado, string rutaTemporales, string rutaLogs, string rutaConfiguracion, string rutaBaseDatos)
        {
            var configuracion = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            EstablecerValor(configuracion, "RutaClonado", rutaClonado);
            EstablecerValor(configuracion, "RutaTemporales", rutaTemporales);
            EstablecerValor(configuracion, "RutaLogs", rutaLogs);
            EstablecerValor(configuracion, "RutaConfiguracion", rutaConfiguracion);
            EstablecerValor(configuracion, "RutaBaseDatos", rutaBaseDatos);

            configuracion.Save(ConfigurationSaveMode.Modified);

            RutaClonado = rutaClonado;
            RutaTemporales = rutaTemporales;
            RutaLogs = rutaLogs;
            RutaConfiguracion = rutaConfiguracion;
            RutaBaseDatos = rutaBaseDatos;
        }

        private static void EstablecerValor(Configuration configuracion, string clave, string valor)
        {
            if (configuracion.AppSettings.Settings[clave] == null)
                configuracion.AppSettings.Settings.Add(clave, valor);
            else
                configuracion.AppSettings.Settings[clave].Value = valor;
        }

        private static string ResolverRuta(string rutaBase, string clave, string valorPorDefecto)
        {
            var valor = ConfigurationManager.AppSettings[clave] ?? valorPorDefecto;
            return Path.IsPathRooted(valor) ? valor : Path.GetFullPath(Path.Combine(rutaBase, valor));
        }
    }
}
