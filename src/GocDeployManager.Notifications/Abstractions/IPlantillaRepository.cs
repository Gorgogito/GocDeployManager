namespace GocDeployManager.Notifications.Abstractions
{
    /// <summary>
    /// Persistencia de las plantillas de mensaje — archivos de texto bajo
    /// RutaConfiguracion\Plantillas\{Canal}\{TipoEvento}, editables sin
    /// recompilar (análisis de notificaciones, sección 13). El editor de
    /// plantillas de ConfiguracionForm usa esta misma interfaz.
    /// </summary>
    public interface IPlantillaRepository
    {
        string Obtener(string canal, string tipoEvento);

        void Guardar(string canal, string tipoEvento, string contenido);

        string ObtenerPorDefecto(string canal, string tipoEvento);

        void RestaurarPorDefecto(string canal, string tipoEvento);
    }
}
