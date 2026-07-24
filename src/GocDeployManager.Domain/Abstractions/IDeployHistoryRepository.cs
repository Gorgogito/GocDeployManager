using System.Collections.Generic;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Persistencia del historial de despliegues (tabla DeployHistory en SQLite).
    /// </summary>
    public interface IDeployHistoryRepository
    {
        void Registrar(Despliegue despliegue);

        IReadOnlyList<Despliegue> Buscar(FiltroHistorial filtro);
    }
}
