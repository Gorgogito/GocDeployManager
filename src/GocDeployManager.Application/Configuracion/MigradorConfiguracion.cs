using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;

namespace GocDeployManager.Application.Configuracion
{
    /// <summary>
    /// Migración única de Ambientes/Sistemas desde su origen anterior (los
    /// JSON por laptop) hacia el destino compartido (SQL Server) — pensada
    /// para correrse una sola vez, no como parte del flujo normal de la app.
    /// Se niega a pisar el destino si ya tiene datos, para que no sea
    /// destructivo si alguien la corre por error una segunda vez.
    /// </summary>
    public sealed class MigradorConfiguracion
    {
        private readonly IAmbienteRepository _origenAmbientes;
        private readonly IAmbienteRepository _destinoAmbientes;
        private readonly ISistemaRepository _origenSistemas;
        private readonly ISistemaRepository _destinoSistemas;

        public MigradorConfiguracion(
            IAmbienteRepository origenAmbientes,
            IAmbienteRepository destinoAmbientes,
            ISistemaRepository origenSistemas,
            ISistemaRepository destinoSistemas)
        {
            _origenAmbientes = Guard.ContraNulo(origenAmbientes, nameof(origenAmbientes));
            _destinoAmbientes = Guard.ContraNulo(destinoAmbientes, nameof(destinoAmbientes));
            _origenSistemas = Guard.ContraNulo(origenSistemas, nameof(origenSistemas));
            _destinoSistemas = Guard.ContraNulo(destinoSistemas, nameof(destinoSistemas));
        }

        public Result<string> Migrar()
        {
            var ambientesDestino = _destinoAmbientes.ObtenerTodos();
            var sistemasDestino = _destinoSistemas.ObtenerTodasLasConfiguraciones();

            if (ambientesDestino.Count > 0 || sistemasDestino.Count > 0)
                return Result.Fail<string>(
                    $"El destino ya tiene {ambientesDestino.Count} ambiente(s) y {sistemasDestino.Count} sistema(s) — " +
                    "no se migra para no sobrescribir datos existentes. Si de verdad querés reemplazarlos, " +
                    "borralos primero desde Configuración y volvé a correr la migración.");

            var ambientesOrigen = _origenAmbientes.ObtenerTodos();
            var sistemasOrigen = _origenSistemas.ObtenerTodasLasConfiguraciones();

            if (ambientesOrigen.Count == 0 && sistemasOrigen.Count == 0)
                return Result.Fail<string>("No se encontraron ambientes ni sistemas en el origen (JSON) para migrar.");

            if (sistemasOrigen.Count > 0)
                _destinoSistemas.Guardar(sistemasOrigen);

            if (ambientesOrigen.Count > 0)
                _destinoAmbientes.Guardar(ambientesOrigen);

            return Result.Ok($"Se migraron {ambientesOrigen.Count} ambiente(s) y {sistemasOrigen.Count} sistema(s) hacia SQL Server.");
        }
    }
}
