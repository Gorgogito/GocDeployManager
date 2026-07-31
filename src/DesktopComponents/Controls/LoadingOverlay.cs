using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// Insignia flotante con spinner animado y mensaje, para indicar que
    /// corre clonado/compilación (sección 9 del análisis). Se agrega como
    /// último control del contenedor (Dock=Fill) y se muestra/oculta con
    /// <see cref="Mostrar"/>/<see cref="Ocultar"/>.
    ///
    /// A diferencia de la versión original, NO cubre todo el contenedor con
    /// un velo — ese contenedor incluye el panel de salida en tiempo real del
    /// despliegue, y un velo (aunque sea semitransparente) lo vuelve
    /// ilegible. En vez de simular transparencia (WinForms no compone bien
    /// el contenido de un control hermano detrás de otro), se recorta la
    /// forma del control con <see cref="Control.Region"/> a un pequeño
    /// recuadro alrededor del spinner+mensaje — fuera de ese recuadro el
    /// control directamente no ocupa espacio de pantalla, así que el panel
    /// de abajo se ve sin ningún truco de composición. El bloqueo real de la
    /// interacción (botón, campos) lo hace <c>ConmutarControlesDurante</c> en
    /// MainForm, no este control.
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
            ActualizarRegion();
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ActualizarRegion();
        }

        /// <summary>
        /// Recorta el control a un recuadro chico centrado — el resto del
        /// área (Dock=Fill, del tamaño del panel de salida completo) queda
        /// fuera de la forma del control y no se pinta, dejando ver lo que
        /// haya debajo sin necesidad de transparencia real.
        /// </summary>
        private void ActualizarRegion()
        {
            if (Width <= 0 || Height <= 0)
                return;

            Region = new Region(ObtenerRectInsignia());
        }

        private Rectangle ObtenerRectInsignia()
        {
            var ancho = Math.Min(Width, LogicalToDeviceUnits(260));
            var alto = Math.Min(Height, LogicalToDeviceUnits(96));
            return new Rectangle(Width / 2 - ancho / 2, Height / 2 - alto / 2, ancho, alto);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var tema = ThemeManager.Actual;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rectInsignia = ObtenerRectInsignia();

            using (var pincelFondo = new SolidBrush(tema.Superficie))
                g.FillRectangle(pincelFondo, rectInsignia);
            using (var lapizBorde = new Pen(tema.Borde))
                g.DrawRectangle(lapizBorde, rectInsignia.X, rectInsignia.Y, rectInsignia.Width - 1, rectInsignia.Height - 1);

            var lado = LogicalToDeviceUnits(36);
            var rectSpinner = new Rectangle(Width / 2 - lado / 2, rectInsignia.Y + LogicalToDeviceUnits(14), lado, lado);

            using (var lapiz = new Pen(tema.Acento, LogicalToDeviceUnits(4)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawArc(lapiz, rectSpinner, _anguloActual, 270);

            if (!string.IsNullOrEmpty(_mensaje))
            {
                var rectTexto = new Rectangle(rectInsignia.X, rectSpinner.Bottom + LogicalToDeviceUnits(8), rectInsignia.Width, LogicalToDeviceUnits(24));
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
