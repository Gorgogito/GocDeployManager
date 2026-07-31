using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// Barra de progreso completamente pintada a mano — deliberadamente NO
    /// hereda de <see cref="ProgressBar"/> (control nativo cuyo relleno lo
    /// dibuja el tema visual de Windows, igual que le pasaba a ComboBox con
    /// UserPaint) sino de <see cref="Control"/>, para tener control total del
    /// color y evitar ese mismo problema.
    /// </summary>
    [DesignerCategory("")]
    public sealed class ProgressBarX : Control, IThemedControl
    {
        private readonly int _radio;
        private readonly SuscripcionTema _suscripcionTema;
        private int _valor;
        private int _maximo = 100;

        public int Minimo { get; set; }

        public int Maximo
        {
            get => _maximo;
            set { _maximo = Math.Max(Minimo + 1, value); Invalidate(); }
        }

        public int Valor
        {
            get => _valor;
            set
            {
                var nuevo = Math.Max(Minimo, Math.Min(_maximo, value));
                if (nuevo == _valor)
                    return;

                _valor = nuevo;
                Invalidate();
            }
        }

        public ProgressBarX()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
                true);

            Height = LogicalToDeviceUnits(10);
            _radio = LogicalToDeviceUnits(5);

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
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var pista = new Rectangle(0, 0, Width - 1, Height - 1);

            using (var rutaPista = Dibujo.RutaRedondeada(pista, _radio))
            {
                using (var pincelPista = new SolidBrush(tema.Borde))
                    g.FillPath(pincelPista, rutaPista);

                var rango = Math.Max(1, _maximo - Minimo);
                var proporcion = Math.Max(0f, Math.Min(1f, (float)(_valor - Minimo) / rango));
                var anchoRelleno = (int)(Width * proporcion);

                if (anchoRelleno > 0)
                {
                    var estadoClip = g.Save();
                    g.SetClip(rutaPista);

                    var rectRelleno = new Rectangle(0, 0, anchoRelleno, Height);
                    using (var pincelRelleno = Dibujo.PincelDegradado(
                        rectRelleno, Theme.Mezclar(tema.Acento, Color.White, 0.25f), tema.Acento))
                    {
                        g.FillRectangle(pincelRelleno, rectRelleno);
                    }

                    g.Restore(estadoClip);
                }
            }
        }
    }
}
