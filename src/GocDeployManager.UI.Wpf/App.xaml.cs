using System;
using System.Windows;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.UI.Login;

namespace GocDeployManager.UI
{
    public partial class App
    {
        private IAppLogger _logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += (s, ex) =>
            {
                ManejarExcepcion(ex.Exception);
                ex.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
                ManejarExcepcion(ex.ExceptionObject as Exception);

            Bootstrapper bootstrapper;
            try
            {
                bootstrapper = new Bootstrapper();
                _logger = bootstrapper.Logger;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo iniciar la aplicación:{Environment.NewLine}{ex.Message}",
                    "GocDeployManager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
                return;
            }

            new LoginWindow(bootstrapper).Show();
        }

        private void ManejarExcepcion(Exception ex)
        {
            _logger?.Error("Excepción no manejada.", ex);
            MessageBox.Show(
                $"Ocurrió un error inesperado:{Environment.NewLine}{ex?.Message}",
                "GocDeployManager",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
