using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GocDeployManager.Domain.Entities;
using GocDeployManager.UI.Ventanas;
using MaterialDesignThemes.Wpf;

namespace GocDeployManager.UI.Configuracion
{
    public partial class ConfiguracionWindow : VentanaBase
    {
        private readonly Bootstrapper _bootstrapper;

        // Datos cargados del repositorio
        private List<Ambiente> _ambientesActuales = new List<Ambiente>();
        private List<ConfiguracionSistema> _sistemasActuales = new List<ConfiguracionSistema>();
        private List<AppUser> _usuariosActuales = new List<AppUser>();

        // Colecciones para los sub-grids editables
        private readonly ObservableCollection<AmbienteSistemaEditable> _ambienteSistemas =
            new ObservableCollection<AmbienteSistemaEditable>();
        private readonly ObservableCollection<PasoDeBuildEditable> _sistemasPasos =
            new ObservableCollection<PasoDeBuildEditable>();

        // Snackbar
        private SnackbarMessageQueue _snackbarQueue;

        // ─── Clases de fila para los DataGrids de selección ───────────────
        private sealed class AmbienteRow
        {
            public string Nombre { get; set; }
            public string SistemasTexto { get; set; }
            public Ambiente Entidad { get; set; }
        }

        private sealed class SistemaRow
        {
            public string Codigo { get; set; }
            public string RepositorioCorto { get; set; }
            public ConfiguracionSistema Entidad { get; set; }
        }

        private sealed class UsuarioRow
        {
            public string NombreUsuario { get; set; }
            public string NombreVisible { get; set; }
            public string Rol { get; set; }
            public string Activo { get; set; }
            public string Bitbucket { get; set; }
            public AppUser Entidad { get; set; }
        }

        public ConfiguracionWindow(Bootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            InitializeComponent();
            Loaded += ConfiguracionWindow_Loaded;
        }

        private void ConfiguracionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _snackbarQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
            snackbar.MessageQueue = _snackbarQueue;

            // Conectar colecciones ObservableCollection a los DataGrids de sub-edición
            gridAmbienteSistemas.ItemsSource = _ambienteSistemas;
            gridSistemasPasos.ItemsSource = _sistemasPasos;

            // Poblar el ComboBox de rol de usuarios
            foreach (RolUsuario rol in Enum.GetValues(typeof(RolUsuario)))
                comboRolUsuario.Items.Add(rol);

            // Cargar datos de todas las pestañas
            CargarAmbientes();
            CargarSistemas();
            CargarUsuarios();
            CargarRutasGenerales();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // No requiere acción — los datos se cargan en Loaded
        }

        // ═══════════════════════════════════════════════════════════════════
        // TAB: AMBIENTES
        // ═══════════════════════════════════════════════════════════════════

        private void CargarAmbientes()
        {
            _ambientesActuales = _bootstrapper.Ambientes.ObtenerTodos().ToList();
            var filas = _ambientesActuales.Select(a => new AmbienteRow
            {
                Nombre = a.Nombre,
                SistemasTexto = string.Join(", ", a.Sistemas.Select(s => s.Sistema.Codigo)),
                Entidad = a,
            }).ToList();

            gridAmbientes.ItemsSource = filas;

            if (filas.Any())
                gridAmbientes.SelectedIndex = 0;
            else
                MostrarFormularioAmbiente(null);
        }

        private void GridAmbientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var fila = gridAmbientes.SelectedItem as AmbienteRow;
            MostrarFormularioAmbiente(fila?.Entidad);
        }

        private void MostrarFormularioAmbiente(Ambiente ambiente)
        {
            if (ambiente == null)
            {
                panelFormAmbiente.Visibility = Visibility.Collapsed;
                lblPlaceholderAmbiente.Visibility = Visibility.Visible;
                txtAmbienteNombre.Text = string.Empty;
                _ambienteSistemas.Clear();
                return;
            }

            lblPlaceholderAmbiente.Visibility = Visibility.Collapsed;
            panelFormAmbiente.Visibility = Visibility.Visible;

            txtAmbienteNombre.Text = ambiente.Nombre;
            _ambienteSistemas.Clear();
            foreach (var s in ambiente.Sistemas)
                _ambienteSistemas.Add(new AmbienteSistemaEditable
                {
                    Codigo = s.Sistema.Codigo,
                    Nombre = s.Sistema.Nombre,
                    RutaDestino = s.RutaDestino,
                });
        }

        private void BtnNuevoAmbiente_Click(object sender, RoutedEventArgs e)
        {
            MostrarErrorAmbientes(string.Empty);
            gridAmbientes.SelectedItem = null;
            lblPlaceholderAmbiente.Visibility = Visibility.Collapsed;
            panelFormAmbiente.Visibility = Visibility.Visible;
            txtAmbienteNombre.Text = string.Empty;
            _ambienteSistemas.Clear();
            txtAmbienteNombre.Focus();
        }

        private void BtnAgregarSistema_Click(object sender, RoutedEventArgs e)
        {
            _ambienteSistemas.Add(new AmbienteSistemaEditable());
            gridAmbienteSistemas.SelectedIndex = _ambienteSistemas.Count - 1;
            gridAmbienteSistemas.ScrollIntoView(gridAmbienteSistemas.SelectedItem);
        }

        private void BtnEliminarFilaSistema_Click(object sender, RoutedEventArgs e)
        {
            var seleccionado = gridAmbienteSistemas.SelectedItem as AmbienteSistemaEditable;
            if (seleccionado != null)
                _ambienteSistemas.Remove(seleccionado);
        }

        private void BtnGuardarAmbiente_Click(object sender, RoutedEventArgs e)
        {
            MostrarErrorAmbientes(string.Empty);

            if (panelFormAmbiente.Visibility != Visibility.Visible)
            {
                MostrarErrorAmbientes("Selecciona o crea un ambiente primero.");
                return;
            }

            var nombre = txtAmbienteNombre.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                MostrarErrorAmbientes("El nombre del ambiente es obligatorio.");
                return;
            }

            var sistemasValidos = _ambienteSistemas
                .Where(s => !string.IsNullOrWhiteSpace(s.Codigo))
                .ToList();

            if (sistemasValidos.Count == 0)
            {
                MostrarErrorAmbientes("Agrega al menos un sistema con código válido.");
                return;
            }

            Ambiente nuevoAmbiente;
            try
            {
                var sistemas = sistemasValidos.Select(s => new AmbienteSistema(
                    new Sistema(s.Codigo, string.IsNullOrWhiteSpace(s.Nombre) ? s.Codigo : s.Nombre),
                    s.RutaDestino ?? string.Empty));
                nuevoAmbiente = new Ambiente(nombre, sistemas);
            }
            catch (ArgumentException ex)
            {
                MostrarErrorAmbientes(ex.Message);
                return;
            }

            var listaActualizada = _ambientesActuales
                .Where(a => !string.Equals(a.Nombre, nuevoAmbiente.Nombre, StringComparison.OrdinalIgnoreCase))
                .Append(nuevoAmbiente)
                .ToList();

            var resultado = _bootstrapper.Ambientes.Guardar(listaActualizada);
            if (resultado.IsFailure)
            {
                MostrarErrorAmbientes(resultado.Error);
                return;
            }

            CargarAmbientes();
            // Reseleccionar el ambiente recién guardado
            var fila = (gridAmbientes.ItemsSource as IEnumerable<AmbienteRow>)?
                .FirstOrDefault(f => string.Equals(f.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
            if (fila != null) gridAmbientes.SelectedItem = fila;

            _snackbarQueue?.Enqueue($"Ambiente '{nombre}' guardado correctamente.");
        }

        private void BtnEliminarAmbiente_Click(object sender, RoutedEventArgs e)
        {
            MostrarErrorAmbientes(string.Empty);

            var fila = gridAmbientes.SelectedItem as AmbienteRow;
            if (fila == null)
            {
                MostrarErrorAmbientes("Selecciona un ambiente de la lista para eliminar.");
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Eliminar el ambiente '{fila.Entidad.Nombre}'? Esta acción no se puede deshacer.",
                "Eliminar ambiente",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            var listaActualizada = _ambientesActuales
                .Where(a => !string.Equals(a.Nombre, fila.Entidad.Nombre, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var resultado = _bootstrapper.Ambientes.Guardar(listaActualizada);
            if (resultado.IsFailure)
            {
                MostrarErrorAmbientes(resultado.Error);
                return;
            }

            CargarAmbientes();
        }

        private void MostrarErrorAmbientes(string mensaje)
        {
            lblErrorAmbientes.Text = mensaje;
            lblErrorAmbientes.Visibility = string.IsNullOrEmpty(mensaje) ? Visibility.Collapsed : Visibility.Visible;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TAB: BITBUCKET
        // ═══════════════════════════════════════════════════════════════════

        private void CargarSistemas()
        {
            _sistemasActuales = _bootstrapper.Sistemas.ObtenerTodos().ToList();
            var filas = _sistemasActuales.Select(c => new SistemaRow
            {
                Codigo = c.Sistema.Codigo,
                RepositorioCorto = AcortarUrl(c.RepositorioUrl),
                Entidad = c,
            }).ToList();

            gridSistemas.ItemsSource = filas;

            if (filas.Any())
                gridSistemas.SelectedIndex = 0;
            else
                MostrarFormularioSistema(null);
        }

        private static string AcortarUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            // Muestra solo la parte final de la URL para caber en la columna
            var partes = url.TrimEnd('/').Split('/');
            return partes.Length >= 2 ? $"…/{partes[partes.Length - 1]}" : url;
        }

        private void GridSistemas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var fila = gridSistemas.SelectedItem as SistemaRow;
            MostrarFormularioSistema(fila?.Entidad);
        }

        private void MostrarFormularioSistema(ConfiguracionSistema configuracion)
        {
            if (configuracion == null)
            {
                panelFormSistema.Visibility = Visibility.Collapsed;
                lblPlaceholderSistema.Visibility = Visibility.Visible;
                txtSistemaCodigo.Text = string.Empty;
                txtSistemaNombre.Text = string.Empty;
                txtSistemaRepoUrl.Text = string.Empty;
                txtSistemaCarpetaPrecompilada.Text = string.Empty;
                _sistemasPasos.Clear();
                return;
            }

            lblPlaceholderSistema.Visibility = Visibility.Collapsed;
            panelFormSistema.Visibility = Visibility.Visible;

            txtSistemaCodigo.Text = configuracion.Sistema.Codigo;
            txtSistemaNombre.Text = configuracion.Sistema.Nombre;
            txtSistemaRepoUrl.Text = configuracion.RepositorioUrl;
            txtSistemaCarpetaPrecompilada.Text = configuracion.CarpetaPrecompilada;
            _sistemasPasos.Clear();
            foreach (var p in configuracion.SecuenciaDeBuild.Pasos)
                _sistemasPasos.Add(new PasoDeBuildEditable
                {
                    Orden = p.Orden,
                    CarpetaProyecto = p.CarpetaProyecto,
                    ParametrosMsBuild = p.ParametrosMsBuild,
                });
        }

        private void BtnNuevoSistema_Click(object sender, RoutedEventArgs e)
        {
            MostrarErrorSistemas(string.Empty);
            gridSistemas.SelectedItem = null;
            lblPlaceholderSistema.Visibility = Visibility.Collapsed;
            panelFormSistema.Visibility = Visibility.Visible;
            txtSistemaCodigo.Text = string.Empty;
            txtSistemaNombre.Text = string.Empty;
            txtSistemaRepoUrl.Text = string.Empty;
            txtSistemaCarpetaPrecompilada.Text = string.Empty;
            _sistemasPasos.Clear();
            txtSistemaCodigo.Focus();
        }

        private void BtnAgregarPaso_Click(object sender, RoutedEventArgs e)
        {
            var siguiente = _sistemasPasos.Count + 1;
            _sistemasPasos.Add(new PasoDeBuildEditable { Orden = siguiente });
            gridSistemasPasos.SelectedIndex = _sistemasPasos.Count - 1;
            gridSistemasPasos.ScrollIntoView(gridSistemasPasos.SelectedItem);
        }

        private void BtnEliminarFilaPaso_Click(object sender, RoutedEventArgs e)
        {
            var seleccionado = gridSistemasPasos.SelectedItem as PasoDeBuildEditable;
            if (seleccionado != null)
                _sistemasPasos.Remove(seleccionado);
        }

        private void BtnGuardarSistema_Click(object sender, RoutedEventArgs e)
        {
            MostrarErrorSistemas(string.Empty);

            if (panelFormSistema.Visibility != Visibility.Visible)
            {
                MostrarErrorSistemas("Selecciona o crea un sistema primero.");
                return;
            }

            var codigo = txtSistemaCodigo.Text.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                MostrarErrorSistemas("El código de sistema es obligatorio.");
                return;
            }

            var repoUrl = txtSistemaRepoUrl.Text.Trim();
            if (string.IsNullOrWhiteSpace(repoUrl))
            {
                MostrarErrorSistemas("La URL del repositorio es obligatoria.");
                return;
            }

            var carpeta = txtSistemaCarpetaPrecompilada.Text.Trim();
            if (string.IsNullOrWhiteSpace(carpeta))
            {
                MostrarErrorSistemas("La carpeta precompilada es obligatoria.");
                return;
            }

            var pasosValidos = _sistemasPasos
                .Where(p => !string.IsNullOrWhiteSpace(p.CarpetaProyecto))
                .ToList();
            if (pasosValidos.Count == 0)
            {
                MostrarErrorSistemas("Agrega al menos un paso de build con carpeta de proyecto.");
                return;
            }

            ConfiguracionSistema nuevaConfiguracion;
            try
            {
                var nombre = string.IsNullOrWhiteSpace(txtSistemaNombre.Text) ? codigo : txtSistemaNombre.Text.Trim();
                var sistema = new Sistema(codigo, nombre);
                var pasos = pasosValidos.Select(p => new PasoDeBuild(p.Orden, p.CarpetaProyecto, p.ParametrosMsBuild));
                var secuencia = new SecuenciaDeBuild(sistema, pasos);
                nuevaConfiguracion = new ConfiguracionSistema(sistema, repoUrl, carpeta, secuencia);
            }
            catch (ArgumentException ex)
            {
                MostrarErrorSistemas(ex.Message);
                return;
            }

            var listaActualizada = _sistemasActuales
                .Where(c => !string.Equals(c.Sistema.Codigo, nuevaConfiguracion.Sistema.Codigo, StringComparison.OrdinalIgnoreCase))
                .Append(nuevaConfiguracion)
                .ToList();

            var resultado = _bootstrapper.Sistemas.Guardar(listaActualizada);
            if (resultado.IsFailure)
            {
                MostrarErrorSistemas(resultado.Error);
                return;
            }

            CargarSistemas();
            var fila = (gridSistemas.ItemsSource as IEnumerable<SistemaRow>)?
                .FirstOrDefault(f => string.Equals(f.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
            if (fila != null) gridSistemas.SelectedItem = fila;

            _snackbarQueue?.Enqueue($"Sistema '{codigo}' guardado correctamente.");
        }

        private void BtnEliminarSistema_Click(object sender, RoutedEventArgs e)
        {
            MostrarErrorSistemas(string.Empty);

            var fila = gridSistemas.SelectedItem as SistemaRow;
            if (fila == null)
            {
                MostrarErrorSistemas("Selecciona un sistema de la lista para eliminar.");
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Eliminar la configuración de '{fila.Entidad.Sistema.Codigo}'?\nLos ambientes que lo referencian no podrán desplegarlo.",
                "Eliminar sistema",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            var listaActualizada = _sistemasActuales
                .Where(c => !string.Equals(c.Sistema.Codigo, fila.Entidad.Sistema.Codigo, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var resultado = _bootstrapper.Sistemas.Guardar(listaActualizada);
            if (resultado.IsFailure)
            {
                MostrarErrorSistemas(resultado.Error);
                return;
            }

            CargarSistemas();
        }

        private void MostrarErrorSistemas(string mensaje)
        {
            lblErrorSistemas.Text = mensaje;
            lblErrorSistemas.Visibility = string.IsNullOrEmpty(mensaje) ? Visibility.Collapsed : Visibility.Visible;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TAB: USUARIOS
        // ═══════════════════════════════════════════════════════════════════

        private void CargarUsuarios()
        {
            _usuariosActuales = _bootstrapper.Usuarios.ObtenerTodos().ToList();
            var filas = _usuariosActuales.Select(u => new UsuarioRow
            {
                NombreUsuario = u.NombreUsuario,
                NombreVisible = u.NombreVisible,
                Rol = u.Rol.ToString(),
                Activo = u.Activo ? "Sí" : "No",
                Bitbucket = string.IsNullOrEmpty(u.UsuarioBitbucket) ? "—" : u.UsuarioBitbucket,
                Entidad = u,
            }).ToList();

            gridUsuarios.ItemsSource = filas;
            ActualizarPanelDetalleUsuario(null);
        }

        private void GridUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var fila = gridUsuarios.SelectedItem as UsuarioRow;
            ActualizarPanelDetalleUsuario(fila?.Entidad);
        }

        private void ActualizarPanelDetalleUsuario(AppUser usuario)
        {
            if (usuario == null)
            {
                panelDetalleUsuario.Visibility = Visibility.Collapsed;
                lblPlaceholderUsuario.Visibility = Visibility.Visible;
                return;
            }

            lblPlaceholderUsuario.Visibility = Visibility.Collapsed;
            panelDetalleUsuario.Visibility = Visibility.Visible;

            lblUsuarioSeleccionado.Text = $"{usuario.NombreVisible} ({usuario.NombreUsuario})";
            comboRolUsuario.SelectedItem = usuario.Rol;
            btnActivarDesactivar.Content = usuario.Activo ? "DESACTIVAR" : "ACTIVAR";
            lblBitbucketEstado.Text = string.IsNullOrEmpty(usuario.UsuarioBitbucket)
                ? "Bitbucket: sin configurar."
                : $"Bitbucket: {usuario.UsuarioBitbucket}";
        }

        private void BtnNuevoUsuario_Click(object sender, RoutedEventArgs e)
        {
            MostrarErrorUsuarios(string.Empty);
            var dialogo = new NuevoUsuarioDialog(_bootstrapper);
            dialogo.Owner = this;
            dialogo.ShowDialog();
            CargarUsuarios();
        }

        private void BtnGuardarRol_Click(object sender, RoutedEventArgs e)
        {
            MostrarErrorUsuarios(string.Empty);

            var fila = gridUsuarios.SelectedItem as UsuarioRow;
            if (fila == null)
            {
                MostrarErrorUsuarios("Selecciona un usuario de la lista.");
                return;
            }

            if (!(comboRolUsuario.SelectedItem is RolUsuario nuevoRol))
            {
                MostrarErrorUsuarios("Selecciona un rol.");
                return;
            }

            var resultado = _bootstrapper.Usuarios.CambiarRol(fila.Entidad.NombreUsuario, nuevoRol);
            if (resultado.IsFailure)
            {
                MostrarErrorUsuarios(resultado.Error);
                return;
            }

            CargarUsuarios();
            _snackbarQueue?.Enqueue($"'{fila.Entidad.NombreUsuario}' ahora es {nuevoRol}.");
        }

        private void BtnActivarDesactivar_Click(object sender, RoutedEventArgs e)
        {
            MostrarErrorUsuarios(string.Empty);

            var fila = gridUsuarios.SelectedItem as UsuarioRow;
            if (fila == null)
            {
                MostrarErrorUsuarios("Selecciona un usuario de la lista.");
                return;
            }

            var resultado = _bootstrapper.Usuarios.CambiarEstado(fila.Entidad.NombreUsuario, !fila.Entidad.Activo);
            if (resultado.IsFailure)
            {
                MostrarErrorUsuarios(resultado.Error);
                return;
            }

            CargarUsuarios();
        }

        private void BtnResetearContrasena_Click(object sender, RoutedEventArgs e)
        {
            MostrarErrorUsuarios(string.Empty);

            var fila = gridUsuarios.SelectedItem as UsuarioRow;
            if (fila == null)
            {
                MostrarErrorUsuarios("Selecciona un usuario de la lista.");
                return;
            }

            var dialogo = new ResetearContrasenaDialog(_bootstrapper, fila.Entidad.NombreUsuario);
            dialogo.Owner = this;
            dialogo.ShowDialog();
        }

        private void BtnConfigurarBitbucket_Click(object sender, RoutedEventArgs e)
        {
            MostrarErrorUsuarios(string.Empty);

            var fila = gridUsuarios.SelectedItem as UsuarioRow;
            if (fila == null)
            {
                MostrarErrorUsuarios("Selecciona un usuario de la lista.");
                return;
            }

            var dialogo = new CredencialesBitbucketDialog(_bootstrapper, fila.Entidad.NombreUsuario);
            dialogo.Owner = this;
            dialogo.ShowDialog();
            CargarUsuarios();
        }

        private void MostrarErrorUsuarios(string mensaje)
        {
            lblErrorUsuarios.Text = mensaje;
            lblErrorUsuarios.Visibility = string.IsNullOrEmpty(mensaje) ? Visibility.Collapsed : Visibility.Visible;
        }

        // ═══════════════════════════════════════════════════════════════════
        // TAB: GENERAL
        // ═══════════════════════════════════════════════════════════════════

        private void CargarRutasGenerales()
        {
            txtRutaClonado.Text = _bootstrapper.RutaClonado;
            txtRutaTemporales.Text = _bootstrapper.RutaTemporales;
            txtRutaLogs.Text = _bootstrapper.RutaLogs;
            txtRutaConfiguracion.Text = _bootstrapper.RutaConfiguracion;
            txtCadenaConexion.Text = _bootstrapper.CadenaConexionSqlServer;
        }

        private void BtnGuardarGeneral_Click(object sender, RoutedEventArgs e)
        {
            MostrarErrorGeneral(string.Empty);

            var clonado = txtRutaClonado.Text.Trim();
            var temporales = txtRutaTemporales.Text.Trim();
            var logs = txtRutaLogs.Text.Trim();
            var config = txtRutaConfiguracion.Text.Trim();
            var cadena = txtCadenaConexion.Text.Trim();

            if (string.IsNullOrWhiteSpace(clonado) || string.IsNullOrWhiteSpace(temporales) ||
                string.IsNullOrWhiteSpace(logs) || string.IsNullOrWhiteSpace(config) ||
                string.IsNullOrWhiteSpace(cadena))
            {
                MostrarErrorGeneral("Todas las rutas son obligatorias.");
                return;
            }

            try
            {
                _bootstrapper.GuardarRutasGenerales(clonado, temporales, logs, config, cadena);
            }
            catch (Exception ex)
            {
                MostrarErrorGeneral($"No se pudieron guardar las rutas: {ex.Message}");
                return;
            }

            _snackbarQueue?.Enqueue("Configuración general guardada. Se aplicará al reiniciar.");
        }

        private void MostrarErrorGeneral(string mensaje)
        {
            lblErrorGeneral.Text = mensaje;
            lblErrorGeneral.Visibility = string.IsNullOrEmpty(mensaje) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
