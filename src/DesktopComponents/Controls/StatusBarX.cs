using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// Barra de estado propia (no envuelve StatusStrip nativo): texto a la
    /// izquierda (etapa actual) y a la derecha (tiempo transcurrido), como
    /// pide la pantalla principal.
    /// </summary>
    [DesignerCategory("")]
    public sealed class StatusBarX : Control, IThemedControl
    {
        private readonly SuscripcionTema _suscripcionTema;
        private string _textoIzquierda;
        private string _textoDerecha;

        public string TextoIzquierda
        {
            get => _textoIzquierda;
            set { _textoIzquierda = value; Invalidate(); }
        }

        public string TextoDerecha
        {
            get => _textoDerecha;
            set { _textoDerecha = value; Invalidate(); }
        }

        public StatusBarX()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
                true);

            Dock = DockStyle.Bottom;
            Height = LogicalToDeviceUnits(28);
            Font = new Font("Segoe UI", 8.5f);

            _suscripcionTema = new SuscripcionTema(AplicarTema);
        }

        public void AplicarTema(Theme tema) => Invalidate();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _suscripcionTema.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var tema = ThemeManager.Actual;

            using (var pincelFondo = new SolidBrush(tema.Superficie))
                e.Graphics.FillRectangle(pincelFondo, ClientRectangle);

            using (var lapizBorde = new Pen(tema.Borde))
                e.Graphics.DrawLine(lapizBorde, 0, 0, Width, 0);

            var margen = LogicalToDeviceUnits(12);
            var mitad = Width / 2;

            if (!string.IsNullOrEmpty(_textoIzquierda))
            {
                TextRenderer.DrawText(
                    e.Graphics, _textoIzquierda, Font,
                    new Rectangle(margen, 0, mitad - margen, Height),
                    tema.TextoSecundario,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }

            if (!string.IsNullOrEmpty(_textoDerecha))
            {
                TextRenderer.DrawText(
                    e.Graphics, _textoDerecha, Font,
                    new Rectangle(mitad, 0, Width - mitad - margen, Height),
                    tema.TextoSecundario,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right |
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }
        }
    }
}
