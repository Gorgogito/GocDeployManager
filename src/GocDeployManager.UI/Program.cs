using System;
using System.IO;
using GocDeployManager.Application.Configuracion;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Infrastructure.Json;
using GocDeployManager.Infrastructure.SqlServer;
using GocDeployManager.UI.Login;

namespace GocDeployManager.UI
{
    internal static class Program
    {
        // Asignado recién si Bootstrapper arranca con éxito: si falla antes,
        // no hay ruta de logs resuelta todavía para escribir en ningún lado.
        private static IAppLogger _logger;

        [STAThread]
        private static void Main(string[] args)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            System.Windows.Forms.Application.ThreadException += (s, e) => ManejarExcepcion(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ManejarExcepcion(e.ExceptionObject as Exception);

            if (args.Length > 0 && string.Equals(args[0], "--migrar-configuracion", StringComparison.OrdinalIgnoreCase))
            {
                MigrarConfiguracionDesdeJson();
                return;
            }

            Bootstrapper bootstrapper;
            try
            {
                bootstrapper = new Bootstrapper();
                _logger = bootstrapper.Logger;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"No se pudo iniciar la aplicación:{Environment.NewLine}{ex.Message}",
                    "GocDeployManager",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            System.Windows.Forms.Application.Run(new LoginForm(bootstrapper));
        }

        /// <summary>
        /// Migración única de Ambientes.json/Sistemas.json (el esquema
        /// anterior, un archivo por laptop) hacia las tablas compartidas de
        /// SQL Server. Se corre una sola vez, desde la máquina que tenga el
        /// JSON correcto, con "GocDeployManager.UI.exe --migrar-configuracion"
        /// (ver manual del usuario) — no hace nada si el destino ya tiene datos.
        /// </summary>
        private static void MigrarConfiguracionDesdeJson()
        {
            Bootstrapper bootstrapper;
            try
            {
                bootstrapper = new Bootstrapper();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"No se pudo conectar a SQL Server para migrar:{Environment.NewLine}{ex.Message}",
                    "Migración de configuración",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            var origenAmbientes = new JsonAmbienteRepository(Path.Combine(bootstrapper.RutaConfiguracion, "Ambientes.json"));
            var origenSistemas = new JsonSistemaRepository(Path.Combine(bootstrapper.RutaConfiguracion, "Sistemas.json"));
            var destinoAmbientes = new SqlServerAmbienteRepository(bootstrapper.CadenaConexionSqlServer);
            var destinoSistemas = new SqlServerSistemaRepository(bootstrapper.CadenaConexionSqlServer);

            var migrador = new MigradorConfiguracion(origenAmbientes, destinoAmbientes, origenSistemas, destinoSistemas);
            var resultado = migrador.Migrar();

            System.Windows.Forms.MessageBox.Show(
                resultado.IsSuccess ? resultado.Value : resultado.Error,
                "Migración de configuración",
                System.Windows.Forms.MessageBoxButtons.OK,
                resultado.IsSuccess ? System.Windows.Forms.MessageBoxIcon.Information : System.Windows.Forms.MessageBoxIcon.Warning);
        }

        private static void ManejarExcepcion(Exception ex)
        {
            _logger?.Error("Excepción no manejada.", ex);

            System.Windows.Forms.MessageBox.Show(
                $"Ocurrió un error inesperado:{Environment.NewLine}{ex?.Message}",
                "GocDeployManager",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error);
        }
    }
}
