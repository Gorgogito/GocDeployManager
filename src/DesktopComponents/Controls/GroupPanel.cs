using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// Panel con borde redondeado y título opcional, usado para agrupar
    /// secciones en las pantallas de configuración.
    /// </summary>
    [DesignerCategory("")]
    public sealed class GroupPanel : Panel, IThemedControl
    {
        private readonly int _radio;
        private readonly int _altoTitulo;
        private readonly SuscripcionTema _suscripcionTema;
        private string _titulo;

        public string Titulo
        {
            get => _titulo;
            set { _titulo = value; Invalidate(); }
        }

        public GroupPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
                true);

            _radio = LogicalToDeviceUnits(8);
            _altoTitulo = LogicalToDeviceUnits(30);
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            Padding = new Padding(
                LogicalToDeviceUnits(14), _altoTitulo + LogicalToDeviceUnits(6),
                LogicalToDeviceUnits(14), LogicalToDeviceUnits(14));

            _suscripcionTema = new SuscripcionTema(AplicarTema);
        }

        public void AplicarTema(Theme tema)
        {
            BackColor = tema.Superficie;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _suscripcionTema.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var tema = ThemeManager.Actual;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var ruta = Dibujo.RutaRedondeada(rect, _radio))
            using (var pincelFondo = new SolidBrush(tema.Superficie))
            using (var lapizBorde = new Pen(tema.Borde))
            {
                e.Graphics.FillPath(pincelFondo, ruta);
                e.Graphics.DrawPath(lapizBorde, ruta);
            }

            if (!string.IsNullOrEmpty(_titulo))
            {
                TextRenderer.DrawText(
                    e.Graphics, _titulo, Font,
                    new Rectangle(LogicalToDeviceUnits(14), 0, Width - LogicalToDeviceUnits(28), _altoTitulo),
                    tema.TextoPrimario,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }
        }
    }
}
