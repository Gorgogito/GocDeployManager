using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DesktopComponents.Controls;
using DesktopComponents.Forms;
using DesktopComponents.Theming;
using GocDeployManager.Domain.Entities;
using GocDeployManager.UI.Principal;

namespace GocDeployManager.UI.Login
{
    /// <summary>
    /// Pantalla de entrada (sección 10 del análisis). Si todavía no existe
    /// ningún usuario en la base, se convierte en el flujo de alta del primer
    /// administrador — no hay otra forma de entrar a la app en ese caso.
    /// </summary>
    [DesignerCategory("")]
    public sealed class LoginForm : MetroForm
    {
        private readonly Bootstrapper _bootstrapper;

        private readonly Label _lblTitulo;
        private readonly Label _lblUsuario;
        private readonly TextBoxX _txtUsuario;
        private readonly Label _lblNombreVisible;
        private readonly TextBoxX _txtNombreVisible;
        private readonly Label _lblContrasena;
        private readonly TextBoxX _txtContrasena;
        private readonly Label _lblConfirmarContrasena;
        private readonly TextBoxX _txtConfirmarContrasena;
        private readonly Label _lblError;
        private readonly ButtonX _btnAccion;

        private bool _modoPrimerArranque;

        public LoginForm(Bootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));

            var tema = ThemeManager.Actual;

            Text = "GocDeployManager — Iniciar sesión";
            ClientSize = new Size(LogicalToDeviceUnits(380), LogicalToDeviceUnits(480));
            StartPosition = FormStartPosition.CenterScreen;
            MinimizeBox = false;

            _lblTitulo = new Label
            {
                Text = "Iniciar sesión",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = tema.TextoPrimario,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
            };

            _lblUsuario = CrearEtiquetaCampo("Usuario", tema);
            _txtUsuario = new TextBoxX { TextoMarcador = "Usuario" };

            _lblNombreVisible = CrearEtiquetaCampo("Nombre visible", tema, visible: false);
            _txtNombreVisible = new TextBoxX { TextoMarcador = "Nombre visible", Visible = false };

            _lblContrasena = CrearEtiquetaCampo("Contraseña", tema);
            _txtContrasena = new TextBoxX { TextoMarcador = "Contraseña", EsPassword = true };

            _lblConfirmarContrasena = CrearEtiquetaCampo("Confirmar contraseña", tema, visible: false);
            _txtConfirmarContrasena = new TextBoxX { TextoMarcador = "Confirmar contraseña", EsPassword = true, Visible = false };

            _lblError = new Label
            {
                ForeColor = tema.Peligro,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft,
            };

            _btnAccion = new ButtonX { Text = "Ingresar", Variante = VarianteButtonX.Primario };
            _btnAccion.Click += BtnAccion_Click;

            Controls.Add(_lblTitulo);
            Controls.Add(_lblUsuario);
            Controls.Add(_txtUsuario);
            Controls.Add(_lblNombreVisible);
            Controls.Add(_txtNombreVisible);
            Controls.Add(_lblContrasena);
            Controls.Add(_txtContrasena);
            Controls.Add(_lblConfirmarContrasena);
            Controls.Add(_txtConfirmarContrasena);
            Controls.Add(_lblError);
            Controls.Add(_btnAccion);

            AcceptButton = _btnAccion;
            ReubicarControles();

            Load += LoginForm_Load;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            if (_bootstrapper.Usuarios.ObtenerTodos().Count == 0)
                EntrarModoPrimerArranque();
        }

        private void EntrarModoPrimerArranque()
        {
            _modoPrimerArranque = true;
            _lblTitulo.Text = "Crear usuario administrador";
            _lblNombreVisible.Visible = true;
            _txtNombreVisible.Visible = true;
            _lblConfirmarContrasena.Visible = true;
            _txtConfirmarContrasena.Visible = true;
            _btnAccion.Text = "Crear y continuar";
            ReubicarControles();
        }

        private void VolverAModoLogin()
        {
            _modoPrimerArranque = false;
            _lblTitulo.Text = "Iniciar sesión";
            _lblNombreVisible.Visible = false;
            _txtNombreVisible.Visible = false;
            _lblConfirmarContrasena.Visible = false;
            _txtConfirmarContrasena.Visible = false;
            _txtNombreVisible.Text = string.Empty;
            _txtConfirmarContrasena.Text = string.Empty;
            _txtContrasena.Text = string.Empty;
            _btnAccion.Text = "Ingresar";
            ReubicarControles();
        }

        private static Label CrearEtiquetaCampo(string texto, Theme tema, bool visible = true)
        {
            return new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 8f),
                ForeColor = tema.TextoSecundario,
                AutoSize = false,
                TextAlign = ContentAlignment.BottomLeft,
                Visible = visible,
            };
        }

        private void ReubicarControles()
        {
            var margen = LogicalToDeviceUnits(28);
            var anchoCampo = ClientSize.Width - margen * 2;
            var altoEtiqueta = LogicalToDeviceUnits(18);
            var salto = LogicalToDeviceUnits(58);

            // MetroForm pinta su propia barra de título en y: 0..36 lógicos;
            // todo lo demás debe empezar después de eso o se superpone con
            // el texto nativo de la ventana.
            var y = LogicalToDeviceUnits(48);

            _lblTitulo.Location = new Point(margen, y);
            _lblTitulo.Size = new Size(anchoCampo, LogicalToDeviceUnits(28));
            y += LogicalToDeviceUnits(38);

            _lblUsuario.Location = new Point(margen, y);
            _lblUsuario.Size = new Size(anchoCampo, altoEtiqueta);
            y += altoEtiqueta;
            _txtUsuario.Location = new Point(margen, y);
            _txtUsuario.Width = anchoCampo;
            y += salto;

            if (_txtNombreVisible.Visible)
            {
                _lblNombreVisible.Location = new Point(margen, y);
                _lblNombreVisible.Size = new Size(anchoCampo, altoEtiqueta);
                y += altoEtiqueta;
                _txtNombreVisible.Location = new Point(margen, y);
                _txtNombreVisible.Width = anchoCampo;
                y += salto;
            }

            _lblContrasena.Location = new Point(margen, y);
            _lblContrasena.Size = new Size(anchoCampo, altoEtiqueta);
            y += altoEtiqueta;
            _txtContrasena.Location = new Point(margen, y);
            _txtContrasena.Width = anchoCampo;
            y += salto;

            if (_txtConfirmarContrasena.Visible)
            {
                _lblConfirmarContrasena.Location = new Point(margen, y);
                _lblConfirmarContrasena.Size = new Size(anchoCampo, altoEtiqueta);
                y += altoEtiqueta;
                _txtConfirmarContrasena.Location = new Point(margen, y);
                _txtConfirmarContrasena.Width = anchoCampo;
                y += salto;
            }

            _lblError.Location = new Point(margen, y);
            _lblError.Size = new Size(anchoCampo, LogicalToDeviceUnits(36));
            y += LogicalToDeviceUnits(40);

            _btnAccion.Location = new Point(margen, y);
            _btnAccion.Width = anchoCampo;
        }

        private void BtnAccion_Click(object sender, EventArgs e)
        {
            _lblError.Text = string.Empty;

            if (_modoPrimerArranque)
                CrearPrimerAdministrador();
            else
                IniciarSesion();
        }

        private void CrearPrimerAdministrador()
        {
            var usuario = _txtUsuario.Text.Trim();
            var nombreVisible = _txtNombreVisible.Text.Trim();
            var clave = _txtContrasena.Text;
            var confirmar = _txtConfirmarContrasena.Text;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(nombreVisible) || string.IsNullOrWhiteSpace(clave))
            {
                _lblError.Text = "Todos los campos son obligatorios.";
                return;
            }

            if (clave != confirmar)
            {
                _lblError.Text = "Las contraseñas no coinciden.";
                return;
            }

            var resultado = _bootstrapper.Usuarios.CrearUsuario(usuario, nombreVisible, RolUsuario.Administrador, clave);
            if (resultado.IsFailure)
            {
                _lblError.Text = resultado.Error;
                return;
            }

            NotificationX.Mostrar(
                "Usuario creado",
                $"Se creó el usuario administrador '{usuario}'. Ahora puedes iniciar sesión.",
                TipoNotificacion.Exito);

            VolverAModoLogin();
            _txtUsuario.Text = usuario;
        }

        private void IniciarSesion()
        {
            var usuario = _txtUsuario.Text.Trim();
            var clave = _txtContrasena.Text;

            var resultado = _bootstrapper.Autenticacion.IniciarSesion(usuario, clave);
            if (resultado.IsFailure)
            {
                _lblError.Text = resultado.Error;
                return;
            }

            var sesion = resultado.Value;

            Hide();
            var main = new MainForm(_bootstrapper, sesion);
            main.FormClosed += (s, args) => Close();
            main.Show();
        }
    }
}
