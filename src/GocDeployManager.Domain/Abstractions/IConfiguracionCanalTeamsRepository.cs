using System.Collections.Generic;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>Persistencia de CanalTeams.json.</summary>
    public interface IConfiguracionCanalTeamsRepository
    {
        IReadOnlyList<MapeoCanalTeams> ObtenerTodos();

        void Guardar(IReadOnlyList<MapeoCanalTeams> mapeos);
    }
}
