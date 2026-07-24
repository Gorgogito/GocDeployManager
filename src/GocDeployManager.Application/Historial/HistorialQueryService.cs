using System.Collections.Generic;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Application.Historial
{
    /// <summary>
    /// Consulta del historial de despliegues (pantalla "Historial", sección 10)
    /// — accesible también para el rol Consulta, de solo lectura.
    /// </summary>
    public sealed class HistorialQueryService
    {
        private readonly IDeployHistoryRepository _historial;

        public HistorialQueryService(IDeployHistoryRepository historial)
        {
            _historial = Guard.ContraNulo(historial, nameof(historial));
        }

        public IReadOnlyList<Despliegue> Buscar(FiltroHistorial filtro) =>
            _historial.Buscar(filtro ?? new FiltroHistorial());
    }
}
