using System;
using System.IO;

namespace GocDeployManager.Notifications.Tests
{
    internal sealed class CarpetaTemporalDePrueba : IDisposable
    {
        public string Ruta { get; }

        public CarpetaTemporalDePrueba()
        {
            Ruta = Path.Combine(Path.GetTempPath(), "GocDeployManagerNotificationsTests_" + Guid.NewGuid());
            Directory.CreateDirectory(Ruta);
        }

        public void Dispose()
        {
            if (Directory.Exists(Ruta))
                Directory.Delete(Ruta, recursive: true);
        }
    }
}
