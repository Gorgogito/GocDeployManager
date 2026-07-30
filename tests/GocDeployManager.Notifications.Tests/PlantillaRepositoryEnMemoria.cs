using System.Collections.Generic;
using GocDeployManager.Notifications.Abstractions;
using GocDeployManager.Notifications.Plantillas;

namespace GocDeployManager.Notifications.Tests
{
    internal sealed class PlantillaRepositoryEnMemoria : IPlantillaRepository
    {
        private readonly Dictionary<(string, string), string> _guardadas = new Dictionary<(string, string), string>();

        public string Obtener(string canal, string tipoEvento) =>
            _guardadas.TryGetValue((canal, tipoEvento), out var contenido) ? contenido : ObtenerPorDefecto(canal, tipoEvento);

        public void Guardar(string canal, string tipoEvento, string contenido) => _guardadas[(canal, tipoEvento)] = contenido;

        public string ObtenerPorDefecto(string canal, string tipoEvento) => PlantillasPorDefecto.Obtener(canal, tipoEvento);

        public void RestaurarPorDefecto(string canal, string tipoEvento) => _guardadas.Remove((canal, tipoEvento));
    }
}
