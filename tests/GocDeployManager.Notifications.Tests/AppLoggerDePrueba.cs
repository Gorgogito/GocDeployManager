using System;
using System.Collections.Generic;
using GocDeployManager.Domain.Abstractions;

namespace GocDeployManager.Notifications.Tests
{
    internal sealed class AppLoggerDePrueba : IAppLogger
    {
        public List<string> Errores { get; } = new List<string>();

        public void Info(string mensaje) { }

        public void Warn(string mensaje) { }

        public void Error(string mensaje, Exception excepcion = null) => Errores.Add(mensaje);
    }
}
