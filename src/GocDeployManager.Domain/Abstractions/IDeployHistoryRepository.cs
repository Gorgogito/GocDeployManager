using System.Collections.Generic;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Persistencia del historial de despliegues (tabla DeployHistory en SQL Server).
    /// </summary>
    public interface IDeployHistoryRepository
    {
        /// <summary>Registra el despliegue y devuelve el Id generado por la base
        /// de datos — lo usa el módulo de notificaciones para vincular sus
        /// intentos de envío (tabla NotificationOutbox) a este despliegue.</summary>
        int Registrar(Despliegue despliegue);

        IReadOnlyList<Despliegue> Buscar(FiltroHistorial filtro);
    }
}
