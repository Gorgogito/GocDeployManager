using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;

namespace GocDeployManager.Application.Auth
{
    /// <summary>
    /// Login propio de la aplicación (sección 15 del análisis). El mensaje de
    /// error es deliberadamente genérico: no revela si falló el usuario o la
    /// contraseña.
    /// </summary>
    public sealed class AuthenticationService
    {
        private const string MensajeCredencialesInvalidas = "Usuario o contraseña incorrectos.";

        private readonly IAppUserRepository _usuarios;
        private readonly IPasswordHasher _hasher;
        private readonly ICredentialProtector _protector;
        private readonly IAppLogger _logger;

        public AuthenticationService(IAppUserRepository usuarios, IPasswordHasher hasher, ICredentialProtector protector, IAppLogger logger)
        {
            _usuarios = Guard.ContraNulo(usuarios, nameof(usuarios));
            _hasher = Guard.ContraNulo(hasher, nameof(hasher));
            _protector = Guard.ContraNulo(protector, nameof(protector));
            _logger = Guard.ContraNulo(logger, nameof(logger));
        }

        public Result<SesionUsuario> IniciarSesion(string nombreUsuario, string contrasenaPlano)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(contrasenaPlano))
            {
                _logger.Warn("Login rechazado: usuario o contraseña vacíos.");
                return Result.Fail<SesionUsuario>(MensajeCredencialesInvalidas);
            }

            var usuario = _usuarios.BuscarPorNombreUsuario(nombreUsuario.Trim());
            if (usuario == null || !usuario.Activo)
            {
                _logger.Warn($"Login rechazado para '{nombreUsuario.Trim()}': usuario inexistente o inactivo.");
                return Result.Fail<SesionUsuario>(MensajeCredencialesInvalidas);
            }

            if (!_hasher.Verificar(contrasenaPlano, usuario.HashContrasena, usuario.SalContrasena))
            {
                _logger.Warn($"Login rechazado para '{usuario.NombreUsuario}': contraseña incorrecta.");
                return Result.Fail<SesionUsuario>(MensajeCredencialesInvalidas);
            }

            string contrasenaBitbucket = null;
            if (!string.IsNullOrEmpty(usuario.ContrasenaBitbucketProtegida))
                contrasenaBitbucket = _protector.Desproteger(usuario.ContrasenaBitbucketProtegida);

            _logger.Info($"Login exitoso: '{usuario.NombreUsuario}' ({usuario.Rol}).");
            return Result.Ok(new SesionUsuario(usuario, contrasenaBitbucket));
        }
    }
}
