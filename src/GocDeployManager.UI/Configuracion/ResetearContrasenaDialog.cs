using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DesktopComponents.Controls;
using DesktopComponents.Forms;
using DesktopComponents.Theming;

namespace GocDeployManager.UI.Configuracion
{
    /// <summary>
    /// Reseteo de contraseña por un administrador (política simple, confirmada
    /// con el cliente: sin expiración ni recuperación, solo reseteo manual).
    /// </summary>
    [DesignerCategory("")]
    public sealed class ResetearContrasenaDialog : MetroForm
    {
        private readonly Bootstrapper _bootstrapper;
        private readonly string _nombreUsuario;
        private readonly TextBoxX _txtContrasena;
        private readonly TextBoxX _txtConfirmar;
        private readonly Label _lblError;

        public ResetearContrasenaDialog(Bootstrapper bootstrapper, string nombreUsuario)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            _nombreUsuario = nombreUsuario;

            Text = $"Resetear contraseña — {nombreUsuario}";
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(LogicalToDeviceUnits(380), LogicalToDeviceUnits(280));

            var tema = ThemeManager.Actual;
            var margen = LogicalToDeviceUnits(28);
            var ancho = ClientSize.Width - margen * 2;
            var altoEtiqueta = LogicalToDeviceUnits(18);
            var salto = LogicalToDeviceUnits(58);
            var y = LogicalToDeviceUnits(48);

            Controls.Add(CrearEtiquetaCampo("Nueva contraseña", tema, margen, y, ancho, altoEtiqueta));
            y += altoEtiqueta;
            _txtContrasena = new TextBoxX { TextoMarcador = "Nueva contraseña", EsPassword = true, Location = new Point(margen, y), Width = ancho };
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

            var btnResetear = new ButtonX { Text = "Resetear", Location = new Point(margen, y), Width = ancho, Variante = VarianteButtonX.Primario };
            btnResetear.Click += BtnResetear_Click;

            Controls.Add(_txtContrasena);
            Controls.Add(_txtConfirmar);
            Controls.Add(_lblError);
            Controls.Add(btnResetear);

            AcceptButton = btnResetear;
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

        private void BtnResetear_Click(object sender, EventArgs e)
        {
            _lblError.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(_txtContrasena.Text))
            {
                _lblError.Text = "La contraseña es obligatoria.";
                return;
            }

            if (_txtContrasena.Text != _txtConfirmar.Text)
            {
                _lblError.Text = "Las contraseñas no coinciden.";
                return;
            }

            var resultado = _bootstrapper.Usuarios.ResetearContrasena(_nombreUsuario, _txtContrasena.Text);
            if (resultado.IsFailure)
            {
                _lblError.Text = resultado.Error;
                return;
            }

            NotificationX.Mostrar("Contraseña reseteada", $"Se actualizó la contraseña de '{_nombreUsuario}'.", TipoNotificacion.Exito);
            Close();
        }
    }
}
