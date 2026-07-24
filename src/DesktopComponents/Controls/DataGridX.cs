using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// DataGridView temizado vía sus propiedades de estilo (DefaultCellStyle,
    /// ColumnHeadersDefaultCellStyle, EnableHeadersVisualStyles=false) — a
    /// diferencia de ComboBox, DataGridView sí está diseñado para theming
    /// completo sin necesitar SetWindowTheme ni owner-draw.
    /// </summary>
    [DesignerCategory("")]
    public sealed class DataGridX : DataGridView, IThemedControl
    {
        private readonly SuscripcionTema _suscripcionTema;

        public DataGridX()
        {
            BorderStyle = BorderStyle.None;
            RowHeadersVisible = false;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeRows = false;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            MultiSelect = false;
            EnableHeadersVisualStyles = false;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ColumnHeadersHeight = LogicalToDeviceUnits(36);
            RowTemplate.Height = LogicalToDeviceUnits(32);
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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
            BackgroundColor = tema.Superficie;
            GridColor = tema.Borde;

            ColumnHeadersDefaultCellStyle.BackColor = tema.Fondo;
            ColumnHeadersDefaultCellStyle.ForeColor = tema.TextoSecundario;
            ColumnHeadersDefaultCellStyle.SelectionBackColor = tema.Fondo;
            ColumnHeadersDefaultCellStyle.SelectionForeColor = tema.TextoSecundario;
            ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
            ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ColumnHeadersDefaultCellStyle.Padding = new Padding(LogicalToDeviceUnits(8), 0, 0, 0);

            DefaultCellStyle.BackColor = tema.Superficie;
            DefaultCellStyle.ForeColor = tema.TextoPrimario;
            DefaultCellStyle.SelectionBackColor = Theme.Mezclar(tema.Acento, tema.Superficie, 0.82f);
            DefaultCellStyle.SelectionForeColor = tema.TextoPrimario;
            DefaultCellStyle.Padding = new Padding(LogicalToDeviceUnits(8), 0, 0, 0);

            AlternatingRowsDefaultCellStyle.BackColor = Theme.Mezclar(tema.Superficie, tema.Fondo, 0.5f);
            AlternatingRowsDefaultCellStyle.ForeColor = tema.TextoPrimario;
            AlternatingRowsDefaultCellStyle.SelectionBackColor = Theme.Mezclar(tema.Acento, tema.Superficie, 0.82f);
            AlternatingRowsDefaultCellStyle.SelectionForeColor = tema.TextoPrimario;

            BackColor = tema.Superficie;

            Invalidate();
        }
    }
}
