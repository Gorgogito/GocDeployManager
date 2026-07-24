using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// Franja superior de acciones de la pantalla principal (entrada de GOC,
    /// selección de ambiente/sistema, botón de iniciar despliegue). No intenta
    /// ser un Ribbon de Office completo con pestañas — es un contenedor propio
    /// que acomoda controles de izquierda a derecha con separación consistente.
    /// </summary>
    [DesignerCategory("")]
    public sealed class RibbonBar : Control, IThemedControl
    {
        private readonly int _espaciado;
        private readonly SuscripcionTema _suscripcionTema;
        private int _siguienteX = -1;

        public RibbonBar()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
                true);

            Dock = DockStyle.Top;
            Height = LogicalToDeviceUnits(56);
            Padding = new Padding(LogicalToDeviceUnits(16), LogicalToDeviceUnits(10), LogicalToDeviceUnits(16), LogicalToDeviceUnits(10));
            _espaciado = LogicalToDeviceUnits(12);

            _suscripcionTema = new SuscripcionTema(AplicarTema);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _suscripcionTema.Dispose();

            base.Dispose(disposing);
        }

        public void AgregarControl(Control control)
        {
            if (_siguienteX < 0)
                _siguienteX = Padding.Left;

            control.Location = new Point(_siguienteX, (Height - control.Height) / 2);
            Controls.Add(control);
            _siguienteX += control.Width + _espaciado;
        }

        public void AgregarSeparador(int anchoLogico = 20)
        {
            if (_siguienteX < 0)
                _siguienteX = Padding.Left;

            _siguienteX += LogicalToDeviceUnits(anchoLogico);
        }

        public void AplicarTema(Theme tema)
        {
            BackColor = tema.Superficie;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var tema = ThemeManager.Actual;

            using (var pincelFondo = new SolidBrush(tema.Superficie))
                e.Graphics.FillRectangle(pincelFondo, ClientRectangle);

            using (var lapizBorde = new Pen(tema.Borde))
                e.Graphics.DrawLine(lapizBorde, 0, Height - 1, Width, Height - 1);
        }
    }
}
