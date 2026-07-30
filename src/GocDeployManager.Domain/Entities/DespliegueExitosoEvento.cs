using System;
using System.Collections.Generic;
using System.Linq;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    public sealed class DespliegueExitosoEvento : IEventoDespliegue
    {
        public int DespliegueId { get; }
        public string Goc { get; }
        public string Rama { get; }
        public string Ambiente { get; }
        public IReadOnlyList<string> Sistemas { get; }
        public string UsuarioAplicacion { get; }
        public DateTime FechaHoraInicio { get; }
        public DateTime FechaHora { get; }
        public TimeSpan Duracion => FechaHora - FechaHoraInicio;
        public IReadOnlyList<string> GruposDestinatariosSeleccionados { get; }
        public IReadOnlyList<string> DestinatariosAdicionales { get; }
        public IReadOnlyList<string> CanalesSeleccionados { get; }

        public DespliegueExitosoEvento(
            int despliegueId,
            string goc,
            string rama,
            string ambiente,
            IEnumerable<string> sistemas,
            string usuarioAplicacion,
            DateTime fechaHoraInicio,
            DateTime fechaHoraFin,
            IEnumerable<string> gruposDestinatariosSeleccionados = null,
            IEnumerable<string> destinatariosAdicionales = null,
            IEnumerable<string> canalesSeleccionados = null)
        {
            DespliegueId = despliegueId;
            Goc = Guard.ContraNuloOVacio(goc, nameof(goc));
            Rama = Guard.ContraNuloOVacio(rama, nameof(rama));
            Ambiente = Guard.ContraNuloOVacio(ambiente, nameof(ambiente));
            Sistemas = Guard.ContraNulo(sistemas, nameof(sistemas)).ToList().AsReadOnly();
            UsuarioAplicacion = Guard.ContraNuloOVacio(usuarioAplicacion, nameof(usuarioAplicacion));
            FechaHoraInicio = fechaHoraInicio;
            FechaHora = fechaHoraFin;
            GruposDestinatariosSeleccionados = (gruposDestinatariosSeleccionados ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
            DestinatariosAdicionales = (destinatariosAdicionales ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
            CanalesSeleccionados = (canalesSeleccionados ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
        }
    }
}
