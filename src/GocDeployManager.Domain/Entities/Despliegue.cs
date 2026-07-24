using System;
using System.Collections.Generic;
using System.Linq;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Registro de auditoría de un despliegue, exitoso o fallido — ambos son
    /// resultados válidos que siempre quedan registrados (sección 2 del análisis).
    /// Se construye vía factorías para garantizar que un despliegue fallido
    /// siempre tenga el detalle del error, y uno exitoso nunca lo tenga.
    /// </summary>
    public sealed class Despliegue
    {
        public DateTime FechaHora { get; }
        public string UsuarioAplicacion { get; }
        public string UsuarioWindows { get; }
        public string Equipo { get; }
        public string Goc { get; }
        public string Rama { get; }
        public string Ambiente { get; }
        public IReadOnlyList<string> Sistemas { get; }
        public TimeSpan TiempoCompilacion { get; }
        public TimeSpan TiempoDespliegue { get; }
        public ResultadoDespliegue Resultado { get; }
        public string Errores { get; }
        public string Observaciones { get; }

        private Despliegue(
            DateTime fechaHora,
            string usuarioAplicacion,
            string usuarioWindows,
            string equipo,
            string goc,
            string rama,
            string ambiente,
            IReadOnlyList<string> sistemas,
            TimeSpan tiempoCompilacion,
            TimeSpan tiempoDespliegue,
            ResultadoDespliegue resultado,
            string errores,
            string observaciones)
        {
            FechaHora = fechaHora;
            UsuarioAplicacion = usuarioAplicacion;
            UsuarioWindows = usuarioWindows;
            Equipo = equipo;
            Goc = goc;
            Rama = rama;
            Ambiente = ambiente;
            Sistemas = sistemas;
            TiempoCompilacion = tiempoCompilacion;
            TiempoDespliegue = tiempoDespliegue;
            Resultado = resultado;
            Errores = errores;
            Observaciones = observaciones;
        }

        public static Despliegue RegistrarExitoso(
            string usuarioAplicacion,
            string usuarioWindows,
            string equipo,
            string goc,
            string rama,
            string ambiente,
            IEnumerable<string> sistemas,
            TimeSpan tiempoCompilacion,
            TimeSpan tiempoDespliegue,
            string observaciones = null,
            DateTime? fechaHora = null)
        {
            return new Despliegue(
                fechaHora ?? DateTime.Now,
                RequerirTexto(usuarioAplicacion, nameof(usuarioAplicacion)),
                RequerirTexto(usuarioWindows, nameof(usuarioWindows)),
                RequerirTexto(equipo, nameof(equipo)),
                RequerirTexto(goc, nameof(goc)),
                RequerirTexto(rama, nameof(rama)),
                RequerirTexto(ambiente, nameof(ambiente)),
                RequerirSistemas(sistemas),
                tiempoCompilacion,
                tiempoDespliegue,
                ResultadoDespliegue.Exitoso,
                errores: null,
                observaciones: observaciones);
        }

        public static Despliegue RegistrarFallido(
            string usuarioAplicacion,
            string usuarioWindows,
            string equipo,
            string goc,
            string rama,
            string ambiente,
            IEnumerable<string> sistemas,
            TimeSpan tiempoCompilacion,
            TimeSpan tiempoDespliegue,
            string errores,
            string observaciones = null,
            DateTime? fechaHora = null)
        {
            return new Despliegue(
                fechaHora ?? DateTime.Now,
                RequerirTexto(usuarioAplicacion, nameof(usuarioAplicacion)),
                RequerirTexto(usuarioWindows, nameof(usuarioWindows)),
                RequerirTexto(equipo, nameof(equipo)),
                RequerirTexto(goc, nameof(goc)),
                RequerirTexto(rama, nameof(rama)),
                RequerirTexto(ambiente, nameof(ambiente)),
                RequerirSistemas(sistemas),
                tiempoCompilacion,
                tiempoDespliegue,
                ResultadoDespliegue.Fallido,
                errores: Guard.ContraNuloOVacio(errores, nameof(errores)),
                observaciones: observaciones);
        }

        private static string RequerirTexto(string valor, string nombreParametro) =>
            Guard.ContraNuloOVacio(valor, nombreParametro);

        private static IReadOnlyList<string> RequerirSistemas(IEnumerable<string> sistemas)
        {
            var lista = Guard.ContraNulo(sistemas, nameof(sistemas)).ToList();
            if (lista.Count == 0)
                throw new ArgumentException("Un despliegue debe registrar al menos un sistema.", nameof(sistemas));

            return lista.AsReadOnly();
        }
    }
}
