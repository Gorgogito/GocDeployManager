using System;
using System.Windows;
using GocDeployManager.UI.Ventanas;

namespace GocDeployManager.UI.Configuracion
{
    public partial class CredencialesBitbucketDialog : VentanaBase
    {
        private readonly Bootstrapper _bootstrapper;
        private readonly string _nombreUsuario;

        public CredencialesBitbucketDialog(Bootstrapper bootstrapper, string nombreUsuario)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            _nombreUsuario = nombreUsuario;
            InitializeComponent();
            Loaded += CredencialesBitbucketDialog_Loaded;
        }

        private void CredencialesBitbucketDialog_Loaded(object sender, RoutedEventArgs e)
        {
            lblSubtitulo.Text = $"Credenciales de Bitbucket — {_nombreUsuario}";
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            MostrarError(string.Empty);

            var usuarioBitbucket = txtUsuarioBitbucket.Text.Trim();
            var clave = txtContrasenaBitbucket.Password;

            if (string.IsNullOrWhiteSpace(usuarioBitbucket) || string.IsNullOrWhiteSpace(clave))
            {
                MostrarError("Usuario y contraseña de Bitbucket son obligatorios.");
                return;
            }

            var resultado = _bootstrapper.Usuarios.EstablecerCredencialesBitbucket(
                _nombreUsuario, usuarioBitbucket, clave);

            if (resultado.IsFailure)
            {
                MostrarError(resultado.Error);
                return;
            }

            MessageBox.Show(
                $"Se guardaron las credenciales de Bitbucket de '{_nombreUsuario}'.",
                "Credenciales guardadas",
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
