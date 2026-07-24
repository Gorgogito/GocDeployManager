using System;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using NLog;

namespace GocDeployManager.Services
{
    /// <summary>
    /// Implementación real de <see cref="IAppLogger"/> sobre NLog. Los targets,
    /// el nivel y la rotación viven en NLog.config (autoReload="true": se
    /// puede ajustar en caliente sin reiniciar la app, sección 14 del
    /// análisis) — este wrapper solo inyecta en runtime la ruta real del log,
    /// para no bifurcar la fuente de verdad de rutas que ya resuelve
    /// Bootstrapper (igual que Clonado/Temporales/Configuración/BD).
    /// </summary>
    public sealed class NLogAppLogger : IAppLogger
    {
        private readonly Logger _logger;

        public NLogAppLogger(string rutaLogs)
        {
            Guard.ContraNuloOVacio(rutaLogs, nameof(rutaLogs));

            var configuracion = LogManager.Configuration;
            if (configuracion == null)
                throw new InvalidOperationException(
                    "No se encontró NLog.config junto al ejecutable — no se puede inicializar el logging.");

            configuracion.Variables["rutaLogs"] = rutaLogs;
            _logger = LogManager.GetLogger("GocDeployManager");
        }

        public void Info(string mensaje) => _logger.Info(mensaje);

        public void Warn(string mensaje) => _logger.Warn(mensaje);

        public void Error(string mensaje, Exception excepcion = null) => _logger.Error(excepcion, mensaje);
    }
}
