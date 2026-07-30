using System.Collections.Generic;
using System.Linq;
using GocDeployManager.Common;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Application.Deploy
{
    /// <summary>
    /// Todo lo que el operador eligió en la pantalla principal, más lo que la
    /// sesión ya conoce (usuario, credenciales de Bitbucket).
    /// </summary>
    public sealed class SolicitudDespliegue
    {
        public Goc Goc { get; }
        public Ambiente Ambiente { get; }
        public IReadOnlyList<Sistema> Sistemas { get; }
        public string UsuarioAplicacion { get; }
        public string UsuarioWindows { get; }
        public string Equipo { get; }
        public string UsuarioBitbucket { get; }
        public string ContrasenaBitbucket { get; }
        public string RutaClonadoBase { get; }

        /// <summary>
        /// Lo que el operador eligió en la sección "Notificaciones" de
        /// MainForm (análisis de notificaciones, sección 14). El orquestador
        /// no interpreta estos valores — solo los reenvía tal cual dentro de
        /// los eventos que publica.
        /// </summary>
        public bool NotificarResultado { get; }
        public IReadOnlyList<string> GruposDestinatariosSeleccionados { get; }
        public IReadOnlyList<string> DestinatariosAdicionales { get; }
        public IReadOnlyList<string> CanalesSeleccionados { get; }

        public SolicitudDespliegue(
            Goc goc,
            Ambiente ambiente,
            IEnumerable<Sistema> sistemas,
            string usuarioAplicacion,
            string usuarioWindows,
            string equipo,
            string usuarioBitbucket,
            string contrasenaBitbucket,
            string rutaClonadoBase,
            bool notificarResultado = false,
            IEnumerable<string> gruposDestinatariosSeleccionados = null,
            IEnumerable<string> destinatariosAdicionales = null,
            IEnumerable<string> canalesSeleccionados = null)
        {
            Goc = Guard.ContraNulo(goc, nameof(goc));
            Ambiente = Guard.ContraNulo(ambiente, nameof(ambiente));

            var listaSistemas = Guard.ContraNulo(sistemas, nameof(sistemas)).ToList();
            if (listaSistemas.Count == 0)
                throw new System.ArgumentException("Debe seleccionarse al menos un sistema.", nameof(sistemas));

            Sistemas = listaSistemas.AsReadOnly();
            UsuarioAplicacion = Guard.ContraNuloOVacio(usuarioAplicacion, nameof(usuarioAplicacion));
            UsuarioWindows = Guard.ContraNuloOVacio(usuarioWindows, nameof(usuarioWindows));
            Equipo = Guard.ContraNuloOVacio(equipo, nameof(equipo));
            UsuarioBitbucket = Guard.ContraNuloOVacio(usuarioBitbucket, nameof(usuarioBitbucket));
            ContrasenaBitbucket = Guard.ContraNuloOVacio(contrasenaBitbucket, nameof(contrasenaBitbucket));
            RutaClonadoBase = Guard.ContraNuloOVacio(rutaClonadoBase, nameof(rutaClonadoBase));

            NotificarResultado = notificarResultado;
            GruposDestinatariosSeleccionados = (gruposDestinatariosSeleccionados ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
            DestinatariosAdicionales = (destinatariosAdicionales ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
            CanalesSeleccionados = (canalesSeleccionados ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
        }
    }
}
