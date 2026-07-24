using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// PropertyGrid temizado vía sus propiedades de color dedicadas
    /// (LineColor, ViewBackColor, HelpBackColor, etc.) — igual que DataGridView,
    /// PropertyGrid está diseñado para poder re-colorearse por completo sin
    /// owner-draw ni trucos de tema nativo.
    /// </summary>
    [DesignerCategory("")]
    public sealed class PropertyGridX : PropertyGrid, IThemedControl
    {
        private readonly SuscripcionTema _suscripcionTema;

        public PropertyGridX()
        {
            ToolbarVisible = false;
            PropertySort = PropertySort.Categorized;
            Font = new Font("Segoe UI", 9.5f);

            _suscripcionTema = new SuscripcionTema(AplicarTema);
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
            LineColor = tema.Borde;
            CategoryForeColor = tema.TextoSecundario;
            CategorySplitterColor = tema.Borde;
            ViewBackColor = tema.Superficie;
            ViewForeColor = tema.TextoPrimario;
            ViewBorderColor = tema.Borde;
            HelpBackColor = tema.Fondo;
            HelpForeColor = tema.TextoSecundario;
            HelpBorderColor = tema.Borde;
            SelectedItemWithFocusBackColor = tema.Acento;
            SelectedItemWithFocusForeColor = Color.White;
            DisabledItemForeColor = tema.TextoSecundario;

            Invalidate();
        }
    }
}
