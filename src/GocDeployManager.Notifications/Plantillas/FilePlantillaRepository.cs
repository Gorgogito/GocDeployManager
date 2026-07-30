using System;
using System.IO;
using GocDeployManager.Common;
using GocDeployManager.Notifications.Abstractions;

namespace GocDeployManager.Notifications.Plantillas
{
    /// <summary>
    /// Plantillas como archivos de texto bajo RutaConfiguracion\Plantillas\
    /// {Canal}\{TipoEvento}.{ext} — editables sin recompilar (análisis de
    /// notificaciones, sección 13). Mismo patrón de respaldo que los
    /// repositorios JSON de Infrastructure.
    /// </summary>
    public sealed class FilePlantillaRepository : IPlantillaRepository
    {
        private readonly string _rutaBase;

        public FilePlantillaRepository(string rutaBase)
        {
            _rutaBase = Guard.ContraNuloOVacio(rutaBase, nameof(rutaBase));
        }

        public string Obtener(string canal, string tipoEvento)
        {
            var ruta = RutaArchivo(canal, tipoEvento);
            return File.Exists(ruta) ? File.ReadAllText(ruta) : ObtenerPorDefecto(canal, tipoEvento);
        }

        public void Guardar(string canal, string tipoEvento, string contenido)
        {
            Guard.ContraNuloOVacio(canal, nameof(canal));
            Guard.ContraNuloOVacio(tipoEvento, nameof(tipoEvento));

            var ruta = RutaArchivo(canal, tipoEvento);
            var carpeta = Path.GetDirectoryName(ruta);
            if (!string.IsNullOrEmpty(carpeta))
                Directory.CreateDirectory(carpeta);

            if (File.Exists(ruta))
                File.Copy(ruta, $"{ruta}.bak-{DateTime.Now:yyyyMMddHHmmss}", overwrite: true);

            File.WriteAllText(ruta, contenido ?? string.Empty);
        }

        public string ObtenerPorDefecto(string canal, string tipoEvento) => PlantillasPorDefecto.Obtener(canal, tipoEvento);

        public void RestaurarPorDefecto(string canal, string tipoEvento) => Guardar(canal, tipoEvento, ObtenerPorDefecto(canal, tipoEvento));

        private string RutaArchivo(string canal, string tipoEvento)
        {
            var extension = canal == NombresDeCanal.Teams ? "json" : "html";
            return Path.Combine(_rutaBase, canal, $"{tipoEvento}.{extension}");
        }
    }
}
