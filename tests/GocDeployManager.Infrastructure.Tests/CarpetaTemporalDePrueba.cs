using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace GocDeployManager.Infrastructure.Tests
{
    /// <summary>
    /// Carpeta temporal única por prueba, eliminada automáticamente al final.
    /// </summary>
    internal sealed class CarpetaTemporalDePrueba : IDisposable
    {
        public string Ruta { get; }

        public CarpetaTemporalDePrueba()
        {
            Ruta = Path.Combine(Path.GetTempPath(), "GocDeployManagerTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Ruta);
        }

        public void Dispose()
        {
            // Microsoft.Data.Sqlite reutiliza conexiones nativas en un pool;
            // sin esto el archivo .db queda bloqueado y el borrado falla.
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(Ruta))
                Directory.Delete(Ruta, recursive: true);
        }
    }
}
