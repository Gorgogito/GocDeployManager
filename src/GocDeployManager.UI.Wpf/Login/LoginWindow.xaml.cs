using System;
using System.Windows;
using System.Windows.Input;
using GocDeployManager.Domain.Entities;
using GocDeployManager.UI.Principal;
using GocDeployManager.UI.Ventanas;
using MaterialDesignThemes.Wpf;

namespace GocDeployManager.UI.Login
{
    public partial class LoginWindow : VentanaBase
    {
        private readonly Bootstrapper _bootstrapper;
        private bool _modoPrimerArranque;
        private bool _mostrandoContrasena;

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
            txtContrasenaVisible.Text = string.Empty;
            txtConfirmarContrasena.Password = string.Empty;
            btnAccion.Content = "INGRESAR";
            OcultarContrasena();
        }

        private string ObtenerContrasena()
            => _mostrandoContrasena ? txtContrasenaVisible.Text : txtContrasena.Password;

        private void OcultarContrasena()
        {
            if (!_mostrandoContrasena) return;
            _mostrandoContrasena = false;
            txtContrasena.Visibility = Visibility.Visible;
            txtContrasenaVisible.Visibility = Visibility.Collapsed;
            iconContrasena.Kind = PackIconKind.Eye;
            btnMostrarContrasena.ToolTip = "Mostrar contraseña";
        }

        private void TxtUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            if (_modoPrimerArranque)
                txtNombreVisible.Focus();
            else if (_mostrandoContrasena)
                txtContrasenaVisible.Focus();
            else
                txtContrasena.Focus();
        }

        private void TxtNombreVisible_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            if (_mostrandoContrasena)
                txtContrasenaVisible.Focus();
            else
                txtContrasena.Focus();
        }

        private void TxtContrasenaVisible_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            btnAccion.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
        }

        private void BtnMostrarContrasena_Click(object sender, RoutedEventArgs e)
        {
            _mostrandoContrasena = !_mostrandoContrasena;
            if (_mostrandoContrasena)
            {
                txtContrasenaVisible.Text = txtContrasena.Password;
                txtContrasena.Visibility = Visibility.Collapsed;
                txtContrasenaVisible.Visibility = Visibility.Visible;
                iconContrasena.Kind = PackIconKind.EyeOff;
                btnMostrarContrasena.ToolTip = "Ocultar contraseña";
            }
            else
            {
                txtContrasena.Password = txtContrasenaVisible.Text;
                txtContrasenaVisible.Visibility = Visibility.Collapsed;
                txtContrasena.Visibility = Visibility.Visible;
                iconContrasena.Kind = PackIconKind.Eye;
                btnMostrarContrasena.ToolTip = "Mostrar contraseña";
            }
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
            var clave = ObtenerContrasena();
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
            var clave = ObtenerContrasena();

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
