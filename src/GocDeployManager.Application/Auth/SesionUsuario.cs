using GocDeployManager.Common;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Application.Auth
{
    /// <summary>
    /// La sesión del operador tras iniciar sesión: su usuario de aplicación y,
    /// si tiene, su contraseña de Bitbucket ya descifrada en memoria (nunca en
    /// disco) — lista para usarse en el clonado durante esta sesión.
    /// </summary>
    public sealed class SesionUsuario
    {
        public AppUser Usuario { get; }
        public string ContrasenaBitbucketEnClaro { get; }

        public SesionUsuario(AppUser usuario, string contrasenaBitbucketEnClaro)
        {
            Usuario = Guard.ContraNulo(usuario, nameof(usuario));
            ContrasenaBitbucketEnClaro = contrasenaBitbucketEnClaro;
        }
    }
}
