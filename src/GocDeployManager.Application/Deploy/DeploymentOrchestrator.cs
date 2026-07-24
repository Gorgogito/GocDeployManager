using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public Result EjecutarDespliegue(SolicitudDespliegue solicitud, IProgress<string> progreso = null)
        {
            Guard.ContraNulo(solicitud, nameof(solicitud));

            var cronometroCompilacion = new Stopwatch();
            var cronometroDespliegue = new Stopwatch();

            _logger.Info($"Despliegue iniciado: GOC={solicitud.Goc.Numero} rama={solicitud.Goc.RamaBitbucket} ambiente={solicitud.Ambiente.Nombre} sistemas={string.Join(",", solicitud.Sistemas.Select(s => s.Codigo))} usuario={solicitud.UsuarioAplicacion}");

            foreach (var sistema in solicitud.Sistemas)
            {
                progreso?.Report($"[{sistema.Nombre}] Resolviendo configuración...");

                var configuracionResultado = _sistemas.ObtenerConfiguracion(sistema);
                if (configuracionResultado.IsFailure)
                    return RegistrarFalloYDevolver(solicitud, cronometroCompilacion.Elapsed, cronometroDespliegue.Elapsed,
                        $"[{sistema.Nombre}] {configuracionResultado.Error}");

                var ambienteSistema = solicitud.Ambiente.BuscarSistema(sistema);
                if (ambienteSistema == null)
                    return RegistrarFalloYDevolver(solicitud, cronometroCompilacion.Elapsed, cronometroDespliegue.Elapsed,
                        $"El ambiente '{solicitud.Ambiente.Nombre}' no tiene configurada una ruta para {sistema.Nombre}.");

                var configuracion = configuracionResultado.Value;
                var rutaTrabajo = Path.Combine(solicitud.RutaClonadoBase, sistema.Codigo, solicitud.Goc.Numero);

                progreso?.Report($"[{sistema.Nombre}] Clonando {solicitud.Goc.RamaBitbucket}...");
                var clonado = _git.ClonarORama(
                    configuracion.RepositorioUrl, solicitud.Goc.RamaBitbucket, rutaTrabajo,
                    solicitud.UsuarioBitbucket, solicitud.ContrasenaBitbucket);

                if (clonado.IsFailure)
                    return RegistrarFalloYDevolver(solicitud, cronometroCompilacion.Elapsed, cronometroDespliegue.Elapsed,
                        $"[{sistema.Nombre}] Clonado: {clonado.Error}");

                cronometroCompilacion.Start();
                foreach (var paso in configuracion.SecuenciaDeBuild.Pasos)
                {
                    progreso?.Report($"[{sistema.Nombre}] Compilando {paso.CarpetaProyecto}...");
                    var build = _msbuild.EjecutarPaso(paso, rutaTrabajo);

                    if (build.IsFailure)
                    {
                        cronometroCompilacion.Stop();
                        return RegistrarFalloYDevolver(solicitud, cronometroCompilacion.Elapsed, cronometroDespliegue.Elapsed,
                            $"[{sistema.Nombre}] Build ({paso.CarpetaProyecto}): {build.Error}");
                    }
                }
                cronometroCompilacion.Stop();

                progreso?.Report($"[{sistema.Nombre}] Copiando archivos...");
                var rutaPrecompilada = Path.Combine(rutaTrabajo, configuracion.CarpetaPrecompilada);
                var patronesExclusion = _exclusionRules.ObtenerPatrones();

                cronometroDespliegue.Start();
                var copia = _deployer.Copiar(rutaPrecompilada, ambienteSistema.RutaDestino, patronesExclusion);
                cronometroDespliegue.Stop();

                if (copia.IsFailure)
                    return RegistrarFalloYDevolver(solicitud, cronometroCompilacion.Elapsed, cronometroDespliegue.Elapsed,
                        $"[{sistema.Nombre}] Copia: {copia.Error}");
            }

            progreso?.Report("Despliegue completado.");

            var despliegueExitoso = Entidades.Despliegue.RegistrarExitoso(
                solicitud.UsuarioAplicacion, solicitud.UsuarioWindows, solicitud.Equipo,
                solicitud.Goc.Numero, solicitud.Goc.RamaBitbucket, solicitud.Ambiente.Nombre,
                solicitud.Sistemas.Select(s => s.Codigo), cronometroCompilacion.Elapsed, cronometroDespliegue.Elapsed);

            _historial.Registrar(despliegueExitoso);
            _logger.Info($"Despliegue exitoso: GOC={solicitud.Goc.Numero} (compilación {cronometroCompilacion.Elapsed:hh\\:mm\\:ss}, despliegue {cronometroDespliegue.Elapsed:hh\\:mm\\:ss}).");
            return Result.Ok();
        }

        private Result RegistrarFalloYDevolver(
            SolicitudDespliegue solicitud, TimeSpan tiempoCompilacion, TimeSpan tiempoDespliegue, string error)
        {
            var despliegueFallido = Entidades.Despliegue.RegistrarFallido(
                solicitud.UsuarioAplicacion, solicitud.UsuarioWindows, solicitud.Equipo,
                solicitud.Goc.Numero, solicitud.Goc.RamaBitbucket, solicitud.Ambiente.Nombre,
                solicitud.Sistemas.Select(s => s.Codigo), tiempoCompilacion, tiempoDespliegue, error);

            _historial.Registrar(despliegueFallido);
            _logger.Error($"Despliegue fallido: GOC={solicitud.Goc.Numero}. Motivo: {error}");
            return Result.Fail(error);
        }
    }
}
