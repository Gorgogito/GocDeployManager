using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
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

            Dock   = DockStyle.Bottom;
            Height = LogicalToDeviceUnits(28);
            Font   = new Font("Segoe UI", 8.5f);

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
            var tema   = ThemeManager.Actual;
            var g      = e.Graphics;
            var margen = LogicalToDeviceUnits(12);
            var mitad  = Width / 2;

            using (var pincelFondo = new SolidBrush(tema.SuperficieElevada))
                g.FillRectangle(pincelFondo, ClientRectangle);

            // Borde superior.
            using (var lapizBorde = new Pen(tema.BordereReposo))
                g.DrawLine(lapizBorde, 0, 0, Width, 0);

            // Barra de acento de 2 dp en el extremo izquierdo.
            using (var pincelAcento = new SolidBrush(tema.Acento))
                g.FillRectangle(pincelAcento, new Rectangle(0, 0, LogicalToDeviceUnits(2), Height));

            if (!string.IsNullOrEmpty(_textoIzquierda))
            {
                TextRenderer.DrawText(
                    g, _textoIzquierda, Font,
                    new Rectangle(margen + LogicalToDeviceUnits(4), 0, mitad - margen, Height),
                    tema.TextoSecundario,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }

            if (!string.IsNullOrEmpty(_textoDerecha))
            {
                TextRenderer.DrawText(
                    g, _textoDerecha, Font,
                    new Rectangle(mitad, 0, Width - mitad - margen, Height),
                    tema.TextoSecundario,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right |
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }
        }
    }
}
