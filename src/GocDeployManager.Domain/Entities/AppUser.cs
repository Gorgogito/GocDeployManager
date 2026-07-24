using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Usuario propio de la aplicación. Guarda solo el hash+sal de su contraseña
    /// (nunca la contraseña en sí) y, si tiene, su credencial de Bitbucket ya
    /// protegida (DPAPI) — nunca en texto plano.
    /// </summary>
    public sealed class AppUser
    {
        public string NombreUsuario { get; }
        public string NombreVisible { get; private set; }
        public RolUsuario Rol { get; private set; }
        public bool Activo { get; private set; }
        public string HashContrasena { get; private set; }
        public string SalContrasena { get; private set; }
        public string UsuarioBitbucket { get; private set; }
        public string ContrasenaBitbucketProtegida { get; private set; }

        public AppUser(string nombreUsuario, string nombreVisible, RolUsuario rol, string hashContrasena, string salContrasena)
        {
            NombreUsuario = Guard.ContraNuloOVacio(nombreUsuario, nameof(nombreUsuario));
            NombreVisible = Guard.ContraNuloOVacio(nombreVisible, nameof(nombreVisible));
            Rol = rol;
            HashContrasena = Guard.ContraNuloOVacio(hashContrasena, nameof(hashContrasena));
            SalContrasena = Guard.ContraNuloOVacio(salContrasena, nameof(salContrasena));
            Activo = true;
        }

        public bool PuedeDesplegar => Rol == RolUsuario.Administrador || Rol == RolUsuario.Operador;
        public bool PuedeAdministrar => Rol == RolUsuario.Administrador;

        public void ResetearContrasena(string nuevoHash, string nuevaSal)
        {
            HashContrasena = Guard.ContraNuloOVacio(nuevoHash, nameof(nuevoHash));
            SalContrasena = Guard.ContraNuloOVacio(nuevaSal, nameof(nuevaSal));
        }

        public void EstablecerCredencialesBitbucket(string usuarioBitbucket, string contrasenaProtegida)
        {
            UsuarioBitbucket = Guard.ContraNuloOVacio(usuarioBitbucket, nameof(usuarioBitbucket));
            ContrasenaBitbucketProtegida = Guard.ContraNuloOVacio(contrasenaProtegida, nameof(contrasenaProtegida));
        }

        public void CambiarRol(RolUsuario nuevoRol) => Rol = nuevoRol;

        public void Activar() => Activo = true;

        public void Desactivar() => Activo = false;
    }
}
