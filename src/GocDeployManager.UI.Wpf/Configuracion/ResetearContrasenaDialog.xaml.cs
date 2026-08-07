using System;
using System.Windows;
using GocDeployManager.UI.Ventanas;

namespace GocDeployManager.UI.Configuracion
{
    public partial class ResetearContrasenaDialog : VentanaBase
    {
        private readonly Bootstrapper _bootstrapper;
        private readonly string _nombreUsuario;

        public ResetearContrasenaDialog(Bootstrapper bootstrapper, string nombreUsuario)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            _nombreUsuario = nombreUsuario;
            InitializeComponent();
            Loaded += ResetearContrasenaDialog_Loaded;
        }

        private void ResetearContrasenaDialog_Loaded(object sender, RoutedEventArgs e)
        {
            lblSubtitulo.Text = $"Resetear contraseña — {_nombreUsuario}";
        }

        private void BtnResetear_Click(object sender, RoutedEventArgs e)
        {
            MostrarError(string.Empty);

            var nueva = txtNuevaContrasena.Password;
            var confirmar = txtConfirmar.Password;

            if (string.IsNullOrWhiteSpace(nueva))
            {
                MostrarError("La nueva contraseña es obligatoria.");
                return;
            }

            if (nueva != confirmar)
            {
                MostrarError("Las contraseñas no coinciden.");
                return;
            }

            var resultado = _bootstrapper.Usuarios.ResetearContrasena(_nombreUsuario, nueva);
            if (resultado.IsFailure)
            {
                MostrarError(resultado.Error);
                return;
            }

            MessageBox.Show(
                $"Se actualizó la contraseña de '{_nombreUsuario}'.",
                "Contraseña reseteada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }

        private void MostrarError(string mensaje)
        {
            lblError.Text = mensaje;
            lblError.Visibility = string.IsNullOrEmpty(mensaje) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
