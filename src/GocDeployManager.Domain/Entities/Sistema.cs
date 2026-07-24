using System;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Identificador de negocio de un sistema (SIT, IDI, ProPag, ...).
    /// Deliberadamente independiente del nombre físico de sus proyectos/soluciones:
    /// ProPag reutiliza los mismos nombres de proyecto que SIT (Sit.BusinessEntities,
    /// SITSolution), así que el código de sistema no puede derivarse de ahí.
    /// </summary>
    public sealed class Sistema : IEquatable<Sistema>
    {
        public string Codigo { get; }
        public string Nombre { get; }

        public Sistema(string codigo, string nombre)
        {
            Codigo = Guard.ContraNuloOVacio(codigo, nameof(codigo)).Trim().ToUpperInvariant();
            Nombre = Guard.ContraNuloOVacio(nombre, nameof(nombre));
        }

        public bool Equals(Sistema other) =>
            other != null && string.Equals(Codigo, other.Codigo, StringComparison.Ordinal);

        public override bool Equals(object obj) => Equals(obj as Sistema);

        public override int GetHashCode() => Codigo.GetHashCode();

        public override string ToString() => Nombre;
    }
}
