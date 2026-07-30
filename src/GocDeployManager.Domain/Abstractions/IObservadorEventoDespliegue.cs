using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Quien reacciona a un evento de despliegue (p. ej. el dispatcher de
    /// notificaciones). El publicador no conoce ninguna implementación
    /// concreta de esta interfaz.
    /// </summary>
    public interface IObservadorEventoDespliegue
    {
        void Manejar(IEventoDespliegue evento);
    }
}
