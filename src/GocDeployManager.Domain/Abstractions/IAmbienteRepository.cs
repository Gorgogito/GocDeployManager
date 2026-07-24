using System.Collections.Generic;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Persistencia de Ambientes.json. Reemplaza la lista completa al guardar,
    /// igual que hoy se edita el archivo entero.
    /// </summary>
    public interface IAmbienteRepository
    {
        IReadOnlyList<Ambiente> ObtenerTodos();

        void Guardar(IReadOnlyList<Ambiente> ambientes);
    }
}
