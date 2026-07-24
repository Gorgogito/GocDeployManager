using System.ComponentModel;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.UI.Historial
{
    /// <summary>
    /// Proyección de solo lectura de <see cref="Despliegue"/> para mostrar en
    /// PropertyGridX — todas las propiedades son de solo lectura (sin setter),
    /// así que PropertyGrid las muestra automáticamente deshabilitadas.
    /// </summary>
    public sealed class DespliegueDetalle
    {
        [DisplayName("Fecha y hora")]
        public string FechaHora { get; }

        [DisplayName("GOC")]
        public string Goc { get; }

        [DisplayName("Rama")]
        public string Rama { get; }

        [DisplayName("Ambiente")]
        public string Ambiente { get; }

        [DisplayName("Sistemas")]
        public string Sistemas { get; }

        [DisplayName("Usuario de aplicación")]
        public string UsuarioAplicacion { get; }

        [DisplayName("Usuario Windows")]
        public string UsuarioWindows { get; }

        [DisplayName("Equipo")]
        public string Equipo { get; }

        [DisplayName("Resultado")]
        public string Resultado { get; }

        [DisplayName("Tiempo de compilación")]
        public string TiempoCompilacion { get; }

        [DisplayName("Tiempo de despliegue")]
        public string TiempoDespliegue { get; }

        [DisplayName("Errores")]
        public string Errores { get; }

        [DisplayName("Observaciones")]
        public string Observaciones { get; }

        public DespliegueDetalle(Despliegue despliegue)
        {
            FechaHora = despliegue.FechaHora.ToString("yyyy-MM-dd HH:mm:ss");
            Goc = despliegue.Goc;
            Rama = despliegue.Rama;
            Ambiente = despliegue.Ambiente;
            Sistemas = string.Join(", ", despliegue.Sistemas);
            UsuarioAplicacion = despliegue.UsuarioAplicacion;
            UsuarioWindows = despliegue.UsuarioWindows;
            Equipo = despliegue.Equipo;
            Resultado = despliegue.Resultado.ToString();
            TiempoCompilacion = despliegue.TiempoCompilacion.ToString(@"hh\:mm\:ss");
            TiempoDespliegue = despliegue.TiempoDespliegue.ToString(@"hh\:mm\:ss");
            Errores = string.IsNullOrEmpty(despliegue.Errores) ? "(ninguno)" : despliegue.Errores;
            Observaciones = string.IsNullOrEmpty(despliegue.Observaciones) ? "(ninguna)" : despliegue.Observaciones;
        }
    }
}
