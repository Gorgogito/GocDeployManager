using System;
using System.Collections.Generic;
using System.Linq;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    public sealed class DespliegueIniciadoEvento : IEventoDespliegue
    {
        public string Goc { get; }
        public string Ambiente { get; }
        public IReadOnlyList<string> Sistemas { get; }
        public string UsuarioAplicacion { get; }
        public DateTime FechaHora { get; }
        public IReadOnlyList<string> GruposDestinatariosSeleccionados { get; }
        public IReadOnlyList<string> DestinatariosAdicionales { get; }
        public IReadOnlyList<string> CanalesSeleccionados { get; }

        public DespliegueIniciadoEvento(
            string goc,
            string ambiente,
            IEnumerable<string> sistemas,
            string usuarioAplicacion,
            DateTime fechaHora,
            IEnumerable<string> gruposDestinatariosSeleccionados = null,
            IEnumerable<string> destinatariosAdicionales = null,
            IEnumerable<string> canalesSeleccionados = null)
        {
            Goc = Guard.ContraNuloOVacio(goc, nameof(goc));
            Ambiente = Guard.ContraNuloOVacio(ambiente, nameof(ambiente));
            Sistemas = Guard.ContraNulo(sistemas, nameof(sistemas)).ToList().AsReadOnly();
            UsuarioAplicacion = Guard.ContraNuloOVacio(usuarioAplicacion, nameof(usuarioAplicacion));
            FechaHora = fechaHora;
            GruposDestinatariosSeleccionados = (gruposDestinatariosSeleccionados ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
            DestinatariosAdicionales = (destinatariosAdicionales ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
            CanalesSeleccionados = (canalesSeleccionados ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
        }
    }
}
