using System.Collections.Generic;
using System.Linq;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Un grupo configurable de destinatarios (ej. "Desarrollo", "QA") — se
    /// selecciona al iniciar un despliegue, además de destinatarios ad-hoc
    /// (análisis de notificaciones, sección 8).
    /// </summary>
    public sealed class GrupoDestinatarios
    {
        public string Nombre { get; }
        public IReadOnlyList<Destinatario> Miembros { get; }

        public GrupoDestinatarios(string nombre, IEnumerable<Destinatario> miembros)
        {
            Nombre = Guard.ContraNuloOVacio(nombre, nameof(nombre));
            Miembros = Guard.ContraNulo(miembros, nameof(miembros)).ToList().AsReadOnly();
        }
    }
}
