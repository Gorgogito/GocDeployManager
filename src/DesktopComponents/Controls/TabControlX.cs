using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// Control de pestañas propio: hereda de Panel y pinta la franja de
    /// pestañas a mano. La pestaña activa tiene fondo elevado + indicador
    /// de 3px en color acento; el hover usa fade suave.
    /// </summary>
    [DesignerCategory("")]
    public sealed class TabControlX : Panel, IThemedControl
    {
        private readonly List<(string Titulo, Panel Contenido)> _paginas = new List<(string, Panel)>();
        private readonly List<Rectangle> _rects = new List<Rectangle>();
        private readonly int _altoPestanas;
        private readonly SuscripcionTema _suscripcionTema;
        private int _indiceSeleccionado = -1;
        private int _indiceHover        = -1;

        public event EventHandler PaginaCambiada;

        public int IndiceSeleccionado => _indiceSeleccionado;

        public TabControlX()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
                true);

            Font          = new Font("Segoe UI", 9.5f);
            _altoPestanas = LogicalToDeviceUnits(40);
            Padding       = new Padding(0, _altoPestanas, 0, 0);

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
            contenido.Dock    = DockStyle.Fill;
            contenido.Visible = _paginas.Count == 0;
            Controls.Add(contenido);
            _paginas.Add((titulo, contenido));
            if (_indiceSeleccionado == -1)
                _indiceSeleccionado = 0;
            Invalidate();
        }

        public void SeleccionarPagina(int indice)
        {
            if (indice < 0 || indice >= _paginas.Count || indice == _indiceSeleccionado) return;
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
            var g    = e.Graphics;
            GraficosX.PrepararAlta(g);

            // Fondo de la franja de pestañas.
            using (var pincelFondo = new SolidBrush(tema.Fondo))
                g.FillRectangle(pincelFondo, new Rectangle(0, 0, Width, _altoPestanas));

            _rects.Clear();
            var x = LogicalToDeviceUnits(6);

            for (var i = 0; i < _paginas.Count; i++)
            {
                var seleccionada = i == _indiceSeleccionado;
                var hover        = i == _indiceHover;

                var fuenteTab    = seleccionada ? new Font(Font, FontStyle.Bold) : Font;
                var ancho        = TextRenderer.MeasureText(g, _paginas[i].Titulo, fuenteTab).Width
                                   + LogicalToDeviceUnits(32);
                var rect         = new Rectangle(x, 0, ancho, _altoPestanas);
                _rects.Add(rect);

                // Fondo de la pestaña activa — superficie más clara que el fondo.
                if (seleccionada)
                {
                    using (var pincel = new SolidBrush(tema.Superficie))
                        g.FillRectangle(pincel, rect);
                }
                else if (hover)
                {
                    using (var pincel = new SolidBrush(Theme.Mezclar(tema.Fondo, tema.TextoPrimario, 0.05f)))
                        g.FillRectangle(pincel, rect);
                }

                // Indicador de 3px en la base de la pestaña activa.
                if (seleccionada)
                {
                    var padIndicador = LogicalToDeviceUnits(8);
                    using (var pincelInd = new SolidBrush(tema.Acento))
                        g.FillRectangle(pincelInd,
                            new Rectangle(rect.Left + padIndicador, rect.Bottom - LogicalToDeviceUnits(3),
                                          rect.Width - padIndicador * 2, LogicalToDeviceUnits(3)));
                }

                var colorTexto = seleccionada ? tema.Acento : tema.TextoSecundario;
                TextRenderer.DrawText(g, _paginas[i].Titulo, fuenteTab, rect, colorTexto,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

                if (seleccionada)
                    fuenteTab.Dispose();

                x = rect.Right;
            }

            // Línea separadora debajo de la franja.
            using (var lapiz = new Pen(tema.BordereReposo))
                g.DrawLine(lapiz, 0, _altoPestanas - 1, Width, _altoPestanas - 1);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var nuevo = -1;
            for (var i = 0; i < _rects.Count; i++)
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
            for (var i = 0; i < _rects.Count; i++)
                if (_rects[i].Contains(e.Location)) { SeleccionarPagina(i); break; }
        }
    }
}
