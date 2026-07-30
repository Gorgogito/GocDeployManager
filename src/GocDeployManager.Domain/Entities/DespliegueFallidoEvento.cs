using System;
using System.Collections.Generic;
using System.Linq;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    public sealed class DespliegueFallidoEvento : IEventoDespliegue
    {
        public int DespliegueId { get; }
        public string Goc { get; }
        public string Ambiente { get; }
        public IReadOnlyList<string> Sistemas { get; }
        public string UsuarioAplicacion { get; }
        public DateTime FechaHora { get; }
        public EtapaDespliegue Etapa { get; }
        public string MensajeError { get; }
        public IReadOnlyList<string> GruposDestinatariosSeleccionados { get; }
        public IReadOnlyList<string> DestinatariosAdicionales { get; }
        public IReadOnlyList<string> CanalesSeleccionados { get; }

        public DespliegueFallidoEvento(
            int despliegueId,
            string goc,
            string ambiente,
            IEnumerable<string> sistemas,
            string usuarioAplicacion,
            DateTime fechaHora,
            EtapaDespliegue etapa,
            string mensajeError,
            IEnumerable<string> gruposDestinatariosSeleccionados = null,
            IEnumerable<string> destinatariosAdicionales = null,
            IEnumerable<string> canalesSeleccionados = null)
        {
            DespliegueId = despliegueId;
            Goc = Guard.ContraNuloOVacio(goc, nameof(goc));
            Ambiente = Guard.ContraNuloOVacio(ambiente, nameof(ambiente));
            Sistemas = Guard.ContraNulo(sistemas, nameof(sistemas)).ToList().AsReadOnly();
            UsuarioAplicacion = Guard.ContraNuloOVacio(usuarioAplicacion, nameof(usuarioAplicacion));
            FechaHora = fechaHora;
            Etapa = etapa;
            MensajeError = Guard.ContraNuloOVacio(mensajeError, nameof(mensajeError));
            GruposDestinatariosSeleccionados = (gruposDestinatariosSeleccionados ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
            DestinatariosAdicionales = (destinatariosAdicionales ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
            CanalesSeleccionados = (canalesSeleccionados ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
        }
    }
}
