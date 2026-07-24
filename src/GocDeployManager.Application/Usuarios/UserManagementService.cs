using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Application.Usuarios
{
    /// <summary>
    /// Alta, reseteo de contraseña y asignación de credenciales de Bitbucket
    /// (pantalla "Gestión de usuarios", sección 10 del análisis) — acciones
    /// reservadas al rol Administrador.
    /// </summary>
    public sealed class UserManagementService
    {
        private readonly IAppUserRepository _usuarios;
        private readonly IPasswordHasher _hasher;
        private readonly ICredentialProtector _protector;
        private readonly IAppLogger _logger;

        public UserManagementService(IAppUserRepository usuarios, IPasswordHasher hasher, ICredentialProtector protector, IAppLogger logger)
        {
            _usuarios = Guard.ContraNulo(usuarios, nameof(usuarios));
            _hasher = Guard.ContraNulo(hasher, nameof(hasher));
            _protector = Guard.ContraNulo(protector, nameof(protector));
            _logger = Guard.ContraNulo(logger, nameof(logger));
        }

        public System.Collections.Generic.IReadOnlyList<AppUser> ObtenerTodos() => _usuarios.ObtenerTodos();

        public Result CrearUsuario(string nombreUsuario, string nombreVisible, RolUsuario rol, string contrasenaInicial)
        {
            if (_usuarios.BuscarPorNombreUsuario(nombreUsuario) != null)
                return Result.Fail($"Ya existe un usuario '{nombreUsuario}'.");

            _hasher.Generar(contrasenaInicial, out var hash, out var sal);
            var usuario = new AppUser(nombreUsuario, nombreVisible, rol, hash, sal);

            _usuarios.Agregar(usuario);
            return Result.Ok();
        }

        public Result ResetearContrasena(string nombreUsuario, string nuevaContrasena)
        {
            var usuario = _usuarios.BuscarPorNombreUsuario(nombreUsuario);
            if (usuario == null)
                return Result.Fail($"No existe el usuario '{nombreUsuario}'.");

            _hasher.Generar(nuevaContrasena, out var hash, out var sal);
            usuario.ResetearContrasena(hash, sal);

            _usuarios.Actualizar(usuario);
            _logger.Info($"Contraseña reseteada para '{usuario.NombreUsuario}'.");
            return Result.Ok();
        }

        public Result EstablecerCredencialesBitbucket(string nombreUsuario, string usuarioBitbucket, string contrasenaBitbucketEnClaro)
        {
            var usuario = _usuarios.BuscarPorNombreUsuario(nombreUsuario);
            if (usuario == null)
                return Result.Fail($"No existe el usuario '{nombreUsuario}'.");

            var protegida = _protector.Proteger(contrasenaBitbucketEnClaro);
            usuario.EstablecerCredencialesBitbucket(usuarioBitbucket, protegida);

            _usuarios.Actualizar(usuario);
            return Result.Ok();
        }

        public Result CambiarRol(string nombreUsuario, RolUsuario nuevoRol)
        {
            var usuario = _usuarios.BuscarPorNombreUsuario(nombreUsuario);
            if (usuario == null)
                return Result.Fail($"No existe el usuario '{nombreUsuario}'.");

            var rolAnterior = usuario.Rol;
            usuario.CambiarRol(nuevoRol);
            _usuarios.Actualizar(usuario);

            _logger.Info($"Cambio de rol: '{usuario.NombreUsuario}' pasó de {rolAnterior} a {nuevoRol}.");
            return Result.Ok();
        }

        public Result CambiarEstado(string nombreUsuario, bool activo)
        {
            var usuario = _usuarios.BuscarPorNombreUsuario(nombreUsuario);
            if (usuario == null)
                return Result.Fail($"No existe el usuario '{nombreUsuario}'.");

            if (activo)
                usuario.Activar();
            else
                usuario.Desactivar();

            _usuarios.Actualizar(usuario);
            return Result.Ok();
        }
    }
}
