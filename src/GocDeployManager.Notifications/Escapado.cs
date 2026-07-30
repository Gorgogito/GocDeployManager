using System.Text;

namespace GocDeployManager.Notifications
{
    /// <summary>
    /// Los valores que se insertan en una plantilla (en particular
    /// <c>MensajeError</c>, que viene de una herramienta externa como
    /// MSBuild y puede traer comillas o texto arbitrario) deben escaparse
    /// según el formato de la plantilla — HTML para correo, JSON para la
    /// tarjeta adaptable de Teams — para no romper la estructura del mensaje.
    /// </summary>
    public static class Escapado
    {
        public static string ParaHtml(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return string.Empty;

            return valor
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("\r\n", "<br>")
                .Replace("\n", "<br>");
        }

        public static string ParaJson(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return string.Empty;

            var resultado = new StringBuilder(valor.Length);
            foreach (var c in valor)
            {
                switch (c)
                {
                    case '\\': resultado.Append("\\\\"); break;
                    case '"': resultado.Append("\\\""); break;
                    case '\r': break;
                    case '\n': resultado.Append("\\n"); break;
                    case '\t': resultado.Append("\\t"); break;
                    default: resultado.Append(c); break;
                }
            }
            return resultado.ToString();
        }
    }
}
