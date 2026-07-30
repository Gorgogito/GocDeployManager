using System.Threading.Tasks;
using GocDeployManager.Common;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Notifications.Abstractions
{
    /// <summary>
    /// Estrategia de envío por canal (correo, Teams, y los que se agreguen a
    /// futuro sin tocar el resto del módulo — Strategy, análisis de
    /// notificaciones sección 5). Recibe el contenido ya renderizado; no
    /// conoce plantillas ni cómo se resolvieron los destinatarios.
    /// </summary>
    public interface ICanalNotificacion
    {
        string Nombre { get; }

        Task<Result> EnviarAsync(NotificacionPendiente notificacion);
    }
}
