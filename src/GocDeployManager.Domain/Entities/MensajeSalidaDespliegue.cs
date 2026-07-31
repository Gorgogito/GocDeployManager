using System;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Un mensaje del panel de salida en tiempo real del despliegue (sección
    /// "Panel de salida en tiempo real"). Reemplaza al <c>IProgress&lt;string&gt;</c>
    /// de texto libre que usaba antes <c>DeploymentOrchestrator</c> — la UI ya
    /// no necesita adivinar severidad ni etapa a partir del texto.
    /// </summary>
    public sealed class MensajeSalidaDespliegue
    {
        public DateTime Momento { get; }
        public NivelMensajeSalida Nivel { get; }
        public string Texto { get; }

        /// <summary>
        /// No nulo únicamente cuando este mensaje marca el inicio de una
        /// etapa nueva — la UI lo usa para dibujar el separador visual.
        /// </summary>
        public EtapaProgreso? Etapa { get; }

        /// <summary>
        /// True solo en los hitos de alto nivel equivalentes a lo que
        /// mostraba el indicador de progreso antes de este panel (resolviendo
        /// configuración, clonando, cada paso de compilación, copiando
        /// archivos, y el cierre final) — la UI usa esta marca para decidir
        /// qué mensajes llegan al indicador de "Progreso del despliegue"
        /// (arriba), a diferencia del panel de salida (abajo), que siempre
        /// recibe todos los mensajes sin filtrar.
        /// </summary>
        public bool EsResumen { get; }

        private MensajeSalidaDespliegue(DateTime momento, NivelMensajeSalida nivel, string texto, EtapaProgreso? etapa, bool esResumen)
        {
            Momento = momento;
            Nivel = nivel;
            Texto = Guard.ContraNuloOVacio(texto, nameof(texto));
            Etapa = etapa;
            EsResumen = esResumen;
        }

        public static MensajeSalidaDespliegue Crear(NivelMensajeSalida nivel, string texto, bool esResumen = false) =>
            new MensajeSalidaDespliegue(DateTime.Now, nivel, texto, null, esResumen);

        public static MensajeSalidaDespliegue InicioDeEtapa(EtapaProgreso etapa, string texto) =>
            new MensajeSalidaDespliegue(DateTime.Now, NivelMensajeSalida.Info, texto, etapa, esResumen: false);
    }
}
