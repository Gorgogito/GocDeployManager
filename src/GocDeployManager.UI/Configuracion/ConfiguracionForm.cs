using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DesktopComponents.Controls;
using DesktopComponents.Forms;
using DesktopComponents.Theming;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.UI.Configuracion
{
    /// <summary>
    /// Pantalla de configuración (sección 10 del análisis). Ambientes y
    /// Bitbucket ya están; Usuarios/General se agregan en fases posteriores
    /// como nuevas pestañas del mismo <see cref="TabControlX"/>.
    /// </summary>
    [DesignerCategory("")]
    public sealed class ConfiguracionForm : MetroForm
    {
        private readonly Bootstrapper _bootstrapper;
        private readonly List<string> _titulosPestanas = new List<string>();

        private List<Ambiente> _ambientesActuales = new List<Ambiente>();
        private List<ConfiguracionSistema> _sistemasActuales = new List<ConfiguracionSistema>();
        private List<AppUser> _usuariosActuales = new List<AppUser>();

        private Breadcrumb _breadcrumb;
        private TabControlX _tabs;

        private DataGridX _gridAmbientes;
        private PropertyGridX _propAmbiente;
        private Label _lblErrorAmbientes;

        private DataGridX _gridSistemas;
        private PropertyGridX _propSistema;
        private Label _lblErrorSistemas;

        private DataGridX _gridUsuarios;
        private Label _lblUsuarioSeleccionado;
        private ComboBoxX _comboRolUsuario;
        private ButtonX _btnActivarDesactivar;
        private Label _lblBitbucketEstado;
        private Label _lblErrorUsuarios;

        private TextBoxX _txtRutaClonado;
        private TextBoxX _txtRutaTemporales;
        private TextBoxX _txtRutaLogs;
        private TextBoxX _txtRutaConfiguracion;
        private TextBoxX _txtRutaBaseDatos;
        private Label _lblErrorGeneral;

        public ConfiguracionForm(Bootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));

            Text = "GocDeployManager — Configuración";
            ClientSize = new Size(LogicalToDeviceUnits(780), LogicalToDeviceUnits(520));
            StartPosition = FormStartPosition.CenterParent;

            ConstruirControles();
            CargarAmbientes();
            CargarSistemas();
            CargarUsuarios();
            CargarRutasGenerales();
        }

        private void ConstruirControles()
        {
            _breadcrumb = new Breadcrumb { Dock = DockStyle.Top };

            _tabs = new TabControlX { Dock = DockStyle.Fill };
            _tabs.PaginaCambiada += (s, e) =>
                _breadcrumb.EstablecerRuta("Configuración", _titulosPestanas[_tabs.IndiceSeleccionado]);

            var panelAmbientes = new Panel();
            ConstruirTabAmbientes(panelAmbientes);
            _tabs.AgregarPagina("Ambientes", panelAmbientes);
            _titulosPestanas.Add("Ambientes");

            var panelBitbucket = new Panel();
            ConstruirTabBitbucket(panelBitbucket);
            _tabs.AgregarPagina("Bitbucket", panelBitbucket);
            _titulosPestanas.Add("Bitbucket");

            var panelUsuarios = new Panel();
            ConstruirTabUsuarios(panelUsuarios);
            _tabs.AgregarPagina("Usuarios", panelUsuarios);
            _titulosPestanas.Add("Usuarios");

            var panelGeneral = new Panel();
            ConstruirTabGeneral(panelGeneral);
            _tabs.AgregarPagina("General", panelGeneral);
            _titulosPestanas.Add("General");

            _breadcrumb.EstablecerRuta("Configuración", _titulosPestanas[0]);

            Controls.Add(_tabs);
            Controls.Add(_breadcrumb);
        }

        // ---------- Ambientes ----------

        private void ConstruirTabAmbientes(Panel contenedor)
        {
            var tema = ThemeManager.Actual;
            var margen = LogicalToDeviceUnits(12);

            var panelBotones = new Panel { Dock = DockStyle.Bottom, Height = LogicalToDeviceUnits(56) };

            var btnNuevo = new ButtonX { Text = "Nuevo", Width = LogicalToDeviceUnits(100), Variante = VarianteButtonX.Secundario };
            var btnGuardar = new ButtonX { Text = "Guardar", Width = LogicalToDeviceUnits(100), Variante = VarianteButtonX.Primario };
            var btnEliminar = new ButtonX { Text = "Eliminar", Width = LogicalToDeviceUnits(100), Variante = VarianteButtonX.Peligro };

            btnNuevo.Location = new Point(margen, LogicalToDeviceUnits(12));
            btnGuardar.Location = new Point(btnNuevo.Right + margen, LogicalToDeviceUnits(12));
            btnEliminar.Location = new Point(btnGuardar.Right + margen, LogicalToDeviceUnits(12));

            btnNuevo.Click += BtnNuevoAmbiente_Click;
            btnGuardar.Click += BtnGuardarAmbiente_Click;
            btnEliminar.Click += BtnEliminarAmbiente_Click;

            _lblErrorAmbientes = new Label
            {
                Location = new Point(btnEliminar.Right + margen, LogicalToDeviceUnits(16)),
                Size = new Size(LogicalToDeviceUnits(380), LogicalToDeviceUnits(24)),
                ForeColor = tema.Peligro,
            };

            panelBotones.Controls.Add(btnNuevo);
            panelBotones.Controls.Add(btnGuardar);
            panelBotones.Controls.Add(btnEliminar);
            panelBotones.Controls.Add(_lblErrorAmbientes);

            _gridAmbientes = new DataGridX { Dock = DockStyle.Left, Width = LogicalToDeviceUnits(300) };
            _gridAmbientes.Columns.Add("Nombre", "Nombre");
            _gridAmbientes.Columns.Add("Sistemas", "Sistemas");
            _gridAmbientes.SelectionChanged += GridAmbientes_SelectionChanged;

            _propAmbiente = new PropertyGridX { Dock = DockStyle.Fill };

            contenedor.Controls.Add(_propAmbiente);
            contenedor.Controls.Add(_gridAmbientes);
            contenedor.Controls.Add(panelBotones);
        }

        private void CargarAmbientes()
        {
            _ambientesActuales = _bootstrapper.Ambientes.ObtenerTodos().ToList();

            _gridAmbientes.Rows.Clear();
            foreach (var ambiente in _ambientesActuales)
            {
                var sistemas = string.Join(", ", ambiente.Sistemas.Select(s => s.Sistema.Codigo));
                var indice = _gridAmbientes.Rows.Add(ambiente.Nombre, sistemas);
                _gridAmbientes.Rows[indice].Tag = ambiente;
            }

            // DataGridView selecciona la primera fila automáticamente al cargar
            // datos, pero no dispara SelectionChanged para esa selección
            // inicial — sin esto, el PropertyGridX queda vacío hasta que el
            // usuario cambia de fila manualmente.
            _propAmbiente.SelectedObject = _gridAmbientes.CurrentRow?.Tag is Ambiente actual
                ? MapearAEditable(actual)
                : null;
        }

        private void GridAmbientes_SelectionChanged(object sender, EventArgs e)
        {
            if (_gridAmbientes.CurrentRow?.Tag is Ambiente ambiente)
                _propAmbiente.SelectedObject = MapearAEditable(ambiente);
        }

        private static AmbienteEditable MapearAEditable(Ambiente ambiente) => new AmbienteEditable
        {
            Nombre = ambiente.Nombre,
            Sistemas = ambiente.Sistemas.Select(s => new AmbienteSistemaEditable
            {
                Codigo = s.Sistema.Codigo,
                Nombre = s.Sistema.Nombre,
                RutaDestino = s.RutaDestino,
            }).ToList(),
        };

        private void BtnNuevoAmbiente_Click(object sender, EventArgs e)
        {
            _lblErrorAmbientes.Text = string.Empty;
            _gridAmbientes.ClearSelection();
            _propAmbiente.SelectedObject = new AmbienteEditable();
        }

        private void BtnGuardarAmbiente_Click(object sender, EventArgs e)
        {
            _lblErrorAmbientes.Text = string.Empty;

            if (!(_propAmbiente.SelectedObject is AmbienteEditable editable))
            {
                _lblErrorAmbientes.Text = "Selecciona o crea un ambiente primero.";
                return;
            }

            if (string.IsNullOrWhiteSpace(editable.Nombre))
            {
                _lblErrorAmbientes.Text = "El nombre del ambiente es obligatorio.";
                return;
            }

            if (editable.Sistemas == null || editable.Sistemas.Count == 0)
            {
                _lblErrorAmbientes.Text = "Agrega al menos un sistema (columna Sistemas, botón \"...\").";
                return;
            }

            Ambiente nuevoAmbiente;
            try
            {
                var sistemas = editable.Sistemas.Select(s => new AmbienteSistema(
                    new Sistema(s.Codigo, string.IsNullOrWhiteSpace(s.Nombre) ? s.Codigo : s.Nombre),
                    s.RutaDestino));

                nuevoAmbiente = new Ambiente(editable.Nombre, sistemas);
            }
            catch (ArgumentException ex)
            {
                _lblErrorAmbientes.Text = ex.Message;
                return;
            }

            var listaActualizada = _ambientesActuales
                .Where(a => !string.Equals(a.Nombre, nuevoAmbiente.Nombre, StringComparison.OrdinalIgnoreCase))
                .Append(nuevoAmbiente)
                .ToList();

            var resultado = _bootstrapper.Ambientes.Guardar(listaActualizada);
            if (resultado.IsFailure)
            {
                _lblErrorAmbientes.Text = resultado.Error;
                return;
            }

            CargarAmbientes();
            NotificationX.Mostrar("Ambiente guardado", $"'{nuevoAmbiente.Nombre}' se guardó correctamente.", TipoNotificacion.Exito);
        }

        private void BtnEliminarAmbiente_Click(object sender, EventArgs e)
        {
            _lblErrorAmbientes.Text = string.Empty;

            if (!(_gridAmbientes.CurrentRow?.Tag is Ambiente ambiente))
            {
                _lblErrorAmbientes.Text = "Selecciona un ambiente de la lista para eliminar.";
                return;
            }

            var confirmacion = MessageBoxX.Preguntar(
                this, "Eliminar ambiente", $"¿Eliminar el ambiente '{ambiente.Nombre}'? Esta acción no se puede deshacer.");

            if (confirmacion != ResultadoMessageBoxX.Si)
                return;

            var listaActualizada = _ambientesActuales
                .Where(a => !string.Equals(a.Nombre, ambiente.Nombre, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var resultado = _bootstrapper.Ambientes.Guardar(listaActualizada);
            if (resultado.IsFailure)
            {
                _lblErrorAmbientes.Text = resultado.Error;
                return;
            }

            _propAmbiente.SelectedObject = null;
            CargarAmbientes();
        }

        // ---------- Bitbucket (Sistemas.json: repo + build steps por sistema) ----------

        private void ConstruirTabBitbucket(Panel contenedor)
        {
            var tema = ThemeManager.Actual;
            var margen = LogicalToDeviceUnits(12);

            var panelBotones = new Panel { Dock = DockStyle.Bottom, Height = LogicalToDeviceUnits(56) };

            var btnNuevo = new ButtonX { Text = "Nuevo", Width = LogicalToDeviceUnits(100), Variante = VarianteButtonX.Secundario };
            var btnGuardar = new ButtonX { Text = "Guardar", Width = LogicalToDeviceUnits(100), Variante = VarianteButtonX.Primario };
            var btnEliminar = new ButtonX { Text = "Eliminar", Width = LogicalToDeviceUnits(100), Variante = VarianteButtonX.Peligro };

            btnNuevo.Location = new Point(margen, LogicalToDeviceUnits(12));
            btnGuardar.Location = new Point(btnNuevo.Right + margen, LogicalToDeviceUnits(12));
            btnEliminar.Location = new Point(btnGuardar.Right + margen, LogicalToDeviceUnits(12));

            btnNuevo.Click += BtnNuevoSistema_Click;
            btnGuardar.Click += BtnGuardarSistema_Click;
            btnEliminar.Click += BtnEliminarSistema_Click;

            _lblErrorSistemas = new Label
            {
                Location = new Point(btnEliminar.Right + margen, LogicalToDeviceUnits(16)),
                Size = new Size(LogicalToDeviceUnits(380), LogicalToDeviceUnits(24)),
                ForeColor = tema.Peligro,
            };

            panelBotones.Controls.Add(btnNuevo);
            panelBotones.Controls.Add(btnGuardar);
            panelBotones.Controls.Add(btnEliminar);
            panelBotones.Controls.Add(_lblErrorSistemas);

            _gridSistemas = new DataGridX { Dock = DockStyle.Left, Width = LogicalToDeviceUnits(300) };
            _gridSistemas.Columns.Add("Codigo", "Sistema");
            _gridSistemas.Columns.Add("Repositorio", "Repositorio");
            _gridSistemas.SelectionChanged += GridSistemas_SelectionChanged;

            _propSistema = new PropertyGridX { Dock = DockStyle.Fill };

            contenedor.Controls.Add(_propSistema);
            contenedor.Controls.Add(_gridSistemas);
            contenedor.Controls.Add(panelBotones);
        }

        private void CargarSistemas()
        {
            _sistemasActuales = _bootstrapper.Sistemas.ObtenerTodos().ToList();

            _gridSistemas.Rows.Clear();
            foreach (var configuracion in _sistemasActuales)
            {
                var indice = _gridSistemas.Rows.Add(configuracion.Sistema.Codigo, configuracion.RepositorioUrl);
                _gridSistemas.Rows[indice].Tag = configuracion;
            }

            _propSistema.SelectedObject = _gridSistemas.CurrentRow?.Tag is ConfiguracionSistema actual
                ? MapearAEditable(actual)
                : null;
        }

        private void GridSistemas_SelectionChanged(object sender, EventArgs e)
        {
            if (_gridSistemas.CurrentRow?.Tag is ConfiguracionSistema configuracion)
                _propSistema.SelectedObject = MapearAEditable(configuracion);
        }

        private static ConfiguracionSistemaEditable MapearAEditable(ConfiguracionSistema configuracion) => new ConfiguracionSistemaEditable
        {
            Codigo = configuracion.Sistema.Codigo,
            Nombre = configuracion.Sistema.Nombre,
            RepositorioUrl = configuracion.RepositorioUrl,
            CarpetaPrecompilada = configuracion.CarpetaPrecompilada,
            Pasos = configuracion.SecuenciaDeBuild.Pasos.Select(p => new PasoDeBuildEditable
            {
                Orden = p.Orden,
                CarpetaProyecto = p.CarpetaProyecto,
                ParametrosMsBuild = p.ParametrosMsBuild,
            }).ToList(),
        };

        private void BtnNuevoSistema_Click(object sender, EventArgs e)
        {
            _lblErrorSistemas.Text = string.Empty;
            _gridSistemas.ClearSelection();
            _propSistema.SelectedObject = new ConfiguracionSistemaEditable();
        }

        private void BtnGuardarSistema_Click(object sender, EventArgs e)
        {
            _lblErrorSistemas.Text = string.Empty;

            if (!(_propSistema.SelectedObject is ConfiguracionSistemaEditable editable))
            {
                _lblErrorSistemas.Text = "Selecciona o crea un sistema primero.";
                return;
            }

            if (string.IsNullOrWhiteSpace(editable.Codigo))
            {
                _lblErrorSistemas.Text = "El código de sistema es obligatorio.";
                return;
            }

            if (string.IsNullOrWhiteSpace(editable.RepositorioUrl))
            {
                _lblErrorSistemas.Text = "La URL del repositorio es obligatoria.";
                return;
            }

            if (string.IsNullOrWhiteSpace(editable.CarpetaPrecompilada))
            {
                _lblErrorSistemas.Text = "La carpeta precompilada es obligatoria.";
                return;
            }

            if (editable.Pasos == null || editable.Pasos.Count == 0)
            {
                _lblErrorSistemas.Text = "Agrega al menos un paso de build (columna Pasos, botón \"...\").";
                return;
            }

            ConfiguracionSistema nuevaConfiguracion;
            try
            {
                var sistema = new Sistema(editable.Codigo, string.IsNullOrWhiteSpace(editable.Nombre) ? editable.Codigo : editable.Nombre);
                var pasos = editable.Pasos.Select(p => new PasoDeBuild(p.Orden, p.CarpetaProyecto, p.ParametrosMsBuild));
                var secuencia = new SecuenciaDeBuild(sistema, pasos);

                nuevaConfiguracion = new ConfiguracionSistema(sistema, editable.RepositorioUrl, editable.CarpetaPrecompilada, secuencia);
            }
            catch (ArgumentException ex)
            {
                _lblErrorSistemas.Text = ex.Message;
                return;
            }

            var listaActualizada = _sistemasActuales
                .Where(c => !string.Equals(c.Sistema.Codigo, nuevaConfiguracion.Sistema.Codigo, StringComparison.OrdinalIgnoreCase))
                .Append(nuevaConfiguracion)
                .ToList();

            var resultado = _bootstrapper.Sistemas.Guardar(listaActualizada);
            if (resultado.IsFailure)
            {
                _lblErrorSistemas.Text = resultado.Error;
                return;
            }

            CargarSistemas();
            NotificationX.Mostrar(
                "Sistema guardado", $"'{nuevaConfiguracion.Sistema.Codigo}' se guardó correctamente.", TipoNotificacion.Exito);
        }

        private void BtnEliminarSistema_Click(object sender, EventArgs e)
        {
            _lblErrorSistemas.Text = string.Empty;

            if (!(_gridSistemas.CurrentRow?.Tag is ConfiguracionSistema configuracion))
            {
                _lblErrorSistemas.Text = "Selecciona un sistema de la lista para eliminar.";
                return;
            }

            var confirmacion = MessageBoxX.Preguntar(
                this, "Eliminar sistema",
                $"¿Eliminar la configuración de '{configuracion.Sistema.Codigo}'? Los ambientes que lo referencian quedarían sin poder desplegarlo.");

            if (confirmacion != ResultadoMessageBoxX.Si)
                return;

            var listaActualizada = _sistemasActuales
                .Where(c => !string.Equals(c.Sistema.Codigo, configuracion.Sistema.Codigo, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var resultado = _bootstrapper.Sistemas.Guardar(listaActualizada);
            if (resultado.IsFailure)
            {
                _lblErrorSistemas.Text = resultado.Error;
                return;
            }

            _propSistema.SelectedObject = null;
            CargarSistemas();
        }

        // ---------- Usuarios ----------

        private void ConstruirTabUsuarios(Panel contenedor)
        {
            var tema = ThemeManager.Actual;
            var margen = LogicalToDeviceUnits(12);

            var panelBotones = new Panel { Dock = DockStyle.Bottom, Height = LogicalToDeviceUnits(56) };
            var btnNuevo = new ButtonX { Text = "Nuevo usuario", Width = LogicalToDeviceUnits(140), Variante = VarianteButtonX.Secundario };
            btnNuevo.Location = new Point(margen, LogicalToDeviceUnits(12));
            btnNuevo.Click += BtnNuevoUsuario_Click;

            _lblErrorUsuarios = new Label
            {
                Location = new Point(btnNuevo.Right + margen, LogicalToDeviceUnits(16)),
                Size = new Size(LogicalToDeviceUnits(500), LogicalToDeviceUnits(24)),
                ForeColor = tema.Peligro,
            };

            panelBotones.Controls.Add(btnNuevo);
            panelBotones.Controls.Add(_lblErrorUsuarios);

            _gridUsuarios = new DataGridX { Dock = DockStyle.Left, Width = LogicalToDeviceUnits(420) };
            _gridUsuarios.Columns.Add("Usuario", "Usuario");
            _gridUsuarios.Columns.Add("NombreVisible", "Nombre visible");
            _gridUsuarios.Columns.Add("Rol", "Rol");
            _gridUsuarios.Columns.Add("Activo", "Activo");
            _gridUsuarios.Columns.Add("Bitbucket", "Bitbucket");
            _gridUsuarios.SelectionChanged += (s, e) => ActualizarPanelDetalleUsuario(_gridUsuarios.CurrentRow?.Tag as AppUser);

            var panelDetalle = new Panel { Dock = DockStyle.Fill };
            var x = LogicalToDeviceUnits(16);
            var y = LogicalToDeviceUnits(16);

            _lblUsuarioSeleccionado = new Label
            {
                Location = new Point(x, y),
                Size = new Size(LogicalToDeviceUnits(320), LogicalToDeviceUnits(24)),
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = tema.TextoPrimario,
                Text = "Selecciona un usuario de la lista.",
            };
            y += LogicalToDeviceUnits(40);

            _comboRolUsuario = new ComboBoxX { Location = new Point(x, y), Width = LogicalToDeviceUnits(160) };
            foreach (RolUsuario rol in Enum.GetValues(typeof(RolUsuario)))
                _comboRolUsuario.Items.Add(rol);

            var btnGuardarRol = new ButtonX
            {
                Text = "Guardar rol",
                Width = LogicalToDeviceUnits(120),
                Location = new Point(_comboRolUsuario.Right + margen, y),
                Variante = VarianteButtonX.Primario,
            };
            btnGuardarRol.Click += BtnGuardarRol_Click;
            y += LogicalToDeviceUnits(46);

            _btnActivarDesactivar = new ButtonX
            {
                Text = "Desactivar",
                Width = LogicalToDeviceUnits(140),
                Location = new Point(x, y),
                Variante = VarianteButtonX.Secundario,
                Enabled = false,
            };
            _btnActivarDesactivar.Click += BtnActivarDesactivar_Click;
            y += LogicalToDeviceUnits(46);

            var btnResetear = new ButtonX
            {
                Text = "Resetear contraseña",
                Width = LogicalToDeviceUnits(180),
                Location = new Point(x, y),
                Variante = VarianteButtonX.Secundario,
            };
            btnResetear.Click += BtnResetearContrasena_Click;
            y += LogicalToDeviceUnits(46);

            var btnBitbucket = new ButtonX
            {
                Text = "Configurar credenciales Bitbucket",
                Width = LogicalToDeviceUnits(260),
                Location = new Point(x, y),
                Variante = VarianteButtonX.Secundario,
            };
            btnBitbucket.Click += BtnConfigurarBitbucket_Click;
            y += LogicalToDeviceUnits(46);

            _lblBitbucketEstado = new Label
            {
                Location = new Point(x, y),
                Size = new Size(LogicalToDeviceUnits(320), LogicalToDeviceUnits(24)),
                ForeColor = tema.TextoSecundario,
            };

            panelDetalle.Controls.Add(_lblUsuarioSeleccionado);
            panelDetalle.Controls.Add(_comboRolUsuario);
            panelDetalle.Controls.Add(btnGuardarRol);
            panelDetalle.Controls.Add(_btnActivarDesactivar);
            panelDetalle.Controls.Add(btnResetear);
            panelDetalle.Controls.Add(btnBitbucket);
            panelDetalle.Controls.Add(_lblBitbucketEstado);

            contenedor.Controls.Add(panelDetalle);
            contenedor.Controls.Add(_gridUsuarios);
            contenedor.Controls.Add(panelBotones);
        }

        private void CargarUsuarios()
        {
            _usuariosActuales = _bootstrapper.Usuarios.ObtenerTodos().ToList();

            _gridUsuarios.Rows.Clear();
            foreach (var usuario in _usuariosActuales)
            {
                var indice = _gridUsuarios.Rows.Add(
                    usuario.NombreUsuario,
                    usuario.NombreVisible,
                    usuario.Rol.ToString(),
                    usuario.Activo ? "Sí" : "No",
                    string.IsNullOrEmpty(usuario.UsuarioBitbucket) ? "Sin configurar" : usuario.UsuarioBitbucket);
                _gridUsuarios.Rows[indice].Tag = usuario;
            }

            ActualizarPanelDetalleUsuario(_gridUsuarios.CurrentRow?.Tag as AppUser);
        }

        private void ActualizarPanelDetalleUsuario(AppUser usuario)
        {
            if (usuario == null)
            {
                _lblUsuarioSeleccionado.Text = "Selecciona un usuario de la lista.";
                _comboRolUsuario.SelectedIndex = -1;
                _btnActivarDesactivar.Text = "Desactivar";
                _btnActivarDesactivar.Enabled = false;
                _lblBitbucketEstado.Text = string.Empty;
                return;
            }

            _lblUsuarioSeleccionado.Text = $"{usuario.NombreVisible} ({usuario.NombreUsuario})";
            _comboRolUsuario.SelectedItem = usuario.Rol;
            _btnActivarDesactivar.Text = usuario.Activo ? "Desactivar" : "Activar";
            _btnActivarDesactivar.Enabled = true;
            _lblBitbucketEstado.Text = string.IsNullOrEmpty(usuario.UsuarioBitbucket)
                ? "Bitbucket: sin configurar."
                : $"Bitbucket: {usuario.UsuarioBitbucket}";
        }

        private void BtnNuevoUsuario_Click(object sender, EventArgs e)
        {
            _lblErrorUsuarios.Text = string.Empty;

            using (var dialogo = new NuevoUsuarioDialog(_bootstrapper))
                dialogo.ShowDialog(this);

            CargarUsuarios();
        }

        private void BtnGuardarRol_Click(object sender, EventArgs e)
        {
            _lblErrorUsuarios.Text = string.Empty;

            if (!(_gridUsuarios.CurrentRow?.Tag is AppUser usuario))
            {
                _lblErrorUsuarios.Text = "Selecciona un usuario de la lista.";
                return;
            }

            if (!(_comboRolUsuario.SelectedItem is RolUsuario nuevoRol))
            {
                _lblErrorUsuarios.Text = "Selecciona un rol.";
                return;
            }

            var resultado = _bootstrapper.Usuarios.CambiarRol(usuario.NombreUsuario, nuevoRol);
            if (resultado.IsFailure)
            {
                _lblErrorUsuarios.Text = resultado.Error;
                return;
            }

            CargarUsuarios();
            NotificationX.Mostrar("Rol actualizado", $"'{usuario.NombreUsuario}' ahora es {nuevoRol}.", TipoNotificacion.Exito);
        }

        private void BtnActivarDesactivar_Click(object sender, EventArgs e)
        {
            _lblErrorUsuarios.Text = string.Empty;

            if (!(_gridUsuarios.CurrentRow?.Tag is AppUser usuario))
            {
                _lblErrorUsuarios.Text = "Selecciona un usuario de la lista.";
                return;
            }

            var resultado = _bootstrapper.Usuarios.CambiarEstado(usuario.NombreUsuario, !usuario.Activo);
            if (resultado.IsFailure)
            {
                _lblErrorUsuarios.Text = resultado.Error;
                return;
            }

            CargarUsuarios();
        }

        private void BtnResetearContrasena_Click(object sender, EventArgs e)
        {
            _lblErrorUsuarios.Text = string.Empty;

            if (!(_gridUsuarios.CurrentRow?.Tag is AppUser usuario))
            {
                _lblErrorUsuarios.Text = "Selecciona un usuario de la lista.";
                return;
            }

            using (var dialogo = new ResetearContrasenaDialog(_bootstrapper, usuario.NombreUsuario))
                dialogo.ShowDialog(this);
        }

        private void BtnConfigurarBitbucket_Click(object sender, EventArgs e)
        {
            _lblErrorUsuarios.Text = string.Empty;

            if (!(_gridUsuarios.CurrentRow?.Tag is AppUser usuario))
            {
                _lblErrorUsuarios.Text = "Selecciona un usuario de la lista.";
                return;
            }

            using (var dialogo = new CredencialesBitbucketDialog(_bootstrapper, usuario.NombreUsuario))
                dialogo.ShowDialog(this);

            CargarUsuarios();
        }

        // ---------- General ----------

        private void ConstruirTabGeneral(Panel contenedor)
        {
            var tema = ThemeManager.Actual;
            var margen = LogicalToDeviceUnits(20);
            var ancho = LogicalToDeviceUnits(560);
            var salto = LogicalToDeviceUnits(56);
            var y = LogicalToDeviceUnits(16);

            Label CrearEtiqueta(string texto, int yPos)
            {
                var etiqueta = new Label
                {
                    Text = texto,
                    Location = new Point(margen, yPos),
                    Size = new Size(ancho, LogicalToDeviceUnits(18)),
                    ForeColor = tema.TextoSecundario,
                    Font = new Font("Segoe UI", 8.5f),
                };
                contenedor.Controls.Add(etiqueta);
                return etiqueta;
            }

            CrearEtiqueta("Ruta de clonado (donde se descargan los GOCs)", y);
            y += LogicalToDeviceUnits(20);
            _txtRutaClonado = new TextBoxX { Location = new Point(margen, y), Width = ancho };
            contenedor.Controls.Add(_txtRutaClonado);
            y += salto;

            CrearEtiqueta("Ruta de archivos temporales", y);
            y += LogicalToDeviceUnits(20);
            _txtRutaTemporales = new TextBoxX { Location = new Point(margen, y), Width = ancho };
            contenedor.Controls.Add(_txtRutaTemporales);
            y += salto;

            CrearEtiqueta("Ruta de logs", y);
            y += LogicalToDeviceUnits(20);
            _txtRutaLogs = new TextBoxX { Location = new Point(margen, y), Width = ancho };
            contenedor.Controls.Add(_txtRutaLogs);
            y += salto;

            CrearEtiqueta("Ruta de configuración (Ambientes.json, Sistemas.json, ExclusionRules.json)", y);
            y += LogicalToDeviceUnits(20);
            _txtRutaConfiguracion = new TextBoxX { Location = new Point(margen, y), Width = ancho };
            contenedor.Controls.Add(_txtRutaConfiguracion);
            y += salto;

            CrearEtiqueta("Ruta del historial (base de datos)", y);
            y += LogicalToDeviceUnits(20);
            _txtRutaBaseDatos = new TextBoxX { Location = new Point(margen, y), Width = ancho };
            contenedor.Controls.Add(_txtRutaBaseDatos);
            y += salto;

            _lblErrorGeneral = new Label
            {
                Location = new Point(margen, y),
                Size = new Size(ancho, LogicalToDeviceUnits(36)),
                ForeColor = tema.Peligro,
            };
            contenedor.Controls.Add(_lblErrorGeneral);
            y += LogicalToDeviceUnits(40);

            var btnGuardar = new ButtonX
            {
                Text = "Guardar",
                Location = new Point(margen, y),
                Width = LogicalToDeviceUnits(160),
                Variante = VarianteButtonX.Primario,
            };
            btnGuardar.Click += BtnGuardarGeneral_Click;
            contenedor.Controls.Add(btnGuardar);
        }

        private void CargarRutasGenerales()
        {
            _txtRutaClonado.Text = _bootstrapper.RutaClonado;
            _txtRutaTemporales.Text = _bootstrapper.RutaTemporales;
            _txtRutaLogs.Text = _bootstrapper.RutaLogs;
            _txtRutaConfiguracion.Text = _bootstrapper.RutaConfiguracion;
            _txtRutaBaseDatos.Text = _bootstrapper.RutaBaseDatos;
        }

        private void BtnGuardarGeneral_Click(object sender, EventArgs e)
        {
            _lblErrorGeneral.Text = string.Empty;

            var rutaClonado = _txtRutaClonado.Text.Trim();
            var rutaTemporales = _txtRutaTemporales.Text.Trim();
            var rutaLogs = _txtRutaLogs.Text.Trim();
            var rutaConfiguracion = _txtRutaConfiguracion.Text.Trim();
            var rutaBaseDatos = _txtRutaBaseDatos.Text.Trim();

            if (string.IsNullOrWhiteSpace(rutaClonado) || string.IsNullOrWhiteSpace(rutaTemporales) ||
                string.IsNullOrWhiteSpace(rutaLogs) || string.IsNullOrWhiteSpace(rutaConfiguracion) ||
                string.IsNullOrWhiteSpace(rutaBaseDatos))
            {
                _lblErrorGeneral.Text = "Todas las rutas son obligatorias.";
                return;
            }

            try
            {
                _bootstrapper.GuardarRutasGenerales(rutaClonado, rutaTemporales, rutaLogs, rutaConfiguracion, rutaBaseDatos);
            }
            catch (Exception ex)
            {
                _lblErrorGeneral.Text = $"No se pudieron guardar las rutas: {ex.Message}";
                return;
            }

            NotificationX.Mostrar(
                "Rutas guardadas",
                "Los cambios se aplicarán la próxima vez que abras la aplicación.",
                TipoNotificacion.Info);
        }
    }
}
