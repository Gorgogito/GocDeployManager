using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// ComboBox de solo selección con lista desplegable pintada a mano según
    /// el tema activo. Se neutraliza el tema visual nativo con SetWindowTheme
    /// para que BackColor y ForeColor se apliquen correctamente.
    /// La flecha chevron se pinta encima del botón nativo para mantener
    /// consistencia visual con el resto de los controles.
    /// </summary>
    [DesignerCategory("")]
    public sealed class ComboBoxX : ComboBox, IThemedControl
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        private readonly SuscripcionTema _suscripcionTema;

        public ComboBoxX()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode      = DrawMode.OwnerDrawFixed;
            FlatStyle     = FlatStyle.Flat;
            Font          = new Font("Segoe UI", 9.5f);
            ItemHeight    = LogicalToDeviceUnits(22);

            _suscripcionTema = new SuscripcionTema(AplicarTema);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetWindowTheme(Handle, string.Empty, string.Empty);
            AplicarTema(ThemeManager.Actual);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _suscripcionTema.Dispose();

            base.Dispose(disposing);
        }

        public void AplicarTema(Theme tema)
        {
            BackColor = tema.Superficie;
            ForeColor = tema.TextoPrimario;
            Invalidate();
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            var tema        = ThemeManager.Actual;
            var seleccionado = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            var colorFondo = seleccionado ? tema.Acento : tema.Superficie;
            var colorTexto = seleccionado ? Color.White : tema.TextoPrimario;

            using (var pincel = new SolidBrush(colorFondo))
                e.Graphics.FillRectangle(pincel, e.Bounds);

            if (e.Index >= 0 && e.Index < Items.Count)
            {
                TextRenderer.DrawText(
                    e.Graphics, GetItemText(Items[e.Index]), Font, e.Bounds, colorTexto,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
                    TextFormatFlags.LeftAndRightPadding | TextFormatFlags.SingleLine |
                    TextFormatFlags.EndEllipsis);
            }

            e.DrawFocusRectangle();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var tema    = ThemeManager.Actual;
            var g       = e.Graphics;
            GraficosX.PrepararAlta(g);

            // Ancho del área de botón desplegable del ComboBox nativo.
            var anchoBoton = SystemInformation.VerticalScrollBarWidth;
            var rectBoton  = new Rectangle(Width - anchoBoton, 0, anchoBoton, Height);

            // Cubrir el botón nativo con nuestros colores.
            using (var pincelFondo = new SolidBrush(tema.Superficie))
                g.FillRectangle(pincelFondo, rectBoton);

            // Línea separadora vertical sutil.
            using (var lapizSep = new Pen(tema.BordereReposo))
                g.DrawLine(lapizSep, rectBoton.Left, rectBoton.Top + 5, rectBoton.Left, rectBoton.Bottom - 5);

            // Chevron propio.
            var cx   = rectBoton.Left + rectBoton.Width / 2;
            var cy   = rectBoton.Top  + rectBoton.Height / 2 - 1;
            var size = LogicalToDeviceUnits(4);

            using (var lapiz = new Pen(tema.TextoSecundario, 1.5f)
            {
                LineJoin   = LineJoin.Round,
                StartCap   = LineCap.Round,
                EndCap     = LineCap.Round,
            })
            {
                g.DrawLine(lapiz, cx - size, cy - 1, cx, cy + size - 1);
                g.DrawLine(lapiz, cx,        cy + size - 1, cx + size, cy - 1);
            }

            // Borde exterior redondeado.
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var ruta       = Dibujo.RutaRedondeada(rect, LogicalToDeviceUnits(4)))
            using (var lapizBorde = new Pen(tema.BordereReposo))
                g.DrawPath(lapizBorde, ruta);
        }
    }
}
