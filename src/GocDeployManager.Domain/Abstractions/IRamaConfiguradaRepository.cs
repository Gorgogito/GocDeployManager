using System.Collections.Generic;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Ramas estándar (master, develop...) que el administrador deja
    /// disponibles para desplegar sin escribirlas a mano. Tabla
    /// RamaConfigurada en SQL Server, compartida por todos los usuarios.
    /// Reemplaza la lista completa al guardar.
    /// </summary>
    public interface IRamaConfiguradaRepository
    {
        IReadOnlyList<string> ObtenerTodas();

        void Guardar(IReadOnlyList<string> ramas);
    }
}
