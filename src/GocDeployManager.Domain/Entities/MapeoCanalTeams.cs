using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Un canal de Teams (URL de Incoming Webhook) destino para un
    /// Sistema/Ambiente. <see cref="Sistema"/>/<see cref="Ambiente"/> nulos
    /// actúan como comodín ("aplica a todos") — hoy el cliente usa un único
    /// mapeo comodín apuntando a "Sura Peru Teams" (análisis de
    /// notificaciones, sección 9), pero el modelo soporta mapeos específicos
    /// sin cambios de diseño.
    /// </summary>
    public sealed class MapeoCanalTeams
    {
        public string Sistema { get; }
        public string Ambiente { get; }
        public string UrlWebhook { get; }

        public MapeoCanalTeams(string sistema, string ambiente, string urlWebhook)
        {
            Sistema = string.IsNullOrWhiteSpace(sistema) ? null : sistema;
            Ambiente = string.IsNullOrWhiteSpace(ambiente) ? null : ambiente;
            UrlWebhook = Guard.ContraNuloOVacio(urlWebhook, nameof(urlWebhook));
        }

        /// <summary>Un mapeo con Sistema/Ambiente concretos es más específico
        /// que uno comodín — mayor puntaje significa mayor especificidad.</summary>
        public int Especificidad => (Sistema != null ? 1 : 0) + (Ambiente != null ? 1 : 0);

        public bool Aplica(string sistema, string ambiente) =>
            (Sistema == null || Sistema == sistema) && (Ambiente == null || Ambiente == ambiente);
    }
}
