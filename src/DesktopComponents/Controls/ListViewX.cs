using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// ListView (vista Detalles) temizado con OwnerDraw + SetWindowTheme —
    /// pensado para el log en tiempo real del despliegue, coloreado por
    /// severidad (sección 10 del análisis: "ListView moderno... coloreado por
    /// severidad info/warn/error").
    /// </summary>
    [DesignerCategory("")]
    public sealed class ListViewX : ListView, IThemedControl
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        private readonly SuscripcionTema _suscripcionTema;

        public ListViewX()
        {
            View = View.Details;
            FullRowSelect = true;
            GridLines = false;
            HeaderStyle = ColumnHeaderStyle.Nonclickable;
            OwnerDraw = true;
            BorderStyle = BorderStyle.None;
            Font = new Font("Segoe UI", 9.5f);

            _suscripcionTema = new SuscripcionTema(AplicarTema);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _suscripcionTema.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetWindowTheme(Handle, string.Empty, string.Empty);
            AplicarTema(ThemeManager.Actual);
        }

        public void AplicarTema(Theme tema)
        {
            BackColor = tema.Superficie;
            ForeColor = tema.TextoPrimario;
            Invalidate();
        }

        /// <summary>
        /// Agrega una fila (una o más columnas) con un color de texto propio —
        /// ej. rojo para errores, ámbar para advertencias, en el log de despliegue.
        /// </summary>
        public ListViewItem AgregarLinea(Color color, params string[] columnas)
        {
            var item = new ListViewItem(columnas) { ForeColor = color };
            Items.Add(item);
            item.EnsureVisible();
            return item;
        }

        /// <summary>
        /// Ensancha la última columna para llenar el ancho disponible — sin
        /// esto, la franja del encabezado más allá de la última columna queda
        /// sin pintar (se ve como un hueco del color nativo de Windows).
        /// Llamar después de configurar las columnas y en cada resize.
        /// </summary>
        public void EstirarUltimaColumna()
        {
            if (Columns.Count == 0)
                return;

            var anchoRestante = ClientSize.Width;
            for (var i = 0; i < Columns.Count - 1; i++)
                anchoRestante -= Columns[i].Width;

            var minimo = LogicalToDeviceUnits(60);
            Columns[Columns.Count - 1].Width = Math.Max(minimo, anchoRestante);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            EstirarUltimaColumna();
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            var tema = ThemeManager.Actual;

            using (var pincel = new SolidBrush(tema.Fondo))
                e.Graphics.FillRectangle(pincel, e.Bounds);

            TextRenderer.DrawText(
                e.Graphics, e.Header.Text, Font,
                new Rectangle(e.Bounds.X + LogicalToDeviceUnits(6), e.Bounds.Y, e.Bounds.Width, e.Bounds.Height),
                tema.TextoSecundario,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

            using (var lapiz = new Pen(tema.Borde))
                e.Graphics.DrawLine(lapiz, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            // Con OwnerDraw=true en vista Detalles, el dibujo real ocurre por
            // subitem en OnDrawSubItem; este método solo evita que WinForms
            // recurra al dibujo nativo para la fila completa.
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            var tema = ThemeManager.Actual;

            // OJO: e.ItemState viene poco confiable en .NET (suele reportar
            // "Selected" siempre); e.Item.Selected es la fuente de verdad real.
            var seleccionado = e.Item.Selected;

            var colorFondo = seleccionado ? Theme.Mezclar(tema.Acento, tema.Superficie, 0.82f) : tema.Superficie;
            var colorTexto = e.Item.ForeColor;

            using (var pincel = new SolidBrush(colorFondo))
                e.Graphics.FillRectangle(pincel, e.Bounds);

            TextRenderer.DrawText(
                e.Graphics, e.SubItem.Text, Font,
                new Rectangle(e.Bounds.X + LogicalToDeviceUnits(6), e.Bounds.Y, e.Bounds.Width, e.Bounds.Height),
                colorTexto,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }
    }
}
