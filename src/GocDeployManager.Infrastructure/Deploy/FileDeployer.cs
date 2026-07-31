using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;

namespace GocDeployManager.Infrastructure.Deploy
{
    /// <summary>
    /// Reproduce el "del *.* /s /f /q" + "xcopy /E /R /Y" que hacen hoy los
    /// despliegue.bat (espejo completo del origen hacia el destino), pero
    /// nunca sobrescribe ni elimina un archivo de destino que coincida con un
    /// patrón de exclusión (ej. "web.config").
    /// </summary>
    public sealed class FileDeployer : IFileDeployer
    {
        public Result Copiar(string rutaOrigen, string rutaDestino, IReadOnlyList<string> patronesExclusion, Action<string, bool> onArchivo = null)
        {
            Guard.ContraNuloOVacio(rutaOrigen, nameof(rutaOrigen));
            Guard.ContraNuloOVacio(rutaDestino, nameof(rutaDestino));
            patronesExclusion = patronesExclusion ?? new List<string>();

            if (!Directory.Exists(rutaOrigen))
                return Result.Fail($"La carpeta origen '{rutaOrigen}' no existe.");

            try
            {
                Directory.CreateDirectory(rutaDestino);
                SincronizarDirectorio(new DirectoryInfo(rutaOrigen), new DirectoryInfo(rutaDestino), patronesExclusion, onArchivo);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Falló la copia hacia '{rutaDestino}': {ex.Message}", ex);
            }
        }

        private static void SincronizarDirectorio(DirectoryInfo origen, DirectoryInfo destino, IReadOnlyList<string> patronesExclusion, Action<string, bool> onArchivo)
        {
            var archivosOrigen = origen.GetFiles();
            var nombresArchivosOrigen = new HashSet<string>(archivosOrigen.Select(a => a.Name), StringComparer.OrdinalIgnoreCase);

            foreach (var archivo in archivosOrigen)
            {
                var rutaDestinoArchivo = Path.Combine(destino.FullName, archivo.Name);

                if (EstaExcluido(archivo.Name, patronesExclusion) && File.Exists(rutaDestinoArchivo))
                {
                    onArchivo?.Invoke(archivo.Name, true); // ya existe y está protegido: nunca se pisa
                    continue;
                }

                archivo.CopyTo(rutaDestinoArchivo, overwrite: true);
                onArchivo?.Invoke(archivo.Name, false);
            }

            if (destino.Exists)
            {
                foreach (var archivoDestino in destino.GetFiles())
                {
                    if (nombresArchivosOrigen.Contains(archivoDestino.Name))
                        continue;
                    if (EstaExcluido(archivoDestino.Name, patronesExclusion))
                        continue;

                    archivoDestino.Delete();
                }
            }

            var carpetasOrigen = origen.GetDirectories();
            var nombresCarpetasOrigen = new HashSet<string>(carpetasOrigen.Select(c => c.Name), StringComparer.OrdinalIgnoreCase);

            foreach (var subcarpeta in carpetasOrigen)
            {
                var destinoSubcarpeta = destino.CreateSubdirectory(subcarpeta.Name);
                SincronizarDirectorio(subcarpeta, destinoSubcarpeta, patronesExclusion, onArchivo);
            }

            if (destino.Exists)
            {
                foreach (var carpetaDestino in destino.GetDirectories())
                {
                    if (!nombresCarpetasOrigen.Contains(carpetaDestino.Name))
                        carpetaDestino.Delete(recursive: true);
                }
            }
        }

        private static bool EstaExcluido(string nombreArchivo, IReadOnlyList<string> patrones) =>
            patrones.Any(patron => CoincideConComodin(nombreArchivo, patron));

        private static bool CoincideConComodin(string texto, string patron)
        {
            var patronRegex = "^" + Regex.Escape(patron).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return Regex.IsMatch(texto, patronRegex, RegexOptions.IgnoreCase);
        }
    }
}
