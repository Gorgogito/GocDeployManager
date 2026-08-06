using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
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
                if (nuevo == _valor) return;
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

            Height = LogicalToDeviceUnits(8);
            _radio = LogicalToDeviceUnits(4);

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
            var g    = e.Graphics;
            GraficosX.PrepararAlta(g);

            var pista = new Rectangle(0, 0, Width - 1, Height - 1);

            using (var rutaPista = Dibujo.RutaRedondeada(pista, _radio))
            {
                // Pista de fondo
                using (var pincelPista = new SolidBrush(Theme.Mezclar(tema.Acento, tema.Superficie, 0.82f)))
                    g.FillPath(pincelPista, rutaPista);

                var rango      = Math.Max(1, _maximo - Minimo);
                var proporcion = Math.Max(0f, Math.Min(1f, (float)(_valor - Minimo) / rango));
                var anchoRelleno = (int)(Width * proporcion);

                if (anchoRelleno > 0)
                {
                    var estadoClip = g.Save();
                    g.SetClip(rutaPista);
                    using (var pincelRelleno = new SolidBrush(tema.Acento))
                        g.FillRectangle(pincelRelleno, new Rectangle(0, 0, anchoRelleno, Height));
                    g.Restore(estadoClip);
                }
            }
        }
    }
}
