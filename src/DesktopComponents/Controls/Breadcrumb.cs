using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// Ruta de ubicación dentro de Configuración (ej. "Configuración › Bitbucket").
    /// El último segmento no es clicable (es la pantalla actual).
    /// </summary>
    [DesignerCategory("")]
    public sealed class Breadcrumb : Control, IThemedControl
    {
        private readonly List<string> _segmentos = new List<string>();
        private readonly List<Rectangle> _rects  = new List<Rectangle>();
        private readonly SuscripcionTema _suscripcionTema;
        private int _indiceHover = -1;

        public event EventHandler<int> SegmentoClicked;

        public Breadcrumb()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
                true);

            Height = LogicalToDeviceUnits(30);
            Font   = new Font("Segoe UI", 9f);

            _suscripcionTema = new SuscripcionTema(AplicarTema);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _suscripcionTema.Dispose();
            base.Dispose(disposing);
        }

        public void EstablecerRuta(params string[] segmentos)
        {
            _segmentos.Clear();
            _segmentos.AddRange(segmentos);
            Invalidate();
        }

        public void AplicarTema(Theme tema)
        {
            BackColor = tema.SuperficieElevada;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var tema = ThemeManager.Actual;
            var g    = e.Graphics;
            _rects.Clear();

            using (var pincelFondo = new SolidBrush(tema.SuperficieElevada))
                g.FillRectangle(pincelFondo, ClientRectangle);

            var padIzq = LogicalToDeviceUnits(12);
            var x = padIzq;

            for (var i = 0; i < _segmentos.Count; i++)
            {
                var esUltimo = i == _segmentos.Count - 1;
                var texto    = _segmentos[i];
                var tamano   = TextRenderer.MeasureText(g, texto, Font);
                var rect     = new Rectangle(x, 0, tamano.Width, Height);
                _rects.Add(rect);

                var hover = !esUltimo && i == _indiceHover;
                var color = esUltimo
                    ? tema.TextoPrimario
                    : (hover ? tema.Acento : tema.TextoSecundario);

                TextRenderer.DrawText(g, texto, Font, rect, color,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

                if (hover)
                {
                    using (var lapiz = new Pen(tema.Acento))
                        g.DrawLine(lapiz, rect.Left, rect.Bottom - 2, rect.Right - 2, rect.Bottom - 2);
                }

                x = rect.Right + LogicalToDeviceUnits(6);

                if (!esUltimo)
                {
                    const string separador = "›";
                    var tamSep = TextRenderer.MeasureText(g, separador, Font);
                    TextRenderer.DrawText(g, separador, Font,
                        new Rectangle(x, 0, tamSep.Width, Height),
                        tema.TextoSecundario, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                    x += tamSep.Width + LogicalToDeviceUnits(6);
                }
            }

            // Línea inferior que conecta visualmente con el TabControlX.
            using (var lapiz = new Pen(tema.BordereReposo))
                g.DrawLine(lapiz, 0, Height - 1, Width, Height - 1);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var nuevo = -1;
            for (var i = 0; i < _rects.Count - 1; i++)
                if (_rects[i].Contains(e.Location)) { nuevo = i; break; }
            if (nuevo != _indiceHover) { _indiceHover = nuevo; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _indiceHover = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_indiceHover >= 0)
                SegmentoClicked?.Invoke(this, _indiceHover);
        }
    }
}
