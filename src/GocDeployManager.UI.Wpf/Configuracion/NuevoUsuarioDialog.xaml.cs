using System;
using System.Windows;
using GocDeployManager.Domain.Entities;
using GocDeployManager.UI.Ventanas;

namespace GocDeployManager.UI.Configuracion
{
    public partial class NuevoUsuarioDialog : VentanaBase
    {
        private readonly Bootstrapper _bootstrapper;

        public NuevoUsuarioDialog(Bootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            InitializeComponent();
            Loaded += NuevoUsuarioDialog_Loaded;
        }

        private void NuevoUsuarioDialog_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (RolUsuario rol in Enum.GetValues(typeof(RolUsuario)))
                comboRol.Items.Add(rol);
            comboRol.SelectedItem = RolUsuario.Operador;
        }

        private void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            MostrarError(string.Empty);

            var usuario = txtUsuario.Text.Trim();
            var nombreVisible = txtNombreVisible.Text.Trim();
            var clave = txtContrasena.Password;
            var confirmar = txtConfirmar.Password;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(nombreVisible) || string.IsNullOrWhiteSpace(clave))
            {
                MostrarError("Todos los campos son obligatorios.");
                return;
            }

            if (clave != confirmar)
            {
                MostrarError("Las contraseñas no coinciden.");
                return;
            }

            if (!(comboRol.SelectedItem is RolUsuario rol))
            {
                MostrarError("Selecciona un rol.");
                return;
            }

            var resultado = _bootstrapper.Usuarios.CrearUsuario(usuario, nombreVisible, rol, clave);
            if (resultado.IsFailure)
            {
                MostrarError(resultado.Error);
                return;
            }

            MessageBox.Show(
                $"El usuario '{usuario}' se creó correctamente.",
                "Usuario creado",
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
