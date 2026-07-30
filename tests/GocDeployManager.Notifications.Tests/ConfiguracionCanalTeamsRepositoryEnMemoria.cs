using System.Collections.Generic;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Notifications.Tests
{
    internal sealed class ConfiguracionCanalTeamsRepositoryEnMemoria : IConfiguracionCanalTeamsRepository
    {
        private IReadOnlyList<MapeoCanalTeams> _mapeos = new List<MapeoCanalTeams>();

        public ConfiguracionCanalTeamsRepositoryEnMemoria(IReadOnlyList<MapeoCanalTeams> mapeosIniciales = null)
        {
            if (mapeosIniciales != null)
                _mapeos = mapeosIniciales;
        }

        public IReadOnlyList<MapeoCanalTeams> ObtenerTodos() => _mapeos;

        public void Guardar(IReadOnlyList<MapeoCanalTeams> mapeos) => _mapeos = mapeos;
    }
}
