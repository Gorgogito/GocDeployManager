using System.Collections.Generic;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Persistencia de GruposDestinatarios.json. Reemplaza la lista completa
    /// al guardar, igual que <see cref="IAmbienteRepository"/>.
    /// </summary>
    public interface IGrupoDestinatariosRepository
    {
        IReadOnlyList<GrupoDestinatarios> ObtenerTodos();

        void Guardar(IReadOnlyList<GrupoDestinatarios> grupos);
    }
}
