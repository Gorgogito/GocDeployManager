using System.Collections.Generic;
using System.Linq;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// La secuencia ordenada de pasos de build de un sistema. Confirmado que difiere
    /// por sistema (SIT: 3 capas + solución, IDI: 6 capas + solución) — nunca es una
    /// constante compartida entre sistemas.
    /// </summary>
    public sealed class SecuenciaDeBuild
    {
        public Sistema Sistema { get; }
        public IReadOnlyList<PasoDeBuild> Pasos { get; }

        public SecuenciaDeBuild(Sistema sistema, IEnumerable<PasoDeBuild> pasos)
        {
            Sistema = Guard.ContraNulo(sistema, nameof(sistema));

            var lista = Guard.ContraNulo(pasos, nameof(pasos))
                .OrderBy(p => p.Orden)
                .ToList();

            if (lista.Count == 0)
                throw new System.ArgumentException("Una secuencia de build debe tener al menos un paso.", nameof(pasos));

            Pasos = lista.AsReadOnly();
        }
    }
}
