using System.Collections.Generic;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Notifications.Tests
{
    internal sealed class GrupoDestinatariosRepositoryEnMemoria : IGrupoDestinatariosRepository
    {
        private IReadOnlyList<GrupoDestinatarios> _grupos = new List<GrupoDestinatarios>();

        public GrupoDestinatariosRepositoryEnMemoria(IReadOnlyList<GrupoDestinatarios> gruposIniciales = null)
        {
            if (gruposIniciales != null)
                _grupos = gruposIniciales;
        }

        public IReadOnlyList<GrupoDestinatarios> ObtenerTodos() => _grupos;

        public void Guardar(IReadOnlyList<GrupoDestinatarios> grupos) => _grupos = grupos;
    }
}
