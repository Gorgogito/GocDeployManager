using System.Collections.Generic;
using GocDeployManager.Common;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Catálogo de sistemas conocidos (SIT, IDI, ProPag, ...) y su configuración
    /// de compilación/repositorio (Sistemas.json). Un sistema sin configuración
    /// (ej. uno recién dado de alta) es un escenario esperado, no una excepción.
    /// </summary>
    public interface ISistemaRepository
    {
        IReadOnlyList<Sistema> ObtenerSistemasConocidos();

        Result<ConfiguracionSistema> ObtenerConfiguracion(Sistema sistema);

        /// <summary>Todas las configuraciones a la vez, para la pantalla de administración.</summary>
        IReadOnlyList<ConfiguracionSistema> ObtenerTodasLasConfiguraciones();

        /// <summary>Reemplaza la lista completa, igual que <see cref="IAmbienteRepository.Guardar"/>.</summary>
        void Guardar(IReadOnlyList<ConfiguracionSistema> configuraciones);
    }
}
