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
    /// Credenciales de Bitbucket del operador (sección 15 del análisis): cada
    /// usuario registra las suyas, protegidas con DPAPI atado a su sesión Windows.
    /// </summary>
    [DesignerCategory("")]
    public sealed class CredencialesBitbucketDialog : MetroForm
    {
        private readonly Bootstrapper _bootstrapper;
        private readonly string _nombreUsuario;
        private readonly TextBoxX _txtUsuarioBitbucket;
        private readonly TextBoxX _txtContrasenaBitbucket;
        private readonly Label _lblError;

        public CredencialesBitbucketDialog(Bootstrapper bootstrapper, string nombreUsuario)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            _nombreUsuario = nombreUsuario;

            Text = $"Credenciales de Bitbucket — {nombreUsuario}";
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(LogicalToDeviceUnits(380), LogicalToDeviceUnits(280));

            var tema = ThemeManager.Actual;
            var margen = LogicalToDeviceUnits(28);
            var ancho = ClientSize.Width - margen * 2;
            var altoEtiqueta = LogicalToDeviceUnits(18);
            var salto = LogicalToDeviceUnits(58);
            var y = LogicalToDeviceUnits(48);

            Controls.Add(CrearEtiquetaCampo("Usuario de Bitbucket", tema, margen, y, ancho, altoEtiqueta));
            y += altoEtiqueta;
            _txtUsuarioBitbucket = new TextBoxX { TextoMarcador = "Usuario de Bitbucket", Location = new Point(margen, y), Width = ancho };
            y += salto;

            Controls.Add(CrearEtiquetaCampo("Contraseña de Bitbucket", tema, margen, y, ancho, altoEtiqueta));
            y += altoEtiqueta;
            _txtContrasenaBitbucket = new TextBoxX { TextoMarcador = "Contraseña de Bitbucket", EsPassword = true, Location = new Point(margen, y), Width = ancho };
            y += salto;

            _lblError = new Label
            {
                Location = new Point(margen, y),
                Size = new Size(ancho, LogicalToDeviceUnits(36)),
                ForeColor = tema.Peligro,
            };
            y += LogicalToDeviceUnits(40);

            var btnGuardar = new ButtonX { Text = "Guardar", Location = new Point(margen, y), Width = ancho, Variante = VarianteButtonX.Primario };
            btnGuardar.Click += BtnGuardar_Click;

            Controls.Add(_txtUsuarioBitbucket);
            Controls.Add(_txtContrasenaBitbucket);
            Controls.Add(_lblError);
            Controls.Add(btnGuardar);

            AcceptButton = btnGuardar;
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

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            _lblError.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(_txtUsuarioBitbucket.Text) || string.IsNullOrWhiteSpace(_txtContrasenaBitbucket.Text))
            {
                _lblError.Text = "Usuario y contraseña de Bitbucket son obligatorios.";
                return;
            }

            var resultado = _bootstrapper.Usuarios.EstablecerCredencialesBitbucket(
                _nombreUsuario, _txtUsuarioBitbucket.Text.Trim(), _txtContrasenaBitbucket.Text);

            if (resultado.IsFailure)
            {
                _lblError.Text = resultado.Error;
                return;
            }

            NotificationX.Mostrar(
                "Credenciales guardadas", $"Se guardaron las credenciales de Bitbucket de '{_nombreUsuario}'.", TipoNotificacion.Exito);
            Close();
        }
    }
}
