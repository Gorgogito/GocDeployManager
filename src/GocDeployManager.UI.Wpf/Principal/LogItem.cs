using System.Windows;
using System.Windows.Media;

namespace GocDeployManager.UI.Principal
{
    public sealed class LogItem
    {
        public string Hora { get; set; }
        public string Mensaje { get; set; }
        public Brush CorTexto { get; set; }
        public Brush CorFondo { get; set; }
        public FontWeight PesoFuente { get; set; }
    }
}
