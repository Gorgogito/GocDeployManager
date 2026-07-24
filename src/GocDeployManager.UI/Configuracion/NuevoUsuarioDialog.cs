using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DesktopComponents.Controls;
using DesktopComponents.Forms;
using DesktopComponents.Theming;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.UI.Configuracion
{
    /// <summary>
    /// Alta de un nuevo usuario de la aplicación (sección 15 del análisis).
    /// </summary>
    [DesignerCategory("")]
    public sealed class NuevoUsuarioDialog : MetroForm
    {
        private readonly Bootstrapper _bootstrapper;
        private readonly TextBoxX _txtUsuario;
        private readonly TextBoxX _txtNombreVisible;
        private readonly ComboBoxX _comboRol;
        private readonly TextBoxX _txtContrasena;
        private readonly TextBoxX _txtConfirmar;
        private readonly Label _lblError;

        public NuevoUsuarioDialog(Bootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));

            Text = "Nuevo usuario";
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(LogicalToDeviceUnits(380), LogicalToDeviceUnits(540));

            var tema = ThemeManager.Actual;
            var margen = LogicalToDeviceUnits(28);
            var ancho = ClientSize.Width - margen * 2;
            var altoEtiqueta = LogicalToDeviceUnits(18);
            var salto = LogicalToDeviceUnits(58);
            var y = LogicalToDeviceUnits(48);

            Controls.Add(CrearEtiquetaCampo("Usuario", tema, margen, y, ancho, altoEtiqueta));
            y += altoEtiqueta;
            _txtUsuario = new TextBoxX { TextoMarcador = "Usuario", Location = new Point(margen, y), Width = ancho };
            y += salto;

            Controls.Add(CrearEtiquetaCampo("Nombre visible", tema, margen, y, ancho, altoEtiqueta));
            y += altoEtiqueta;
            _txtNombreVisible = new TextBoxX { TextoMarcador = "Nombre visible", Location = new Point(margen, y), Width = ancho };
            y += salto;

            Controls.Add(CrearEtiquetaCampo("Rol", tema, margen, y, ancho, altoEtiqueta));
            y += altoEtiqueta;
            _comboRol = new ComboBoxX { Location = new Point(margen, y), Width = ancho };
            foreach (RolUsuario rol in Enum.GetValues(typeof(RolUsuario)))
                _comboRol.Items.Add(rol);
            _comboRol.SelectedItem = RolUsuario.Operador;
            y += salto;

            Controls.Add(CrearEtiquetaCampo("Contraseña", tema, margen, y, ancho, altoEtiqueta));
            y += altoEtiqueta;
            _txtContrasena = new TextBoxX { TextoMarcador = "Contraseña", EsPassword = true, Location = new Point(margen, y), Width = ancho };
            y += salto;

            Controls.Add(CrearEtiquetaCampo("Confirmar contraseña", tema, margen, y, ancho, altoEtiqueta));
            y += altoEtiqueta;
            _txtConfirmar = new TextBoxX { TextoMarcador = "Confirmar contraseña", EsPassword = true, Location = new Point(margen, y), Width = ancho };
            y += salto;

            _lblError = new Label
            {
                Location = new Point(margen, y),
                Size = new Size(ancho, LogicalToDeviceUnits(36)),
                ForeColor = tema.Peligro,
            };
            y += LogicalToDeviceUnits(40);

            var btnCrear = new ButtonX { Text = "Crear", Location = new Point(margen, y), Width = ancho, Variante = VarianteButtonX.Primario };
            btnCrear.Click += BtnCrear_Click;

            Controls.Add(_txtUsuario);
            Controls.Add(_txtNombreVisible);
            Controls.Add(_comboRol);
            Controls.Add(_txtContrasena);
            Controls.Add(_txtConfirmar);
            Controls.Add(_lblError);
            Controls.Add(btnCrear);

            AcceptButton = btnCrear;
        }

        private static Label CrearEtiquetaCampo(string texto, Theme tema, int x, int y, int ancho, int alto)
        {
            return new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 8f),
                ForeColor = tema.TextoSecundario,
                AutoSize = false,
                TextAlign = ContentAlignment.BottomLeft,
                Location = new Point(x, y),
                Size = new Size(ancho, alto),
            };
        }

        private void BtnCrear_Click(object sender, EventArgs e)
        {
            _lblError.Text = string.Empty;

            var usuario = _txtUsuario.Text.Trim();
            var nombreVisible = _txtNombreVisible.Text.Trim();
            var clave = _txtContrasena.Text;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(nombreVisible) || string.IsNullOrWhiteSpace(clave))
            {
                _lblError.Text = "Todos los campos son obligatorios.";
                return;
            }

            if (clave != _txtConfirmar.Text)
            {
                _lblError.Text = "Las contraseñas no coinciden.";
                return;
            }

            if (!(_comboRol.SelectedItem is RolUsuario rol))
            {
                _lblError.Text = "Selecciona un rol.";
                return;
            }

            var resultado = _bootstrapper.Usuarios.CrearUsuario(usuario, nombreVisible, rol, clave);
            if (resultado.IsFailure)
            {
                _lblError.Text = resultado.Error;
                return;
            }

            NotificationX.Mostrar("Usuario creado", $"'{usuario}' se creó correctamente.", TipoNotificacion.Exito);
            Close();
        }
    }
}
