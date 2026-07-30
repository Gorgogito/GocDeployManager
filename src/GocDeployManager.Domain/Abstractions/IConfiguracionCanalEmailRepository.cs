using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>Persistencia de CanalEmail.json.</summary>
    public interface IConfiguracionCanalEmailRepository
    {
        ConfiguracionCanalEmail Obtener();

        void Guardar(ConfiguracionCanalEmail configuracion);
    }
}
