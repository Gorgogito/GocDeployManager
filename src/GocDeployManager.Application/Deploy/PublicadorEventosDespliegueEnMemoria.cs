using System.Collections.Generic;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Application.Deploy
{
    /// <summary>
    /// Implementación por defecto de <see cref="IPublicadorEventosDespliegue"/>
    /// — una lista de observadores en memoria, sin dependencias de
    /// infraestructura. Aísla al orquestador de cualquier fallo de un
    /// observador: una excepción en, por ejemplo, el dispatcher de
    /// notificaciones nunca debe hacer fallar un despliegue (análisis de
    /// notificaciones, sección 5).
    /// </summary>
    public sealed class PublicadorEventosDespliegueEnMemoria : IPublicadorEventosDespliegue
    {
        private readonly List<IObservadorEventoDespliegue> _observadores = new List<IObservadorEventoDespliegue>();
        private readonly IAppLogger _logger;

        public PublicadorEventosDespliegueEnMemoria(IAppLogger logger)
        {
            _logger = Guard.ContraNulo(logger, nameof(logger));
        }

        public void Suscribir(IObservadorEventoDespliegue observador)
        {
            _observadores.Add(Guard.ContraNulo(observador, nameof(observador)));
        }

        public void Publicar(IEventoDespliegue evento)
        {
            Guard.ContraNulo(evento, nameof(evento));

            foreach (var observador in _observadores)
            {
                try
                {
                    observador.Manejar(evento);
                }
                catch (System.Exception ex)
                {
                    _logger.Error($"Un observador de eventos de despliegue falló al procesar {evento.GetType().Name} (GOC={evento.Goc}). No afecta el resultado del despliegue.", ex);
                }
            }
        }
    }
}
