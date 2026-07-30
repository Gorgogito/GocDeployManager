using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Punto único de desacople entre <c>DeploymentOrchestrator</c> y
    /// cualquier interesado en el resultado de un despliegue (notificaciones
    /// u otro). El orquestador solo conoce esta interfaz de dos métodos —
    /// nunca un canal concreto (correo, Teams, ...).
    /// </summary>
    public interface IPublicadorEventosDespliegue
    {
        void Suscribir(IObservadorEventoDespliegue observador);

        void Publicar(IEventoDespliegue evento);
    }
}
