using System.Collections.Generic;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Patrones de archivos que nunca se sobrescriben si ya existen en destino
    /// (ExclusionRules.json) — automatiza lo que hoy se hace excluyendo
    /// "web.config" a mano antes de desplegar.
    /// </summary>
    public interface IExclusionRulesRepository
    {
        IReadOnlyList<string> ObtenerPatrones();
    }
}
