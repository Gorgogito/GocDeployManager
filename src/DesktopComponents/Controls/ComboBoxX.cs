using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// ComboBox de solo selección (sin edición libre) con lista desplegable
    /// pintada a mano según el tema activo. Windows ignora BackColor en un
    /// DropDownList mientras tenga el tema visual nativo activo (se ve blanco
    /// siempre, sin importar el tema oscuro/claro de la app); se desactiva ese
    /// tema nativo vía SetWindowTheme para que el color sí se aplique.
    /// </summary>
    [DesignerCategory("")]
    public sealed class ComboBoxX : ComboBox, IThemedControl
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        private readonly SuscripcionTema _suscripcionTema;

        public ComboBoxX()
        {
            // A diferencia de ButtonX, ComboBox envuelve una ventana nativa de
            // Windows que depende de su propio dibujo interno: activar
            // ControlStyles.UserPaint aquí lo deja completamente en blanco,
            // porque WinForms deja de invocar el dibujo nativo y nada lo
            // reemplaza. El look propio se logra solo con FlatStyle + OwnerDraw
            // + SetWindowTheme.
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode = DrawMode.OwnerDrawFixed;
            FlatStyle = FlatStyle.Flat;
            Font = new Font("Segoe UI", 9.5f);
            ItemHeight = LogicalToDeviceUnits(22);

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
            var tema = ThemeManager.Actual;
            var seleccionado = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            var colorFondo = seleccionado ? tema.Acento : tema.Superficie;
            var colorTexto = seleccionado ? Color.White : tema.TextoPrimario;

            using (var pincel = new SolidBrush(colorFondo))
                e.Graphics.FillRectangle(pincel, e.Bounds);

            if (e.Index >= 0 && e.Index < Items.Count)
            {
                TextRenderer.DrawText(
                    e.Graphics, GetItemText(Items[e.Index]), Font, e.Bounds, colorTexto,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.LeftAndRightPadding |
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }

            e.DrawFocusRectangle();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var tema = ThemeManager.Actual;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var lapizBorde = new Pen(tema.Borde))
                e.Graphics.DrawRectangle(lapizBorde, rect);
        }
    }
}
