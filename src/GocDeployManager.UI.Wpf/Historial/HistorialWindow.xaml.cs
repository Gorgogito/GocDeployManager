using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;
using GocDeployManager.UI.Ventanas;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;

namespace GocDeployManager.UI.Historial
{
    public partial class HistorialWindow : VentanaBase
    {
        private readonly Bootstrapper _bootstrapper;
        private List<Despliegue> _resultadosActuales = new List<Despliegue>();
        private SnackbarMessageQueue _snackbarQueue;

        private static readonly Brush BrushExito   = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
        private static readonly Brush BrushFallido  = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));
        private static readonly Brush BrushNeutro   = new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75));

        private sealed class DespliegueRow
        {
            public string Fecha          { get; set; }
            public string Goc            { get; set; }
            public string Ambiente       { get; set; }
            public string Sistemas       { get; set; }
            public string Resultado      { get; set; }
            public Brush  CorResultado   { get; set; }
            public string Usuario        { get; set; }
            public string Compilacion    { get; set; }
            public string TiempoDespliegue { get; set; }
            public Despliegue Entidad    { get; set; }
        }

        public HistorialWindow(Bootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            InitializeComponent();
            Loaded += HistorialWindow_Loaded;
        }

        private void HistorialWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _snackbarQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(4));
            snackbar.MessageQueue = _snackbarQueue;

            CargarOpcionesFiltro();
            Buscar();
        }

        // ─── Carga de filtros ─────────────────────────────────────────────

        private void CargarOpcionesFiltro()
        {
            comboAmbiente.Items.Add("Todos los ambientes");
            foreach (var a in _bootstrapper.Ambientes.ObtenerTodos())
                comboAmbiente.Items.Add(a.Nombre);
            comboAmbiente.SelectedIndex = 0;

            comboSistema.Items.Add("Todos los sistemas");
            foreach (var s in _bootstrapper.Sistemas.ObtenerTodos())
                comboSistema.Items.Add(s.Sistema.Codigo);
            comboSistema.SelectedIndex = 0;

            comboResultado.Items.Add("Cualquier resultado");
            comboResultado.Items.Add(ResultadoDespliegue.Exitoso.ToString());
            comboResultado.Items.Add(ResultadoDespliegue.Fallido.ToString());
            comboResultado.SelectedIndex = 0;
        }

        // ─── Búsqueda ─────────────────────────────────────────────────────

        private void Buscar()
        {
            var filtro = new FiltroHistorial
            {
                Goc = string.IsNullOrWhiteSpace(txtGoc.Text) ? null : txtGoc.Text.Trim(),
                Ambiente = comboAmbiente.SelectedIndex <= 0 ? null : comboAmbiente.SelectedItem as string,
                Sistema  = comboSistema.SelectedIndex  <= 0 ? null : comboSistema.SelectedItem  as string,
                Resultado = ObtenerResultadoSeleccionado(),
                FechaDesde = chkDesde.IsChecked == true ? dpDesde.SelectedDate : (DateTime?)null,
                FechaHasta = chkHasta.IsChecked == true && dpHasta.SelectedDate.HasValue
                    ? dpHasta.SelectedDate.Value.Date.AddDays(1).AddTicks(-1)
                    : (DateTime?)null,
            };

            _resultadosActuales = _bootstrapper.Historial.Buscar(filtro).ToList();
            CargarGrid();
        }

        private ResultadoDespliegue? ObtenerResultadoSeleccionado()
        {
            if (comboResultado.SelectedIndex <= 0) return null;
            return (ResultadoDespliegue)Enum.Parse(
                typeof(ResultadoDespliegue), comboResultado.SelectedItem as string);
        }

        private void CargarGrid()
        {
            var filas = _resultadosActuales.Select(d => new DespliegueRow
            {
                Fecha          = d.FechaHora.ToString("yyyy-MM-dd HH:mm:ss"),
                Goc            = d.Goc,
                Ambiente       = d.Ambiente,
                Sistemas       = string.Join(", ", d.Sistemas),
                Resultado      = d.Resultado.ToString(),
                CorResultado   = d.Resultado == ResultadoDespliegue.Exitoso ? BrushExito : BrushFallido,
                Usuario        = d.UsuarioAplicacion,
                Compilacion    = d.TiempoCompilacion.ToString(@"hh\:mm\:ss"),
                TiempoDespliegue = d.TiempoDespliegue.ToString(@"hh\:mm\:ss"),
                Entidad        = d,
            }).ToList();

            gridHistorial.ItemsSource = filas;
            lblEstado.Text = $"{filas.Count} despliegue(s) encontrado(s).";
        }

        // ─── Eventos de filtro ────────────────────────────────────────────

        private void BtnBuscar_Click(object sender, RoutedEventArgs e) => Buscar();

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            txtGoc.Text = string.Empty;
            comboAmbiente.SelectedIndex  = 0;
            comboSistema.SelectedIndex   = 0;
            comboResultado.SelectedIndex = 0;
            chkDesde.IsChecked = false;
            chkHasta.IsChecked = false;
            dpDesde.SelectedDate = null;
            dpHasta.SelectedDate = null;
            Buscar();
        }

        // ─── Acciones sobre filas ─────────────────────────────────────────

        private void BtnVerDetalle_Click(object sender, RoutedEventArgs e)
            => AbrirDetalleSeleccionado();

        private void GridHistorial_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => AbrirDetalleSeleccionado();

        private void AbrirDetalleSeleccionado()
        {
            var fila = gridHistorial.SelectedItem as DespliegueRow;
            if (fila == null)
            {
                lblEstado.Text = "Selecciona un despliegue de la lista.";
                return;
            }

            var dialogo = new DetalleDespliegueDialog(fila.Entidad);
            dialogo.Owner = this;
            dialogo.ShowDialog();
        }

        // ─── Exportación ─────────────────────────────────────────────────

        private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            if (!VerificarHayResultados()) return;

            var dlg = new SaveFileDialog
            {
                Filter   = "Excel (*.xlsx)|*.xlsx",
                FileName = "HistorialDespliegues.xlsx",
            };
            if (dlg.ShowDialog(this) != true) return;

            try
            {
                ExportadorHistorial.ExportarExcel(_resultadosActuales, dlg.FileName);
                _snackbarQueue.Enqueue($"Se exportaron {_resultadosActuales.Count} registros a Excel.");
            }
            catch (Exception ex)
            {
                lblEstado.Text = $"Error al exportar: {ex.Message}";
            }
        }

        private void BtnExportarPdf_Click(object sender, RoutedEventArgs e)
        {
            if (!VerificarHayResultados()) return;

            var dlg = new SaveFileDialog
            {
                Filter   = "PDF (*.pdf)|*.pdf",
                FileName = "HistorialDespliegues.pdf",
            };
            if (dlg.ShowDialog(this) != true) return;

            try
            {
                ExportadorHistorial.ExportarPdf(_resultadosActuales, dlg.FileName);
                _snackbarQueue.Enqueue($"Se exportaron {_resultadosActuales.Count} registros a PDF.");
            }
            catch (Exception ex)
            {
                lblEstado.Text = $"Error al exportar: {ex.Message}";
            }
        }

        private bool VerificarHayResultados()
        {
            if (_resultadosActuales.Count > 0) return true;
            lblEstado.Text = "No hay resultados para exportar.";
            return false;
        }
    }
}
