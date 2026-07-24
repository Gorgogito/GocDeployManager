using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DesktopComponents.Controls;
using DesktopComponents.Forms;
using DesktopComponents.Theming;
using GocDeployManager.Application.Auth;
using GocDeployManager.Application.Deploy;
using GocDeployManager.Domain.Entities;
using GocDeployManager.UI.Configuracion;
using GocDeployManager.UI.Historial;

namespace GocDeployManager.UI.Principal
{
    /// <summary>
    /// Pantalla principal (sección 10 del análisis): ingreso de GOC, ambiente
    /// y sistema(s), disparo del despliegue y seguimiento en vivo. Una vez
    /// iniciado, no admite cancelación (decisión del cliente).
    /// </summary>
    [DesignerCategory("")]
    public sealed class MainForm : MetroForm
    {
        private readonly Bootstrapper _bootstrapper;
        private readonly SesionUsuario _sesion;
        private readonly Stopwatch _cronometro = new Stopwatch();
        private readonly Timer _timerReloj = new Timer { Interval = 1000 };
        private readonly Timer _timerProgreso = new Timer { Interval = 150 };
        private readonly List<CheckBox> _checkboxesSistema = new List<CheckBox>();

        private SideMenu _sideMenu;
        private RibbonBar _ribbon;
        private Label _lblGoc;
        private TextBoxX _txtGoc;
        private Label _lblAmbiente;
        private ComboBoxX _comboAmbiente;
        private Label _lblTema;
        private ComboBoxX _comboTema;
        private ButtonX _btnIniciarDespliegue;
        private Label _lblError;
        private GroupPanel _grupoSistemas;
        private GroupPanel _grupoProgreso;
        private Label _lblEtapaActual;
        private ProgressBarX _barraProgreso;
        private ListViewX _log;
        private StatusBarX _statusBar;
        private LoadingOverlay _overlay;
        private SuscripcionTema _suscripcionTemaPropia;

        private bool _progresoSubiendo = true;

        public MainForm(Bootstrapper bootstrapper, SesionUsuario sesion)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            _sesion = sesion ?? throw new ArgumentNullException(nameof(sesion));

            Text = $"GocDeployManager — {_sesion.Usuario.NombreVisible}";
            ClientSize = new Size(LogicalToDeviceUnits(900), LogicalToDeviceUnits(560));
            StartPosition = FormStartPosition.CenterScreen;

            // Única pantalla de la app que se puede agrandar/maximizar — el
            // resto de las pantallas (diálogos, Configuración, Historial)
            // mantiene su tamaño fijo. MinimumSize usa el tamaño de la
            // ventana ya calculado (Size, no ClientSize) para que nunca
            // pueda achicarse por debajo del diseño original.
            Redimensionable = true;

            ConstruirControles();
            CargarAmbientes();

            MinimumSize = Size;

            // _lblError/_lblEtapaActual/_checkboxesSistema/_lblGoc/etc. son
            // Label/CheckBox nativos creados a mano en esta pantalla, no
            // controles IThemedControl de DesktopComponents — sin esto, al
            // cambiar de tema en vivo desde el combo "Tema" quedan con el
            // color del tema anterior (en Oscuro, texto oscuro sobre fondo
            // oscuro, prácticamente invisible).
            _suscripcionTemaPropia = new SuscripcionTema(ReaplicarTemaControlesPropios);

            _timerReloj.Tick += TimerReloj_Tick;
            _timerProgreso.Tick += TimerProgreso_Tick;

            FormClosed += (s, e) =>
            {
                _timerReloj.Dispose();
                _timerProgreso.Dispose();
                _suscripcionTemaPropia.Dispose();
            };
        }

        private void ReaplicarTemaControlesPropios(Theme tema)
        {
            _lblGoc.ForeColor = tema.TextoSecundario;
            _lblAmbiente.ForeColor = tema.TextoSecundario;
            _lblTema.ForeColor = tema.TextoSecundario;
            _lblError.ForeColor = tema.Peligro;
            _lblEtapaActual.ForeColor = tema.TextoPrimario;

            foreach (var chk in _checkboxesSistema)
                chk.ForeColor = tema.TextoPrimario;
        }

        private void ConstruirControles()
        {
            var tema = ThemeManager.Actual;

            _sideMenu = new SideMenu();
            _sideMenu.AgregarItem("Principal", "principal");
            _sideMenu.AgregarItem("Configuración", "configuracion");
            _sideMenu.AgregarItem("Historial", "historial");
            _sideMenu.IndiceSeleccionado = 0;
            _sideMenu.ItemSeleccionado += SideMenu_ItemSeleccionado;

            _ribbon = new RibbonBar();
            _lblGoc = new Label
            {
                Text = "GOC:",
                AutoSize = true,
                ForeColor = tema.TextoSecundario,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            _txtGoc = new TextBoxX { Width = LogicalToDeviceUnits(130), TextoMarcador = "GOC-00000" };
            _lblAmbiente = new Label
            {
                Text = "Ambiente:",
                AutoSize = true,
                ForeColor = tema.TextoSecundario,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            _comboAmbiente = new ComboBoxX { Width = LogicalToDeviceUnits(160), DisplayMember = "Nombre" };
            _comboAmbiente.SelectedIndexChanged += (s, e) => ActualizarSistemasDisponibles();
            _btnIniciarDespliegue = new ButtonX { Text = "Iniciar despliegue", Width = LogicalToDeviceUnits(160) };
            _btnIniciarDespliegue.Click += BtnIniciarDespliegue_Click;

            _lblTema = new Label
            {
                Text = "Tema:",
                AutoSize = true,
                ForeColor = tema.TextoSecundario,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            _comboTema = new ComboBoxX { Width = LogicalToDeviceUnits(90) };
            _comboTema.Items.Add("Claro");
            _comboTema.Items.Add("Oscuro");
            _comboTema.SelectedItem = ThemeManager.Actual == Theme.Oscuro ? "Oscuro" : "Claro";
            _comboTema.SelectedIndexChanged += ComboTema_SelectedIndexChanged;

            _ribbon.AgregarControl(_lblGoc);
            _ribbon.AgregarControl(_txtGoc);
            _ribbon.AgregarControl(_lblAmbiente);
            _ribbon.AgregarControl(_comboAmbiente);
            _ribbon.AgregarSeparador();
            _ribbon.AgregarControl(_btnIniciarDespliegue);
            _ribbon.AgregarSeparador();
            _ribbon.AgregarControl(_lblTema);
            _ribbon.AgregarControl(_comboTema);

            if (!_sesion.Usuario.PuedeDesplegar)
            {
                _btnIniciarDespliegue.Enabled = false;
                _btnIniciarDespliegue.Text = "Sin permiso";
            }

            _lblError = new Label
            {
                Dock = DockStyle.Top,
                Height = LogicalToDeviceUnits(24),
                Padding = new Padding(LogicalToDeviceUnits(16), 0, 0, 0),
                ForeColor = tema.Peligro,
            };

            _grupoSistemas = new GroupPanel
            {
                Titulo = "Sistemas afectados",
                Dock = DockStyle.Top,
                Height = LogicalToDeviceUnits(76),
            };

            _grupoProgreso = new GroupPanel
            {
                Titulo = "Progreso del despliegue",
                Dock = DockStyle.Top,
                Height = LogicalToDeviceUnits(90),
            };

            // Los hijos de un GroupPanel se posicionan con Location absoluto,
            // así que hay que respetar Padding.Top a mano para no dibujar
            // encima de la banda de título del panel (mismo bug que hubo en
            // LoginForm con la barra de título de MetroForm).
            _lblEtapaActual = new Label
            {
                Location = new Point(_grupoProgreso.Padding.Left, _grupoProgreso.Padding.Top),
                Width = LogicalToDeviceUnits(600),
                Height = LogicalToDeviceUnits(20),
                ForeColor = tema.TextoPrimario,
                Text = "Sin despliegues en curso.",
            };
            _barraProgreso = new ProgressBarX
            {
                Location = new Point(_grupoProgreso.Padding.Left, _grupoProgreso.Padding.Top + LogicalToDeviceUnits(26)),
                Width = LogicalToDeviceUnits(600),
            };
            _grupoProgreso.Controls.Add(_lblEtapaActual);
            _grupoProgreso.Controls.Add(_barraProgreso);

            // El grupo es Dock=Top (se estira solo a lo ancho); su contenido
            // usaba un ancho fijo de 600px — ahora se reajusta a mano en cada
            // Resize para aprovechar el ancho real de la ventana (nunca por
            // debajo de esos 600px, para no romper el diseño original).
            _grupoProgreso.Resize += (s, e) =>
            {
                var anchoDisponible = Math.Max(
                    LogicalToDeviceUnits(600),
                    _grupoProgreso.ClientSize.Width - _grupoProgreso.Padding.Left - _grupoProgreso.Padding.Right);
                _lblEtapaActual.Width = anchoDisponible;
                _barraProgreso.Width = anchoDisponible;
            };

            _log = new ListViewX { Dock = DockStyle.Fill };
            _log.Columns.Add("Hora", LogicalToDeviceUnits(80));
            _log.Columns.Add("Mensaje", LogicalToDeviceUnits(400));

            _overlay = new LoadingOverlay();

            _statusBar = new StatusBarX
            {
                TextoIzquierda = "Listo.",
                TextoDerecha = string.Empty,
            };

            var contenedorCentral = new Panel { Dock = DockStyle.Fill };
            contenedorCentral.Controls.Add(_log);
            contenedorCentral.Controls.Add(_grupoProgreso);
            contenedorCentral.Controls.Add(_grupoSistemas);
            contenedorCentral.Controls.Add(_lblError);
            contenedorCentral.Controls.Add(_overlay);

            Controls.Add(contenedorCentral);
            Controls.Add(_statusBar);
            Controls.Add(_ribbon);
            Controls.Add(_sideMenu);

            _log.EstirarUltimaColumna();
        }

        private void CargarAmbientes()
        {
            var ambientes = _bootstrapper.Ambientes.ObtenerTodos();
            _comboAmbiente.Items.Clear();
            foreach (var ambiente in ambientes)
                _comboAmbiente.Items.Add(ambiente);

            if (_comboAmbiente.Items.Count > 0)
                _comboAmbiente.SelectedIndex = 0;
        }

        private void ComboTema_SelectedIndexChanged(object sender, EventArgs e)
        {
            ThemeManager.Establecer((string)_comboTema.SelectedItem == "Oscuro" ? Theme.Oscuro : Theme.Claro);
        }

        private void ActualizarSistemasDisponibles()
        {
            foreach (var chk in _checkboxesSistema)
                _grupoSistemas.Controls.Remove(chk);
            _checkboxesSistema.Clear();

            var ambiente = _comboAmbiente.SelectedItem as Ambiente;
            if (ambiente == null)
                return;

            var tema = ThemeManager.Actual;
            var x = _grupoSistemas.Padding.Left;
            var y = _grupoSistemas.Padding.Top;

            foreach (var ambienteSistema in ambiente.Sistemas)
            {
                var chk = new CheckBox
                {
                    Text = ambienteSistema.Sistema.Nombre,
                    Location = new Point(x, y),
                    AutoSize = true,
                    ForeColor = tema.TextoPrimario,
                    Tag = ambienteSistema.Sistema,
                    Checked = true,
                };

                _grupoSistemas.Controls.Add(chk);
                _checkboxesSistema.Add(chk);
                x += LogicalToDeviceUnits(110);
            }
        }

        private void SideMenu_ItemSeleccionado(object sender, SideMenu.ItemMenu item)
        {
            var etiqueta = (string)item.Etiqueta;
            if (etiqueta == "principal")
                return;

            _sideMenu.IndiceSeleccionado = 0;

            if (etiqueta == "configuracion")
            {
                if (!_sesion.Usuario.PuedeAdministrar)
                {
                    NotificationX.Mostrar(
                        "Sin permiso", "Solo un administrador puede acceder a Configuración.", TipoNotificacion.Advertencia);
                    return;
                }

                using (var configuracion = new ConfiguracionForm(_bootstrapper))
                    configuracion.ShowDialog(this);

                CargarAmbientes();
                return;
            }

            if (etiqueta == "historial")
            {
                using (var historial = new HistorialForm(_bootstrapper))
                    historial.ShowDialog(this);

                return;
            }
        }

        private async void BtnIniciarDespliegue_Click(object sender, EventArgs e)
        {
            _lblError.Text = string.Empty;

            var resultadoGoc = Goc.Crear(_txtGoc.Text);
            if (resultadoGoc.IsFailure)
            {
                _lblError.Text = resultadoGoc.Error;
                return;
            }

            if (!(_comboAmbiente.SelectedItem is Ambiente ambiente))
            {
                _lblError.Text = "Selecciona un ambiente.";
                return;
            }

            var sistemasSeleccionados = _checkboxesSistema
                .Where(c => c.Checked)
                .Select(c => (Sistema)c.Tag)
                .ToList();

            if (sistemasSeleccionados.Count == 0)
            {
                _lblError.Text = "Selecciona al menos un sistema.";
                return;
            }

            if (string.IsNullOrEmpty(_sesion.Usuario.UsuarioBitbucket) || string.IsNullOrEmpty(_sesion.ContrasenaBitbucketEnClaro))
            {
                _lblError.Text = "Tu usuario no tiene credenciales de Bitbucket configuradas.";
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

            await EjecutarDespliegueAsync(solicitud);
        }

        private async Task EjecutarDespliegueAsync(SolicitudDespliegue solicitud)
        {
            ConmutarControlesDurante(activo: true);
            _log.Items.Clear();
            _overlay.Mensaje = "Desplegando...";
            _overlay.Mostrar();
            _cronometro.Restart();
            _timerReloj.Start();
            _timerProgreso.Start();

            var progreso = new Progress<string>(mensaje =>
            {
                _statusBar.TextoIzquierda = mensaje;
                _lblEtapaActual.Text = mensaje;
                _log.AgregarLinea(ThemeManager.Actual.TextoSecundario, DateTime.Now.ToString("HH:mm:ss"), mensaje);
            });

            var resultado = await Task.Run(() => _bootstrapper.Orquestador.EjecutarDespliegue(solicitud, progreso));

            _timerReloj.Stop();
            _timerProgreso.Stop();
            _barraProgreso.Valor = resultado.IsSuccess ? 100 : _barraProgreso.Valor;
            _overlay.Ocultar();
            ConmutarControlesDurante(activo: false);

            if (resultado.IsSuccess)
            {
                _log.AgregarLinea(ThemeManager.Actual.Exito, DateTime.Now.ToString("HH:mm:ss"), "Despliegue completado con éxito.");
                _statusBar.TextoIzquierda = "Despliegue exitoso.";
                NotificationX.Mostrar(
                    "Despliegue exitoso",
                    $"{solicitud.Goc.Numero} se desplegó en {solicitud.Ambiente.Nombre}.",
                    TipoNotificacion.Exito);
            }
            else
            {
                _log.AgregarLinea(ThemeManager.Actual.Peligro, DateTime.Now.ToString("HH:mm:ss"), resultado.Error);
                _statusBar.TextoIzquierda = "Despliegue fallido.";
                NotificationX.Mostrar("Despliegue fallido", resultado.Error, TipoNotificacion.Error);
            }
        }

        private void ConmutarControlesDurante(bool activo)
        {
            _btnIniciarDespliegue.Enabled = !activo && _sesion.Usuario.PuedeDesplegar;
            _txtGoc.Enabled = !activo;
            _comboAmbiente.Enabled = !activo;
            foreach (var chk in _checkboxesSistema)
                chk.Enabled = !activo;
        }

        private void TimerReloj_Tick(object sender, EventArgs e)
        {
            _statusBar.TextoDerecha = $"Tiempo transcurrido: {_cronometro.Elapsed:hh\\:mm\\:ss}";
        }

        private void TimerProgreso_Tick(object sender, EventArgs e)
        {
            var valor = _barraProgreso.Valor;
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

            _barraProgreso.Valor = valor;
        }
    }
}
