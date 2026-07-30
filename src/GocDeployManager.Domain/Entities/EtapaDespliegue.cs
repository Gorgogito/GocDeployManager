namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Etapa del despliegue en la que ocurrió un fallo — corresponde 1 a 1 con
    /// los puntos de retorno de <c>DeploymentOrchestrator.EjecutarDespliegue</c>.
    /// </summary>
    public enum EtapaDespliegue
    {
        ResolucionConfiguracion,
        Clonado,
        Compilacion,
        Copia,
    }
}
