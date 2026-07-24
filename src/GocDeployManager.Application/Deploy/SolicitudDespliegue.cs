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

        public SolicitudDespliegue(
            Goc goc,
            Ambiente ambiente,
            IEnumerable<Sistema> sistemas,
            string usuarioAplicacion,
            string usuarioWindows,
            string equipo,
            string usuarioBitbucket,
            string contrasenaBitbucket,
            string rutaClonadoBase)
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
        }
    }
}
