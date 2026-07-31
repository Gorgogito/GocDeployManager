namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Etapa del despliegue mostrada como separador visual en el panel de
    /// salida en tiempo real. Deliberadamente separado de
    /// <see cref="EtapaDespliegue"/>, que sirve solo para clasificar en qué
    /// etapa ocurrió un fallo dentro de los eventos de notificación — no se
    /// reutiliza ese enum para no acoplar el panel de salida al sistema de
    /// notificaciones (todavía en pruebas).
    /// </summary>
    public enum EtapaProgreso
    {
        Validacion,
        Clonado,
        Compilacion,
        Despliegue,
        Finalizacion,
    }
}
