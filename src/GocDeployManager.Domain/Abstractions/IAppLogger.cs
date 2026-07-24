using System;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Registro de eventos técnicos y de auditoría (sección 14/16 del
    /// análisis) — Application/Infrastructure dependen solo de esta
    /// abstracción; la implementación real con NLog vive en Services.
    /// Nunca debe recibir contraseñas ni secretos como parte del mensaje.
    /// </summary>
    public interface IAppLogger
    {
        void Info(string mensaje);

        void Warn(string mensaje);

        void Error(string mensaje, Exception excepcion = null);
    }
}
