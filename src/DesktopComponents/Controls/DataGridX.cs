using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    [DesignerCategory("")]
    public sealed class DataGridX : DataGridView, IThemedControl
    {
        private readonly SuscripcionTema _suscripcionTema;
        private int _filaHover = -1;

        public DataGridX()
        {
            BorderStyle                        = BorderStyle.None;
            RowHeadersVisible                  = false;
            AllowUserToAddRows                 = false;
            AllowUserToDeleteRows              = false;
            AllowUserToResizeRows              = false;
            SelectionMode                      = DataGridViewSelectionMode.FullRowSelect;
            MultiSelect                        = false;
            EnableHeadersVisualStyles          = false;
            ColumnHeadersHeightSizeMode        = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ColumnHeadersHeight                = LogicalToDeviceUnits(36);
            RowTemplate.Height                 = LogicalToDeviceUnits(32);
            CellBorderStyle                    = DataGridViewCellBorderStyle.SingleHorizontal;
            ColumnHeadersBorderStyle           = DataGridViewHeaderBorderStyle.Single;
            AutoSizeColumnsMode                = DataGridViewAutoSizeColumnsMode.Fill;
            Font                               = new Font("Segoe UI", 9.5f);
            ShowCellToolTips                   = false;

            CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex != _filaHover)
                {
                    var anterior = _filaHover;
                    _filaHover   = e.RowIndex;
                    if (anterior >= 0 && anterior < RowCount && IsHandleCreated)
                        InvalidateRow(anterior);
                    if (IsHandleCreated)
                        InvalidateRow(_filaHover);
                }
            };

            CellMouseLeave += (s, e) =>
            {
                if (_filaHover >= 0)
                {
                    var anterior = _filaHover;
                    _filaHover   = -1;
                    if (anterior < RowCount && IsHandleCreated)
                        InvalidateRow(anterior);
                }
            };

            CellPainting += DataGridX_CellPainting;

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
            GridColor       = tema.BordereReposo;

            ColumnHeadersDefaultCellStyle.BackColor          = tema.SuperficieElevada;
            ColumnHeadersDefaultCellStyle.ForeColor          = tema.TextoSecundario;
            ColumnHeadersDefaultCellStyle.SelectionBackColor = tema.SuperficieElevada;
            ColumnHeadersDefaultCellStyle.SelectionForeColor = tema.TextoSecundario;
            ColumnHeadersDefaultCellStyle.Font               = new Font(Font, FontStyle.Bold);
            ColumnHeadersDefaultCellStyle.Alignment          = DataGridViewContentAlignment.MiddleLeft;
            ColumnHeadersDefaultCellStyle.Padding            = new Padding(LogicalToDeviceUnits(8), 0, 0, 0);

            DefaultCellStyle.BackColor          = tema.Superficie;
            DefaultCellStyle.ForeColor          = tema.TextoPrimario;
            DefaultCellStyle.SelectionBackColor = Theme.Mezclar(tema.Acento, tema.Superficie, 0.82f);
            DefaultCellStyle.SelectionForeColor = tema.TextoPrimario;
            DefaultCellStyle.Padding            = new Padding(LogicalToDeviceUnits(8), 0, 0, 0);

            AlternatingRowsDefaultCellStyle.BackColor          = Theme.Mezclar(tema.Superficie, tema.Fondo, 0.5f);
            AlternatingRowsDefaultCellStyle.ForeColor          = tema.TextoPrimario;
            AlternatingRowsDefaultCellStyle.SelectionBackColor = Theme.Mezclar(tema.Acento, tema.Superficie, 0.82f);
            AlternatingRowsDefaultCellStyle.SelectionForeColor = tema.TextoPrimario;

            BackColor = tema.Superficie;
            Invalidate();
        }

        private void DataGridX_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Pintar hover de fila solo sobre filas de datos no seleccionadas.
            if (e.RowIndex < 0 || e.RowIndex != _filaHover) return;
            if ((e.State & DataGridViewElementStates.Selected) != 0) return;

            var tema       = ThemeManager.Actual;
            var colorHover = Theme.Mezclar(tema.Acento, tema.Superficie, 0.93f);

            using (var pincel = new SolidBrush(colorHover))
                e.Graphics.FillRectangle(pincel, e.CellBounds);

            e.Paint(e.CellBounds,
                DataGridViewPaintParts.ContentForeground |
                DataGridViewPaintParts.Border);

            e.Handled = true;
        }
    }
}
