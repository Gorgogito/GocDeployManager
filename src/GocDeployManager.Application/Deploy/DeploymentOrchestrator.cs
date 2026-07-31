using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using Entidades = GocDeployManager.Domain.Entities;

namespace GocDeployManager.Application.Deploy
{
    /// <summary>
    /// El caso de uso central: clonar, compilar y copiar cada sistema del GOC,
    /// registrando el resultado en el historial — exitoso o fallido, siempre
    /// (sección 11 del análisis). No admite cancelación una vez iniciado
    /// (decisión del cliente).
    /// </summary>
    public sealed class DeploymentOrchestrator
    {
        private static readonly Regex PatronLineaDeError = new Regex(@"\berror\b", RegexOptions.IgnoreCase);

        private readonly IGitClient _git;
        private readonly IMsBuildRunner _msbuild;
        private readonly IFileDeployer _deployer;
        private readonly ISistemaRepository _sistemas;
        private readonly IExclusionRulesRepository _exclusionRules;
        private readonly IDeployHistoryRepository _historial;
        private readonly IAppLogger _logger;

        public DeploymentOrchestrator(
            IGitClient git,
            IMsBuildRunner msbuild,
            IFileDeployer deployer,
            ISistemaRepository sistemas,
            IExclusionRulesRepository exclusionRules,
            IDeployHistoryRepository historial,
            IAppLogger logger)
        {
            _git = Guard.ContraNulo(git, nameof(git));
            _msbuild = Guard.ContraNulo(msbuild, nameof(msbuild));
            _deployer = Guard.ContraNulo(deployer, nameof(deployer));
            _sistemas = Guard.ContraNulo(sistemas, nameof(sistemas));
            _exclusionRules = Guard.ContraNulo(exclusionRules, nameof(exclusionRules));
            _historial = Guard.ContraNulo(historial, nameof(historial));
            _logger = Guard.ContraNulo(logger, nameof(logger));
        }

        public Result EjecutarDespliegue(SolicitudDespliegue solicitud, IProgress<Entidades.MensajeSalidaDespliegue> progreso = null)
        {
            Guard.ContraNulo(solicitud, nameof(solicitud));

            var cronometroTotal = Stopwatch.StartNew();
            var cronometroCompilacion = new Stopwatch();
            var cronometroDespliegue = new Stopwatch();

            _logger.Info($"Despliegue iniciado: GOC={solicitud.Goc.Numero} rama={solicitud.Goc.RamaBitbucket} ambiente={solicitud.Ambiente.Nombre} sistemas={string.Join(",", solicitud.Sistemas.Select(s => s.Codigo))} usuario={solicitud.UsuarioAplicacion}");

            foreach (var sistema in solicitud.Sistemas)
            {
                ReportarInicioEtapa(progreso, Entidades.EtapaProgreso.Validacion, $"VALIDACIÓN — {sistema.Nombre}");
                Reportar(progreso, Entidades.NivelMensajeSalida.Info, $"[{sistema.Nombre}] Resolviendo configuración...", esResumen: true);

                var configuracionResultado = _sistemas.ObtenerConfiguracion(sistema);
                if (configuracionResultado.IsFailure)
                {
                    var error = $"[{sistema.Nombre}] {configuracionResultado.Error}";
                    Reportar(progreso, Entidades.NivelMensajeSalida.Error, error);
                    return RegistrarFalloYDevolver(solicitud, cronometroCompilacion.Elapsed, cronometroDespliegue.Elapsed,
                        progreso, "Validación", error);
                }

                var ambienteSistema = solicitud.Ambiente.BuscarSistema(sistema);
                if (ambienteSistema == null)
                {
                    var error = $"El ambiente '{solicitud.Ambiente.Nombre}' no tiene configurada una ruta para {sistema.Nombre}.";
                    Reportar(progreso, Entidades.NivelMensajeSalida.Error, error);
                    return RegistrarFalloYDevolver(solicitud, cronometroCompilacion.Elapsed, cronometroDespliegue.Elapsed,
                        progreso, "Validación", error);
                }

                var configuracion = configuracionResultado.Value;
                string rutaTrabajo;
                try
                {
                    rutaTrabajo = Path.Combine(solicitud.RutaClonadoBase, sistema.Codigo, solicitud.Goc.Numero);
                }
                catch (ArgumentException ex)
                {
                    var error = $"[{sistema.Nombre}] El código de sistema configurado tiene caracteres inválidos para una ruta (revisá Configuración › Bitbucket): {ex.Message}";
                    Reportar(progreso, Entidades.NivelMensajeSalida.Error, error);
                    return RegistrarFalloYDevolver(solicitud, cronometroCompilacion.Elapsed, cronometroDespliegue.Elapsed,
                        progreso, "Validación", error);
                }

                Reportar(progreso, Entidades.NivelMensajeSalida.Success, $"[{sistema.Nombre}] Configuración válida.");

                ReportarInicioEtapa(progreso, Entidades.EtapaProgreso.Clonado, $"CLONADO — {sistema.Nombre}");
                Reportar(progreso, Entidades.NivelMensajeSalida.Info, $"[{sistema.Nombre}] Rama: {solicitud.Goc.RamaBitbucket}");
                Reportar(progreso, Entidades.NivelMensajeSalida.Info, $"[{sistema.Nombre}] Clonando/actualizando repositorio...", esResumen: true);

                var clonado = _git.ClonarORama(
                    configuracion.RepositorioUrl, solicitud.Goc.RamaBitbucket, rutaTrabajo,
                    solicitud.UsuarioBitbucket, solicitud.ContrasenaBitbucket,
                    linea => ReportarSalidaDeProceso(progreso, linea));

                if (clonado.IsFailure)
                {
                    var error = $"[{sistema.Nombre}] Clonado: {PrimeraLinea(clonado.Error)}";
                    Reportar(progreso, Entidades.NivelMensajeSalida.Error, error);
                    return RegistrarFalloYDevolver(solicitud, cronometroCompilacion.Elapsed, cronometroDespliegue.Elapsed,
                        progreso, "Clonado", $"[{sistema.Nombre}] Clonado: {clonado.Error}");
                }

                Reportar(progreso, Entidades.NivelMensajeSalida.Success, $"[{sistema.Nombre}] Clonado finalizado correctamente.");

                ReportarInicioEtapa(progreso, Entidades.EtapaProgreso.Compilacion, $"COMPILACIÓN — {sistema.Nombre}");
                cronometroCompilacion.Start();
                foreach (var paso in configuracion.SecuenciaDeBuild.Pasos)
                {
                    Reportar(progreso, Entidades.NivelMensajeSalida.Info, $"[{sistema.Nombre}] Compilando {paso.CarpetaProyecto}...", esResumen: true);
                    var build = _msbuild.EjecutarPaso(paso, rutaTrabajo, linea => ReportarSalidaDeProceso(progreso, linea));

                    if (build.IsFailure)
                    {
                        cronometroCompilacion.Stop();
                        var error = $"[{sistema.Nombre}] Build ({paso.CarpetaProyecto}): {PrimeraLinea(build.Error)}";
                        Reportar(progreso, Entidades.NivelMensajeSalida.Error, error);
                        return RegistrarFalloYDevolver(solicitud, cronometroCompilacion.Elapsed, cronometroDespliegue.Elapsed,
                            progreso, "Compilación", $"[{sistema.Nombre}] Build ({paso.CarpetaProyecto}): {build.Error}");
                    }

                    Reportar(progreso, Entidades.NivelMensajeSalida.Success, $"[{sistema.Nombre}] {paso.CarpetaProyecto}: OK");
                }
                cronometroCompilacion.Stop();

                ReportarInicioEtapa(progreso, Entidades.EtapaProgreso.Despliegue, $"DESPLIEGUE — {sistema.Nombre}");
                Reportar(progreso, Entidades.NivelMensajeSalida.Info, $"[{sistema.Nombre}] Preparando archivos...", esResumen: true);
                string rutaPrecompilada;
                try
                {
                    rutaPrecompilada = Path.Combine(rutaTrabajo, configuracion.CarpetaPrecompilada);
                }
                catch (ArgumentException ex)
                {
                    var error = $"[{sistema.Nombre}] La carpeta precompilada configurada tiene caracteres inválidos para una ruta (revisá Configuración › Bitbucket): {ex.Message}";
                    Reportar(progreso, Entidades.NivelMensajeSalida.Error, error);
                    return RegistrarFalloYDevolver(solicitud, cronometroCompilacion.Elapsed, cronometroDespliegue.Elapsed,
                        progreso, "Despliegue", error);
                }
                var patronesExclusion = _exclusionRules.ObtenerPatrones();

                var archivosCopiados = 0;
                var archivosOmitidos = 0;
                Action<string, bool> onArchivo = (nombre, omitido) =>
                {
                    if (omitido)
                        archivosOmitidos++;
                    else
                        archivosCopiados++;

                    Reportar(progreso, Entidades.NivelMensajeSalida.Debug,
                        omitido ? $"[{sistema.Nombre}] Omitiendo {nombre} (archivo protegido)." : $"[{sistema.Nombre}] Copiando {nombre}");
                };

                cronometroDespliegue.Start();
                var copia = _deployer.Copiar(rutaPrecompilada, ambienteSistema.RutaDestino, patronesExclusion, onArchivo);
                cronometroDespliegue.Stop();

                if (copia.IsFailure)
                {
                    var error = $"[{sistema.Nombre}] Copia: {copia.Error}";
                    Reportar(progreso, Entidades.NivelMensajeSalida.Error, error);
                    return RegistrarFalloYDevolver(solicitud, cronometroCompilacion.Elapsed, cronometroDespliegue.Elapsed,
                        progreso, "Despliegue", error);
                }

                Reportar(progreso, Entidades.NivelMensajeSalida.Success,
                    $"[{sistema.Nombre}] Copia finalizada correctamente. {archivosCopiados} archivo(s) copiado(s), {archivosOmitidos} omitido(s).");
            }

            ReportarInicioEtapa(progreso, Entidades.EtapaProgreso.Finalizacion, "FINALIZACIÓN");
            Reportar(progreso, Entidades.NivelMensajeSalida.Success,
                $"Despliegue finalizado correctamente. GOC: {solicitud.Goc.Numero} · Ambiente: {solicitud.Ambiente.Nombre} · " +
                $"Sistemas: {string.Join(", ", solicitud.Sistemas.Select(s => s.Codigo))} · Duración: {cronometroTotal.Elapsed:hh\\:mm\\:ss}.",
                esResumen: true);

            var despliegueExitoso = Entidades.Despliegue.RegistrarExitoso(
                solicitud.UsuarioAplicacion, solicitud.UsuarioWindows, solicitud.Equipo,
                solicitud.Goc.Numero, solicitud.Goc.RamaBitbucket, solicitud.Ambiente.Nombre,
                solicitud.Sistemas.Select(s => s.Codigo), cronometroCompilacion.Elapsed, cronometroDespliegue.Elapsed);

            _historial.Registrar(despliegueExitoso);
            _logger.Info($"Despliegue exitoso: GOC={solicitud.Goc.Numero} (compilación {cronometroCompilacion.Elapsed:hh\\:mm\\:ss}, despliegue {cronometroDespliegue.Elapsed:hh\\:mm\\:ss}).");
            return Result.Ok();
        }

        private Result RegistrarFalloYDevolver(
            SolicitudDespliegue solicitud, TimeSpan tiempoCompilacion, TimeSpan tiempoDespliegue,
            IProgress<Entidades.MensajeSalidaDespliegue> progreso, string etapaTexto, string error)
        {
            ReportarInicioEtapa(progreso, Entidades.EtapaProgreso.Finalizacion, "FINALIZACIÓN");
            Reportar(progreso, Entidades.NivelMensajeSalida.Error,
                $"Despliegue finalizado con errores. GOC: {solicitud.Goc.Numero} · Etapa: {etapaTexto} · Error: {PrimeraLinea(error)}",
                esResumen: true);

            var despliegueFallido = Entidades.Despliegue.RegistrarFallido(
                solicitud.UsuarioAplicacion, solicitud.UsuarioWindows, solicitud.Equipo,
                solicitud.Goc.Numero, solicitud.Goc.RamaBitbucket, solicitud.Ambiente.Nombre,
                solicitud.Sistemas.Select(s => s.Codigo), tiempoCompilacion, tiempoDespliegue, error);

            _historial.Registrar(despliegueFallido);
            _logger.Error($"Despliegue fallido: GOC={solicitud.Goc.Numero}. Motivo: {error}");
            return Result.Fail(error);
        }

        private static void Reportar(IProgress<Entidades.MensajeSalidaDespliegue> progreso, Entidades.NivelMensajeSalida nivel, string texto, bool esResumen = false) =>
            progreso?.Report(Entidades.MensajeSalidaDespliegue.Crear(nivel, texto, esResumen));

        private static void ReportarInicioEtapa(IProgress<Entidades.MensajeSalidaDespliegue> progreso, Entidades.EtapaProgreso etapa, string texto) =>
            progreso?.Report(Entidades.MensajeSalidaDespliegue.InicioDeEtapa(etapa, texto));

        /// <summary>
        /// Streaming en vivo de una línea de stdout/stderr de git.exe o
        /// MSBuild.exe: se clasifica como Error si tiene pinta de serlo
        /// ("fatal:", "error CSxxxx", etc.), si no como Debug — nunca marcado
        /// como resumen, así que solo llega al panel de salida detallado, no
        /// al indicador de "Progreso del despliegue".
        /// </summary>
        private static void ReportarSalidaDeProceso(IProgress<Entidades.MensajeSalidaDespliegue> progreso, string linea)
        {
            // git y MSBuild emiten líneas en blanco (o solo espacios) como
            // separadores visuales de su propia salida — MensajeSalidaDespliegue
            // exige texto no vacío (Guard.ContraNuloOVacio usa IsNullOrWhiteSpace),
            // así que hay que descartarlas acá, no solo las realmente vacías.
            if (progreso == null || string.IsNullOrWhiteSpace(linea))
                return;

            var nivel = ParecelineaDeError(linea) ? Entidades.NivelMensajeSalida.Error : Entidades.NivelMensajeSalida.Debug;
            progreso.Report(Entidades.MensajeSalidaDespliegue.Crear(nivel, linea));
        }

        private static bool ParecelineaDeError(string linea) =>
            linea.IndexOf("fatal:", StringComparison.OrdinalIgnoreCase) >= 0 || PatronLineaDeError.IsMatch(linea);

        private static string PrimeraLinea(string texto) =>
            string.IsNullOrEmpty(texto) ? texto : texto.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.None)[0];
    }
}
