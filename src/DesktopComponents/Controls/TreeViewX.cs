using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// TreeView temizado: DrawMode.OwnerDrawText para colorear texto/selección
    /// y SetWindowTheme (misma técnica que resolvió el bug de ComboBoxX) para
    /// que Windows deje de forzar el resaltado de selección nativo por encima
    /// del color propio.
    /// </summary>
    [DesignerCategory("")]
    public sealed class TreeViewX : TreeView, IThemedControl
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        private readonly SuscripcionTema _suscripcionTema;

        public TreeViewX()
        {
            BorderStyle = BorderStyle.None;
            DrawMode = TreeViewDrawMode.OwnerDrawText;
            Font = new Font("Segoe UI", 9.5f);
            ItemHeight = LogicalToDeviceUnits(26);
            ShowLines = false;
            FullRowSelect = true;
            HideSelection = false;

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

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            var tema = ThemeManager.Actual;
            var seleccionado = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;

            var colorFondo = seleccionado ? Theme.Mezclar(tema.Acento, tema.Superficie, 0.82f) : tema.Superficie;
            var colorTexto = seleccionado ? tema.Acento : tema.TextoPrimario;

            using (var pincel = new SolidBrush(colorFondo))
                e.Graphics.FillRectangle(pincel, e.Bounds);

            TextRenderer.DrawText(
                e.Graphics, e.Node.Text, Font,
                new Rectangle(e.Bounds.X + LogicalToDeviceUnits(4), e.Bounds.Y, e.Bounds.Width, e.Bounds.Height),
                colorTexto,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }
    }
}
