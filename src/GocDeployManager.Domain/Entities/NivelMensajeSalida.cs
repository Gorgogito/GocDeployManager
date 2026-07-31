namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Severidad de un mensaje del panel de salida en tiempo real del
    /// despliegue. Independiente de <see cref="EtapaDespliegue"/> (que solo
    /// clasifica en qué etapa ocurrió un fallo para los eventos de
    /// notificación) y de los niveles de <c>IAppLogger</c> (log técnico a
    /// archivo) — este enum es exclusivamente para lo que ve el usuario en
    /// vivo mientras corre el despliegue.
    /// </summary>
    public enum NivelMensajeSalida
    {
        Info,
        Success,
        Warning,
        Error,
        Debug,
    }
}
