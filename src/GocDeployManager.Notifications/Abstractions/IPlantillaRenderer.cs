using System.Collections.Generic;

namespace GocDeployManager.Notifications.Abstractions
{
    /// <summary>
    /// Reemplazo simple de tokens <c>{{Token}}</c> — sin motor de plantillas
    /// de terceros hasta que haga falta lógica condicional real dentro del
    /// texto (análisis de notificaciones, sección 13).
    /// </summary>
    public interface IPlantillaRenderer
    {
        string Renderizar(string plantilla, IReadOnlyDictionary<string, string> valores);
    }
}
