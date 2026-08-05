using System;
using GocDeployManager.Domain.Abstractions;
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
