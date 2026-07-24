using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// Velo semitransparente + spinner animado, para cubrir la pantalla
    /// principal mientras corre clonado/compilación (sección 9 del análisis).
    /// Se agrega como último control del contenedor (Dock=Fill) y se
    /// muestra/oculta con <see cref="Mostrar"/>/<see cref="Ocultar"/>.
    /// </summary>
    [DesignerCategory("")]
    public sealed class LoadingOverlay : Control, IThemedControl
    {
        private readonly Timer _timerAnimacion;
        private readonly SuscripcionTema _suscripcionTema;
        private float _anguloActual;
        private string _mensaje = "Procesando...";

        public string Mensaje
        {
            get => _mensaje;
            set { _mensaje = value; Invalidate(); }
        }

        public LoadingOverlay()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
                true);

            Dock = DockStyle.Fill;
            Visible = false;
            Font = new Font("Segoe UI", 10f);

            _timerAnimacion = new Timer { Interval = 30 };
            _timerAnimacion.Tick += (s, e) =>
            {
                _anguloActual = (_anguloActual + 8f) % 360f;
                Invalidate();
            };

            _suscripcionTema = new SuscripcionTema(AplicarTema);
        }

        public void Mostrar()
        {
            BringToFront();
            Visible = true;
            _timerAnimacion.Start();
        }

        public void Ocultar()
        {
            _timerAnimacion.Stop();
            Visible = false;
        }

        public void AplicarTema(Theme tema) => Invalidate();

        protected override void OnPaint(PaintEventArgs e)
        {
            var tema = ThemeManager.Actual;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var pincelVelo = new SolidBrush(Color.FromArgb(190, tema.Fondo)))
                g.FillRectangle(pincelVelo, ClientRectangle);

            var lado = LogicalToDeviceUnits(36);
            var rectSpinner = new Rectangle(Width / 2 - lado / 2, Height / 2 - lado / 2 - LogicalToDeviceUnits(20), lado, lado);

            using (var lapiz = new Pen(tema.Acento, LogicalToDeviceUnits(4)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawArc(lapiz, rectSpinner, _anguloActual, 270);

            if (!string.IsNullOrEmpty(_mensaje))
            {
                var rectTexto = new Rectangle(0, rectSpinner.Bottom + LogicalToDeviceUnits(12), Width, LogicalToDeviceUnits(24));
                TextRenderer.DrawText(g, _mensaje, Font, rectTexto, tema.TextoPrimario,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timerAnimacion?.Dispose();
                _suscripcionTema.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
