using System.Collections.Generic;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Persistencia de usuarios de la aplicación (tabla AppUser en SQLite).
    /// </summary>
    public interface IAppUserRepository
    {
        AppUser BuscarPorNombreUsuario(string nombreUsuario);

        IReadOnlyList<AppUser> ObtenerTodos();

        void Agregar(AppUser usuario);

        void Actualizar(AppUser usuario);
    }
}
