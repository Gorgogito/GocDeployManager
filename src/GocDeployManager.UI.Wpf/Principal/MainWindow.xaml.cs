using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GocDeployManager.Application.Auth;
using GocDeployManager.Application.Deploy;
using GocDeployManager.Domain.Entities;
using GocDeployManager.UI.Configuracion;
using GocDeployManager.UI.Historial;
using GocDeployManager.UI.Temas;
using GocDeployManager.UI.Ventanas;
using MaterialDesignThemes.Wpf;

namespace GocDeployManager.UI.Principal
{
    public partial class MainWindow : VentanaBase
    {
        private readonly Bootstrapper _bootstrapper;
        private readonly SesionUsuario _sesion;
        private readonly Stopwatch _cronometro = new Stopwatch();
        private readonly DispatcherTimer _timerReloj;
        private readonly DispatcherTimer _timerProgreso;
        private readonly ObservableCollection<LogItem> _logItems = new ObservableCollection<LogItem>();
        private readonly List<CheckBox> _checkboxesSistema = new List<CheckBox>();

        private bool _seleccionandoNav;
        private bool _progresoSubiendo = true;
        private int _totalEtapas;
        private int _etapaActual;

        // Colores fijos para el log — independientes del tema
        private static readonly Brush BrushExito      = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
        private static readonly Brush BrushPeligro    = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));
        private static readonly Brush BrushAdvertencia = new SolidColorBrush(Color.FromRgb(0xE6, 0x5C, 0x00));
        private static readonly Brush BrushDebug      = new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75));

        public MainWindow(Bootstrapper bootstrapper, SesionUsuario sesion)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            _sesion = sesion ?? throw new ArgumentNullException(nameof(sesion));

            _timerReloj = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timerReloj.Tick += TimerReloj_Tick;

            _timerProgreso = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
            _timerProgreso.Tick += TimerProgreso_Tick;

            InitializeComponent();

            Loaded  += MainWindow_Loaded;
            Closed  += (s, e) => { _timerReloj.Stop(); _timerProgreso.Stop(); };
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Title = $"GocDeployManager — {_sesion.Usuario.NombreVisible}";
            lblTituloVentana.Text = Title;
            lblUsuarioSide.Text   = _sesion.Usuario.NombreUsuario;

            logListView.ItemsSource = _logItems;
            snackbar.MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(4));

            if (!_sesion.Usuario.PuedeDesplegar)
            {
                btnIniciarDespliegue.IsEnabled = false;
                btnIniciarDespliegue.Content   = "Sin permiso";
            }

            // Wiring de tema después de poner el índice inicial, evita disparo extra
            comboTema.SelectedIndex = TemaManager.Actual == TemaBase.Oscuro ? 1 : 0;
            comboTema.SelectionChanged += ComboTema_SelectionChanged;

            // Wiring del menú lateral después de seleccionar el índice inicial
            lstSideMenu.SelectedIndex = 0;
            lstSideMenu.SelectionChanged += LstSideMenu_SelectionChanged;

            // Wiring del combo de ambiente (puede disparar ActualizarSistemas al cargar)
            comboAmbiente.SelectionChanged += ComboAmbiente_SelectionChanged;
            CargarAmbientes();
        }

        // ─── Carga de datos ───────────────────────────────────────────────

        private void CargarAmbientes()
        {
            var ambientes = _bootstrapper.Ambientes.ObtenerTodos().ToList();
            comboAmbiente.ItemsSource = ambientes;
            if (ambientes.Any())
                comboAmbiente.SelectedIndex = 0;
        }

        private void ComboAmbiente_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ActualizarSistemasDisponibles();

        private void ActualizarSistemasDisponibles()
        {
            panelSistemas.Children.Clear();
            _checkboxesSistema.Clear();

            var ambiente = comboAmbiente.SelectedItem as Ambiente;
            if (ambiente == null) return;

            foreach (var ambienteSistema in ambiente.Sistemas)
            {
                var chk = new CheckBox
                {
                    Content   = ambienteSistema.Sistema.Nombre,
                    Tag       = ambienteSistema.Sistema,
                    IsChecked = true,
                    Margin    = new Thickness(0, 0, 16, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                };
                panelSistemas.Children.Add(chk);
                _checkboxesSistema.Add(chk);
            }
        }

        // ─── Navegación lateral ───────────────────────────────────────────

        private void LstSideMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_seleccionandoNav) return;

            var item    = lstSideMenu.SelectedItem as ListBoxItem;
            var etiqueta = item?.Tag as string;
            if (string.IsNullOrEmpty(etiqueta) || etiqueta == "principal") return;

            // Vuelve siempre a "Principal" después de abrir un diálogo
            _seleccionandoNav = true;
            lstSideMenu.SelectedIndex = 0;
            _seleccionandoNav = false;

            if (etiqueta == "configuracion")
            {
                if (!_sesion.Usuario.PuedeAdministrar)
                {
                    snackbar.MessageQueue.Enqueue("Solo un administrador puede acceder a Configuración.");
                    return;
                }
                new ConfiguracionWindow(_bootstrapper).ShowDialog();
                CargarAmbientes();
                return;
            }

            if (etiqueta == "historial")
            {
                new HistorialWindow(_bootstrapper).ShowDialog();
                return;
            }
        }

        // ─── Tema ─────────────────────────────────────────────────────────

        private void ComboTema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var temaSeleccionado = comboTema.SelectedIndex == 1 ? TemaBase.Oscuro : TemaBase.Claro;
            TemaManager.Establecer(temaSeleccionado);
        }

        // ─── Despliegue ───────────────────────────────────────────────────

        private void BtnIniciarDespliegue_Click(object sender, RoutedEventArgs e)
        {
            MostrarError(string.Empty);

            var resultadoGoc = Goc.Crear(txtGoc.Text);
            if (resultadoGoc.IsFailure)
            {
                MostrarError(resultadoGoc.Error);
                return;
            }

            if (!(comboAmbiente.SelectedItem is Ambiente ambiente))
            {
                MostrarError("Selecciona un ambiente.");
                return;
            }

            var sistemasSeleccionados = _checkboxesSistema
                .Where(c => c.IsChecked == true)
                .Select(c => (Sistema)c.Tag)
                .ToList();

            if (sistemasSeleccionados.Count == 0)
            {
                MostrarError("Selecciona al menos un sistema.");
                return;
            }

            if (string.IsNullOrEmpty(_sesion.Usuario.UsuarioBitbucket) ||
                string.IsNullOrEmpty(_sesion.ContrasenaBitbucketEnClaro))
            {
                MostrarError("Tu usuario no tiene credenciales de Bitbucket configuradas.");
                return;
            }

            var solicitud = new SolicitudDespliegue(
                resultadoGoc.Value,
                ambiente,
                sistemasSeleccionados,
                _sesion.Usuario.NombreUsuario,
                Environment.UserName,
                Environment.MachineName,
                _sesion.Usuario.UsuarioBitbucket,
                _sesion.ContrasenaBitbucketEnClaro,
                _bootstrapper.RutaClonado);

            _ = EjecutarDespliegueAsync(solicitud);
        }

        private async Task EjecutarDespliegueAsync(SolicitudDespliegue solicitud)
        {
            // N sistemas × 4 etapas (Validacion, Clonado, Compilacion, Despliegue) + 1 Finalizacion
            _totalEtapas = solicitud.Sistemas.Count * 4 + 1;
            _etapaActual = 0;

            ConmutarControlesDurante(activo: true);
            _logItems.Clear();
            lblOverlay.Text = "Desplegando...";
            _cronometro.Restart();
            _timerReloj.Start();
            _timerProgreso.Start();

            var progreso = new Progress<MensajeSalidaDespliegue>(MostrarMensajeSalida);
            var resultado = await Task.Run(() =>
                _bootstrapper.Orquestador.EjecutarDespliegue(solicitud, progreso));

            _timerReloj.Stop();
            _timerProgreso.Stop();
            if (resultado.IsSuccess) barraProgreso.Value = 100;
            ConmutarControlesDurante(activo: false);

            if (resultado.IsSuccess)
            {
                lblStatusIzquierda.Text = "Despliegue exitoso.";
                snackbar.MessageQueue.Enqueue(
                    $"{solicitud.Goc.Numero} desplegado en {solicitud.Ambiente.Nombre} exitosamente.");
            }
            else
            {
                lblStatusIzquierda.Text = "Despliegue fallido.";
                snackbar.MessageQueue.Enqueue($"Error: {resultado.Error}");
            }
        }

        private void MostrarMensajeSalida(MensajeSalidaDespliegue mensaje)
        {
            if (mensaje.Etapa.HasValue)
            {
                _etapaActual++;
                var pct = (int)(_etapaActual * 100.0 / _totalEtapas);
                barraProgresoStatus.Value  = pct;
                lblPorcentajeStatus.Text   = $"{pct}%";

                var bg = mensaje.Nivel == NivelMensajeSalida.Success ? BrushExito
                       : mensaje.Nivel == NivelMensajeSalida.Error   ? BrushPeligro
                       : (Brush)FindResource("PrimaryHueMidBrush");
                AgregarLogItem(string.Empty, mensaje.Texto, Brushes.White, bg, FontWeights.Bold);
            }
            else
            {
                var formato = mensaje.Nivel == NivelMensajeSalida.Debug ? "HH:mm:ss.fff" : "HH:mm:ss";
                AgregarLogItem(
                    mensaje.Momento.ToString(formato),
                    PrefijoParaNivel(mensaje.Nivel) + mensaje.Texto,
                    BrushParaNivel(mensaje.Nivel));
            }

            if (mensaje.EsResumen)
            {
                lblStatusIzquierda.Text = mensaje.Texto;
                lblEtapaActual.Text     = mensaje.Texto;
            }
        }

        private void AgregarLogItem(string hora, string mensaje, Brush corTexto,
                                    Brush corFondo = null, FontWeight? pesoFuente = null)
        {
            var item = new LogItem
            {
                Hora      = hora,
                Mensaje   = mensaje,
                CorTexto  = corTexto,
                CorFondo  = corFondo ?? Brushes.Transparent,
                PesoFuente = pesoFuente ?? FontWeights.Normal,
            };
            _logItems.Add(item);
            logListView.ScrollIntoView(item);
        }

        private Brush BrushParaNivel(NivelMensajeSalida nivel)
        {
            switch (nivel)
            {
                case NivelMensajeSalida.Success: return BrushExito;
                case NivelMensajeSalida.Warning: return BrushAdvertencia;
                case NivelMensajeSalida.Error:   return BrushPeligro;
                case NivelMensajeSalida.Debug:   return BrushDebug;
                default: return (Brush)FindResource("MaterialDesignBody");
            }
        }

        private static string PrefijoParaNivel(NivelMensajeSalida nivel)
        {
            switch (nivel)
            {
                case NivelMensajeSalida.Success: return "[SUCCESS] ";
                case NivelMensajeSalida.Warning: return "[WARNING] ";
                case NivelMensajeSalida.Error:   return "[ERROR] ";
                default: return string.Empty;
            }
        }

        // ─── Helpers de UI ────────────────────────────────────────────────

        private void ConmutarControlesDurante(bool activo)
        {
            btnIniciarDespliegue.IsEnabled = !activo && _sesion.Usuario.PuedeDesplegar;
            txtGoc.IsEnabled              = !activo;
            comboAmbiente.IsEnabled       = !activo;
            foreach (var chk in _checkboxesSistema)
                chk.IsEnabled = !activo;
            overlayGrid.Visibility = activo ? Visibility.Visible : Visibility.Collapsed;

            barraProgresoStatus.Visibility = activo ? Visibility.Visible : Visibility.Collapsed;
            lblPorcentajeStatus.Visibility = activo ? Visibility.Visible : Visibility.Collapsed;
            if (!activo)
            {
                barraProgresoStatus.Value = 0;
                lblPorcentajeStatus.Text  = string.Empty;
            }
        }

        private void MostrarError(string mensaje)
        {
            lblError.Text       = mensaje;
            lblError.Visibility = string.IsNullOrEmpty(mensaje) ? Visibility.Collapsed : Visibility.Visible;
        }

        // ─── Timers ───────────────────────────────────────────────────────

        private void TimerReloj_Tick(object sender, EventArgs e)
        {
            lblStatusDerecha.Text = $"Tiempo transcurrido: {_cronometro.Elapsed:hh\\:mm\\:ss}";
        }

        private void TimerProgreso_Tick(object sender, EventArgs e)
        {
            var valor = barraProgreso.Value;
            if (_progresoSubiendo)
            {
                valor += 4;
                if (valor >= 95) _progresoSubiendo = false;
            }
            else
            {
                valor -= 4;
                if (valor <= 5) _progresoSubiendo = true;
            }
            barraProgreso.Value = valor;
        }
    }
}
