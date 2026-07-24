using System;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Criterios de búsqueda de la pantalla de Historial (sección 10 del análisis).
    /// Todos los campos son opcionales: null significa "sin filtrar por este campo".
    /// </summary>
    public sealed class FiltroHistorial
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string Goc { get; set; }
        public string Ambiente { get; set; }
        public string Sistema { get; set; }
        public ResultadoDespliegue? Resultado { get; set; }
    }
}
