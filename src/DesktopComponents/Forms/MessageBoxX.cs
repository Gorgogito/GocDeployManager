using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DesktopComponents.Controls;
using DesktopComponents.Theming;

namespace DesktopComponents.Forms
{
    public enum ResultadoMessageBoxX
    {
        Aceptar,
        Cancelar,
        Si,
        No,
    }

    public enum IconoMessageBoxX
    {
        Informacion,
        Exito,
        Advertencia,
        Error,
        Pregunta,
    }

    /// <summary>
    /// Reemplazo de <see cref="MessageBox"/> con el mismo chrome que el resto
    /// de la aplicación (barra de acento por severidad, botones ButtonX).
    /// </summary>
    [DesignerCategory("")]
    public sealed class MessageBoxX : MetroForm
    {
        private readonly IconoMessageBoxX _icono;
        private ResultadoMessageBoxX _resultado = ResultadoMessageBoxX.Cancelar;

        private MessageBoxX(string titulo, string mensaje, IconoMessageBoxX icono, (string Texto, ResultadoMessageBoxX Resultado, VarianteButtonX Variante)[] botones)
        {
            _icono = icono;
            Text = titulo;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            ClientSize = new Size(LogicalToDeviceUnits(420), LogicalToDeviceUnits(190));

            var tema = ThemeManager.Actual;
            var margenIzquierdo = LogicalToDeviceUnits(28);

            var etiquetaMensaje = new Label
            {
                Text = mensaje,
                Location = new Point(margenIzquierdo, LogicalToDeviceUnits(50)),
                Size = new Size(ClientSize.Width - margenIzquierdo - LogicalToDeviceUnits(20), LogicalToDeviceUnits(80)),
                ForeColor = tema.TextoPrimario,
                Font = new Font("Segoe UI", 10f),
            };
            Controls.Add(etiquetaMensaje);

            var x = ClientSize.Width - LogicalToDeviceUnits(20);
            for (var i = botones.Length - 1; i >= 0; i--)
            {
                var (texto, resultado, variante) = botones[i];
                var boton = new ButtonX { Text = texto, Width = LogicalToDeviceUnits(110), Variante = variante };
                x -= boton.Width;
                boton.Location = new Point(x, ClientSize.Height - LogicalToDeviceUnits(46));
                x -= LogicalToDeviceUnits(10);

                boton.Click += (s, e) => { _resultado = resultado; Close(); };
                Controls.Add(boton);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var tema = ThemeManager.Actual;
            var lado = LogicalToDeviceUnits(4);
            var altoBarraTitulo = LogicalToDeviceUnits(36);

            using (var pincel = new SolidBrush(ColorSegunIcono(tema)))
                e.Graphics.FillRectangle(pincel, new Rectangle(0, altoBarraTitulo, lado, Height - altoBarraTitulo));
        }

        private Color ColorSegunIcono(Theme tema)
        {
            switch (_icono)
            {
                case IconoMessageBoxX.Exito: return tema.Exito;
                case IconoMessageBoxX.Error: return tema.Peligro;
                case IconoMessageBoxX.Advertencia: return tema.Advertencia;
                case IconoMessageBoxX.Pregunta: return tema.Acento;
                default: return tema.TextoSecundario;
            }
        }

        public static ResultadoMessageBoxX Mostrar(
            IWin32Window propietario, string titulo, string mensaje, IconoMessageBoxX icono = IconoMessageBoxX.Informacion)
        {
            using (var dialogo = new MessageBoxX(titulo, mensaje, icono,
                new[] { ("Aceptar", ResultadoMessageBoxX.Aceptar, VarianteButtonX.Primario) }))
            {
                dialogo.ShowDialog(propietario);
                return dialogo._resultado;
            }
        }

        public static ResultadoMessageBoxX Preguntar(IWin32Window propietario, string titulo, string mensaje)
        {
            using (var dialogo = new MessageBoxX(titulo, mensaje, IconoMessageBoxX.Pregunta,
                new[]
                {
                    ("Cancelar", ResultadoMessageBoxX.Cancelar, VarianteButtonX.Secundario),
                    ("Sí", ResultadoMessageBoxX.Si, VarianteButtonX.Primario),
                }))
            {
                dialogo.ShowDialog(propietario);
                return dialogo._resultado;
            }
        }
    }
}
