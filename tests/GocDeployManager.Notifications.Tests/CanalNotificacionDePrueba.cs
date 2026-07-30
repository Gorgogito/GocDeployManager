using System.Collections.Generic;
using System.Threading.Tasks;
using GocDeployManager.Common;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Notifications.Abstractions;

namespace GocDeployManager.Notifications.Tests
{
    internal sealed class CanalNotificacionDePrueba : ICanalNotificacion
    {
        public string Nombre { get; }
        public bool DebeFallar { get; set; }
        public List<int> IdsEnviados { get; } = new List<int>();

        public CanalNotificacionDePrueba(string nombre)
        {
            Nombre = nombre;
        }

        public Task<Result> EnviarAsync(NotificacionPendiente notificacion)
        {
            if (DebeFallar)
                return Task.FromResult(Result.Fail("fallo simulado"));

            IdsEnviados.Add(notificacion.Id);
            return Task.FromResult(Result.Ok());
        }
    }
}
