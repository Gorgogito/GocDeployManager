using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// Control de pestañas propio: hereda de Panel (control liviano, no nativo
    /// como System.Windows.Forms.TabControl) y pinta la franja de pestañas a
    /// mano, alternando la visibilidad de los paneles de contenido agregados.
    /// Evita deliberadamente el mismo riesgo de theming que tuvo ComboBoxX.
    /// </summary>
    [DesignerCategory("")]
    public sealed class TabControlX : Panel, IThemedControl
    {
        private readonly List<(string Titulo, Panel Contenido)> _paginas = new List<(string, Panel)>();
        private readonly List<Rectangle> _rects = new List<Rectangle>();
        private readonly int _altoPestanas;
        private readonly SuscripcionTema _suscripcionTema;
        private int _indiceSeleccionado = -1;
        private int _indiceHover = -1;

        public event EventHandler PaginaCambiada;

        public int IndiceSeleccionado => _indiceSeleccionado;

        public TabControlX()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
                true);

            Font = new Font("Segoe UI", 9.5f);
            _altoPestanas = LogicalToDeviceUnits(38);
            Padding = new Padding(0, _altoPestanas, 0, 0);

            _suscripcionTema = new SuscripcionTema(AplicarTema);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _suscripcionTema.Dispose();

            base.Dispose(disposing);
        }

        public void AgregarPagina(string titulo, Panel contenido)
        {
            contenido.Dock = DockStyle.Fill;
            contenido.Visible = _paginas.Count == 0;
            Controls.Add(contenido);
            _paginas.Add((titulo, contenido));

            if (_indiceSeleccionado == -1)
                _indiceSeleccionado = 0;

            Invalidate();
        }

        public void SeleccionarPagina(int indice)
        {
            if (indice < 0 || indice >= _paginas.Count || indice == _indiceSeleccionado)
                return;

            _paginas[_indiceSeleccionado].Contenido.Visible = false;
            _indiceSeleccionado = indice;
            _paginas[_indiceSeleccionado].Contenido.Visible = true;

            Invalidate();
            PaginaCambiada?.Invoke(this, EventArgs.Empty);
        }

        public void AplicarTema(Theme tema)
        {
            BackColor = tema.Fondo;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var tema = ThemeManager.Actual;
            var g = e.Graphics;

            using (var pincelFondo = new SolidBrush(tema.Fondo))
                g.FillRectangle(pincelFondo, new Rectangle(0, 0, Width, _altoPestanas));

            _rects.Clear();
            var x = LogicalToDeviceUnits(4);

            for (var i = 0; i < _paginas.Count; i++)
            {
                var ancho = TextRenderer.MeasureText(g, _paginas[i].Titulo, Font).Width + LogicalToDeviceUnits(28);
                var rect = new Rectangle(x, 0, ancho, _altoPestanas);
                _rects.Add(rect);

                var seleccionada = i == _indiceSeleccionado;
                var hover = i == _indiceHover;

                if (seleccionada)
                {
                    using (var pincel = new SolidBrush(tema.Superficie))
                        g.FillRectangle(pincel, rect);

                    using (var lapiz = new Pen(tema.Acento, 2f))
                        g.DrawLine(lapiz, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
                }
                else if (hover)
                {
                    using (var pincel = new SolidBrush(Theme.Mezclar(tema.Fondo, tema.TextoPrimario, 0.05f)))
                        g.FillRectangle(pincel, rect);
                }

                var colorTexto = seleccionada ? tema.Acento : tema.TextoSecundario;
                TextRenderer.DrawText(g, _paginas[i].Titulo, Font, rect, colorTexto,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

                x = rect.Right;
            }

            using (var lapizBorde = new Pen(tema.Borde))
                g.DrawLine(lapizBorde, 0, _altoPestanas - 1, Width, _altoPestanas - 1);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var nuevo = -1;
            for (var i = 0; i < _rects.Count; i++)
            {
                if (_rects[i].Contains(e.Location))
                {
                    nuevo = i;
                    break;
                }
            }

            if (nuevo != _indiceHover)
            {
                _indiceHover = nuevo;
                Invalidate();
            }
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

            for (var i = 0; i < _rects.Count; i++)
            {
                if (_rects[i].Contains(e.Location))
                {
                    SeleccionarPagina(i);
                    break;
                }
            }
        }
    }
}
