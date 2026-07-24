using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DesktopComponents.Controls;
using DesktopComponents.Forms;
using DesktopComponents.Theming;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.UI.Historial
{
    /// <summary>
    /// Consulta del historial de despliegues (sección 10 del análisis): filtros,
    /// búsqueda, exportación (Excel y PDF) y detalle completo.
    /// </summary>
    [DesignerCategory("")]
    public sealed class HistorialForm : MetroForm
    {
        private readonly Bootstrapper _bootstrapper;
        private List<Despliegue> _resultadosActuales = new List<Despliegue>();

        private TextBoxX _txtGoc;
        private ComboBoxX _comboAmbiente;
        private ComboBoxX _comboSistema;
        private ComboBoxX _comboResultado;
        private DateTimePicker _dtpDesde;
        private DateTimePicker _dtpHasta;

        private DataGridX _gridHistorial;
        private Label _lblEstado;

        public HistorialForm(Bootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));

            Text = "GocDeployManager — Historial de despliegues";
            ClientSize = new Size(LogicalToDeviceUnits(1040), LogicalToDeviceUnits(600));
            StartPosition = FormStartPosition.CenterParent;

            ConstruirControles();
            CargarOpcionesFiltro();
            Buscar();
        }

        private void ConstruirControles()
        {
            var tema = ThemeManager.Actual;

            var filtros = new RibbonBar();
            var lblGoc = new Label
            {
                Text = "GOC:",
                AutoSize = true,
                ForeColor = tema.TextoSecundario,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            _txtGoc = new TextBoxX { Width = LogicalToDeviceUnits(110), TextoMarcador = "GOC" };
            _comboAmbiente = new ComboBoxX { Width = LogicalToDeviceUnits(140) };
            _comboSistema = new ComboBoxX { Width = LogicalToDeviceUnits(100) };
            _comboResultado = new ComboBoxX { Width = LogicalToDeviceUnits(100) };
            _dtpDesde = new DateTimePicker { Width = LogicalToDeviceUnits(130), ShowCheckBox = true, Checked = false, Format = DateTimePickerFormat.Short };
            _dtpHasta = new DateTimePicker { Width = LogicalToDeviceUnits(130), ShowCheckBox = true, Checked = false, Format = DateTimePickerFormat.Short };
            var btnBuscar = new ButtonX { Text = "Buscar", Width = LogicalToDeviceUnits(90), Variante = VarianteButtonX.Primario };
            var btnLimpiar = new ButtonX { Text = "Limpiar", Width = LogicalToDeviceUnits(90), Variante = VarianteButtonX.Secundario };

            btnBuscar.Click += (s, e) => Buscar();
            btnLimpiar.Click += BtnLimpiar_Click;

            filtros.AgregarControl(lblGoc);
            filtros.AgregarControl(_txtGoc);
            filtros.AgregarControl(_comboAmbiente);
            filtros.AgregarControl(_comboSistema);
            filtros.AgregarControl(_comboResultado);
            filtros.AgregarControl(_dtpDesde);
            filtros.AgregarControl(_dtpHasta);
            filtros.AgregarSeparador();
            filtros.AgregarControl(btnBuscar);
            filtros.AgregarControl(btnLimpiar);

            var barraAcciones = new RibbonBar { Height = LogicalToDeviceUnits(50) };
            var btnVerDetalle = new ButtonX { Text = "Ver detalle", Width = LogicalToDeviceUnits(120), Variante = VarianteButtonX.Secundario };
            var btnExportarExcel = new ButtonX { Text = "Exportar a Excel", Width = LogicalToDeviceUnits(150), Variante = VarianteButtonX.Secundario };
            var btnExportarPdf = new ButtonX { Text = "Exportar a PDF", Width = LogicalToDeviceUnits(150), Variante = VarianteButtonX.Secundario };

            btnVerDetalle.Click += BtnVerDetalle_Click;
            btnExportarExcel.Click += BtnExportarExcel_Click;
            btnExportarPdf.Click += BtnExportarPdf_Click;

            barraAcciones.AgregarControl(btnVerDetalle);
            barraAcciones.AgregarControl(btnExportarExcel);
            barraAcciones.AgregarControl(btnExportarPdf);

            var anchoEstado = LogicalToDeviceUnits(320);
            _lblEstado = new Label
            {
                AutoSize = false,
                Location = new Point(ClientSize.Width - LogicalToDeviceUnits(16) - anchoEstado, LogicalToDeviceUnits(14)),
                Size = new Size(anchoEstado, LogicalToDeviceUnits(24)),
                ForeColor = tema.TextoSecundario,
                TextAlign = ContentAlignment.MiddleRight,
            };
            barraAcciones.Controls.Add(_lblEstado);

            _gridHistorial = new DataGridX { Dock = DockStyle.Fill };
            _gridHistorial.Columns.Add("Fecha", "Fecha y hora");
            _gridHistorial.Columns.Add("Goc", "GOC");
            _gridHistorial.Columns.Add("Ambiente", "Ambiente");
            _gridHistorial.Columns.Add("Sistemas", "Sistemas");
            _gridHistorial.Columns.Add("Resultado", "Resultado");
            _gridHistorial.Columns.Add("Usuario", "Usuario");
            _gridHistorial.Columns.Add("TiempoCompilacion", "Compilación");
            _gridHistorial.Columns.Add("TiempoDespliegue", "Despliegue");
            _gridHistorial.CellDoubleClick += (s, e) => AbrirDetalleSeleccionado();

            Controls.Add(_gridHistorial);
            Controls.Add(barraAcciones);
            Controls.Add(filtros);
        }

        private void CargarOpcionesFiltro()
        {
            _comboAmbiente.Items.Clear();
            _comboAmbiente.Items.Add("Todos los ambientes");
            foreach (var ambiente in _bootstrapper.Ambientes.ObtenerTodos())
                _comboAmbiente.Items.Add(ambiente.Nombre);
            _comboAmbiente.SelectedIndex = 0;

            _comboSistema.Items.Clear();
            _comboSistema.Items.Add("Todos los sistemas");
            foreach (var sistema in _bootstrapper.Sistemas.ObtenerTodos())
                _comboSistema.Items.Add(sistema.Sistema.Codigo);
            _comboSistema.SelectedIndex = 0;

            _comboResultado.Items.Clear();
            _comboResultado.Items.Add("Cualquier resultado");
            _comboResultado.Items.Add(ResultadoDespliegue.Exitoso.ToString());
            _comboResultado.Items.Add(ResultadoDespliegue.Fallido.ToString());
            _comboResultado.SelectedIndex = 0;
        }

        private void Buscar()
        {
            var filtro = new FiltroHistorial
            {
                Goc = string.IsNullOrWhiteSpace(_txtGoc.Text) ? null : _txtGoc.Text.Trim(),
                Ambiente = _comboAmbiente.SelectedIndex <= 0 ? null : (string)_comboAmbiente.SelectedItem,
                Sistema = _comboSistema.SelectedIndex <= 0 ? null : (string)_comboSistema.SelectedItem,
                Resultado = ObtenerResultadoSeleccionado(),
                FechaDesde = _dtpDesde.Checked ? _dtpDesde.Value.Date : (DateTime?)null,
                FechaHasta = _dtpHasta.Checked ? _dtpHasta.Value.Date.AddDays(1).AddTicks(-1) : (DateTime?)null,
            };

            _resultadosActuales = _bootstrapper.Historial.Buscar(filtro).ToList();
            CargarGrid();
        }

        private ResultadoDespliegue? ObtenerResultadoSeleccionado()
        {
            if (_comboResultado.SelectedIndex <= 0)
                return null;

            return (ResultadoDespliegue)Enum.Parse(typeof(ResultadoDespliegue), (string)_comboResultado.SelectedItem);
        }

        private void CargarGrid()
        {
            _gridHistorial.Rows.Clear();
            foreach (var d in _resultadosActuales)
            {
                var indice = _gridHistorial.Rows.Add(
                    d.FechaHora.ToString("yyyy-MM-dd HH:mm:ss"),
                    d.Goc,
                    d.Ambiente,
                    string.Join(", ", d.Sistemas),
                    d.Resultado.ToString(),
                    d.UsuarioAplicacion,
                    d.TiempoCompilacion.ToString(@"hh\:mm\:ss"),
                    d.TiempoDespliegue.ToString(@"hh\:mm\:ss"));
                _gridHistorial.Rows[indice].Tag = d;
            }

            _lblEstado.Text = $"{_resultadosActuales.Count} despliegue(s) encontrado(s).";
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            _txtGoc.Text = string.Empty;
            _comboAmbiente.SelectedIndex = 0;
            _comboSistema.SelectedIndex = 0;
            _comboResultado.SelectedIndex = 0;
            _dtpDesde.Checked = false;
            _dtpHasta.Checked = false;
            Buscar();
        }

        private void BtnVerDetalle_Click(object sender, EventArgs e) => AbrirDetalleSeleccionado();

        private void AbrirDetalleSeleccionado()
        {
            if (!(_gridHistorial.CurrentRow?.Tag is Despliegue despliegue))
            {
                _lblEstado.Text = "Selecciona un despliegue de la lista.";
                return;
            }

            using (var dialogo = new DetalleDespliegueDialog(despliegue))
                dialogo.ShowDialog(this);
        }

        private void BtnExportarExcel_Click(object sender, EventArgs e)
        {
            if (_resultadosActuales.Count == 0)
            {
                _lblEstado.Text = "No hay resultados para exportar.";
                return;
            }

            using (var dialogo = new SaveFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", FileName = "HistorialDespliegues.xlsx" })
            {
                if (dialogo.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    ExportadorHistorial.ExportarExcel(_resultadosActuales, dialogo.FileName);
                    NotificationX.Mostrar(
                        "Exportado", $"Se exportaron {_resultadosActuales.Count} registros a Excel.", TipoNotificacion.Exito);
                }
                catch (Exception ex)
                {
                    _lblEstado.Text = $"No se pudo exportar: {ex.Message}";
                }
            }
        }

        private void BtnExportarPdf_Click(object sender, EventArgs e)
        {
            if (_resultadosActuales.Count == 0)
            {
                _lblEstado.Text = "No hay resultados para exportar.";
                return;
            }

            using (var dialogo = new SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf", FileName = "HistorialDespliegues.pdf" })
            {
                if (dialogo.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    ExportadorHistorial.ExportarPdf(_resultadosActuales, dialogo.FileName);
                    NotificationX.Mostrar(
                        "Exportado", $"Se exportaron {_resultadosActuales.Count} registros a PDF.", TipoNotificacion.Exito);
                }
                catch (Exception ex)
                {
                    _lblEstado.Text = $"No se pudo exportar: {ex.Message}";
                }
            }
        }
    }
}
