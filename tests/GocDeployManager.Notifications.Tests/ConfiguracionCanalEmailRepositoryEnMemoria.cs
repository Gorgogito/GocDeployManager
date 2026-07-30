using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Notifications.Tests
{
    internal sealed class ConfiguracionCanalEmailRepositoryEnMemoria : IConfiguracionCanalEmailRepository
    {
        private ConfiguracionCanalEmail _configuracion;

        public ConfiguracionCanalEmailRepositoryEnMemoria(ConfiguracionCanalEmail configuracionInicial = null)
        {
            _configuracion = configuracionInicial;
        }

        public ConfiguracionCanalEmail Obtener() => _configuracion;

        public void Guardar(ConfiguracionCanalEmail configuracion) => _configuracion = configuracion;
    }
}
