using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// Menú de navegación lateral (Principal / Configuración / Historial),
    /// pintado a mano — no envuelve ningún control nativo, así que no arrastra
    /// el riesgo de theming que tuvo ComboBoxX.
    /// </summary>
    [DesignerCategory("")]
    public sealed class SideMenu : Control, IThemedControl
    {
        public sealed class ItemMenu
        {
            public string Texto { get; }
            public object Etiqueta { get; }

            public ItemMenu(string texto, object etiqueta = null)
            {
                Texto = texto;
                Etiqueta = etiqueta;
            }
        }

        private readonly List<ItemMenu> _items = new List<ItemMenu>();
        private readonly int _altoItem;
        private readonly SuscripcionTema _suscripcionTema;
        private int _indiceSeleccionado = -1;
        private int _indiceHover = -1;

        public event EventHandler<ItemMenu> ItemSeleccionado;

        public IReadOnlyList<ItemMenu> Items => _items;

        public int IndiceSeleccionado
        {
            get => _indiceSeleccionado;
            set { _indiceSeleccionado = value; Invalidate(); }
        }

        public SideMenu()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
                true);

            Dock = DockStyle.Left;
            Width = LogicalToDeviceUnits(180);
            Font = new Font("Segoe UI", 9.5f);
            _altoItem = LogicalToDeviceUnits(42);

            _suscripcionTema = new SuscripcionTema(AplicarTema);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _suscripcionTema.Dispose();

            base.Dispose(disposing);
        }

        public void AgregarItem(string texto, object etiqueta = null)
        {
            _items.Add(new ItemMenu(texto, etiqueta));

            if (_indiceSeleccionado == -1)
                _indiceSeleccionado = 0;

            Invalidate();
        }

        public void AplicarTema(Theme tema)
        {
            BackColor = tema.Superficie;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var tema = ThemeManager.Actual;
            var g = e.Graphics;

            using (var pincelFondo = new SolidBrush(tema.Superficie))
                g.FillRectangle(pincelFondo, ClientRectangle);

            using (var lapizBorde = new Pen(tema.Borde))
                g.DrawLine(lapizBorde, Width - 1, 0, Width - 1, Height);

            for (var i = 0; i < _items.Count; i++)
            {
                var rectItem = new Rectangle(0, i * _altoItem, Width, _altoItem);
                var seleccionado = i == _indiceSeleccionado;
                var hover = i == _indiceHover;

                if (seleccionado)
                {
                    using (var pincelFondoSel = new SolidBrush(Theme.Mezclar(tema.Acento, tema.Superficie, 0.85f)))
                        g.FillRectangle(pincelFondoSel, rectItem);

                    using (var pincelBarra = new SolidBrush(tema.Acento))
                        g.FillRectangle(pincelBarra, new Rectangle(0, rectItem.Y, LogicalToDeviceUnits(3), _altoItem));
                }
                else if (hover)
                {
                    using (var pincelHover = new SolidBrush(Theme.Mezclar(tema.Superficie, tema.TextoPrimario, 0.05f)))
                        g.FillRectangle(pincelHover, rectItem);
                }

                var colorTexto = seleccionado ? tema.Acento : tema.TextoPrimario;
                TextRenderer.DrawText(
                    g, _items[i].Texto, Font,
                    new Rectangle(LogicalToDeviceUnits(20), rectItem.Y, Width - LogicalToDeviceUnits(30), _altoItem),
                    colorTexto,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var nuevoHover = _items.Count == 0 ? -1 : e.Y / _altoItem;
            if (nuevoHover >= _items.Count)
                nuevoHover = -1;

            if (nuevoHover != _indiceHover)
            {
                _indiceHover = nuevoHover;
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

            var indice = e.Y / _altoItem;
            if (indice < 0 || indice >= _items.Count)
                return;

            IndiceSeleccionado = indice;
            ItemSeleccionado?.Invoke(this, _items[indice]);
        }
    }
}
