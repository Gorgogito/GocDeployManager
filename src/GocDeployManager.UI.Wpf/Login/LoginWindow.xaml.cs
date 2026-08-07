using System;
using System.Windows;
using GocDeployManager.Domain.Entities;
using GocDeployManager.UI.Principal;
using GocDeployManager.UI.Ventanas;

namespace GocDeployManager.UI.Login
{
    public partial class LoginWindow : VentanaBase
    {
        private readonly Bootstrapper _bootstrapper;
        private bool _modoPrimerArranque;

        public LoginWindow(Bootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            InitializeComponent();
            Loaded += LoginWindow_Loaded;
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_bootstrapper.Usuarios.ObtenerTodos().Count == 0)
                EntrarModoPrimerArranque();
        }

        private void EntrarModoPrimerArranque()
        {
            _modoPrimerArranque = true;
            lblTitulo.Text = "Crear usuario administrador";
            panelNombreVisible.Visibility = Visibility.Visible;
            panelConfirmarContrasena.Visibility = Visibility.Visible;
            btnAccion.Content = "CREAR Y CONTINUAR";
        }

        private void VolverAModoLogin()
        {
            _modoPrimerArranque = false;
            lblTitulo.Text = "Iniciar sesión";
            panelNombreVisible.Visibility = Visibility.Collapsed;
            panelConfirmarContrasena.Visibility = Visibility.Collapsed;
            txtNombreVisible.Text = string.Empty;
            txtContrasena.Password = string.Empty;
            txtConfirmarContrasena.Password = string.Empty;
            btnAccion.Content = "INGRESAR";
        }

        private void BtnAccion_Click(object sender, RoutedEventArgs e)
        {
            MostrarError(string.Empty);

            if (_modoPrimerArranque)
                CrearPrimerAdministrador();
            else
                IniciarSesion();
        }

        private void CrearPrimerAdministrador()
        {
            var usuario = txtUsuario.Text.Trim();
            var nombreVisible = txtNombreVisible.Text.Trim();
            var clave = txtContrasena.Password;
            var confirmar = txtConfirmarContrasena.Password;

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

            var resultado = _bootstrapper.Usuarios.CrearUsuario(usuario, nombreVisible, RolUsuario.Administrador, clave);
            if (resultado.IsFailure)
            {
                MostrarError(resultado.Error);
                return;
            }

            MessageBox.Show(
                $"Se creó el usuario administrador '{usuario}'. Ahora puedes iniciar sesión.",
                "Usuario creado",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            VolverAModoLogin();
            txtUsuario.Text = usuario;
        }

        private void IniciarSesion()
        {
            var usuario = txtUsuario.Text.Trim();
            var clave = txtContrasena.Password;

            var resultado = _bootstrapper.Autenticacion.IniciarSesion(usuario, clave);
            if (resultado.IsFailure)
            {
                MostrarError(resultado.Error);
                return;
            }

            var sesion = resultado.Value;
            Hide();
            var main = new MainWindow(_bootstrapper, sesion);
            main.Closed += (s, args) => Close();
            main.Show();
        }

        private void MostrarError(string mensaje)
        {
            if (string.IsNullOrEmpty(mensaje))
            {
                lblError.Visibility = Visibility.Collapsed;
                lblError.Text = string.Empty;
            }
            else
            {
                lblError.Text = mensaje;
                lblError.Visibility = Visibility.Visible;
            }
        }
    }
}
