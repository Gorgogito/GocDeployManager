using System.Collections.Generic;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Persistencia de ambientes (tablas Ambiente/AmbienteSistema en SQL
    /// Server, compartidas por todos los usuarios). Reemplaza la lista
    /// completa al guardar.
    /// </summary>
    public interface IAmbienteRepository
    {
        IReadOnlyList<Ambiente> ObtenerTodos();

        void Guardar(IReadOnlyList<Ambiente> ambientes);
    }
}
