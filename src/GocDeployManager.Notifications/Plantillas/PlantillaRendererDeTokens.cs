using System.Collections.Generic;
using GocDeployManager.Notifications.Abstractions;

namespace GocDeployManager.Notifications.Plantillas
{
    public sealed class PlantillaRendererDeTokens : IPlantillaRenderer
    {
        public string Renderizar(string plantilla, IReadOnlyDictionary<string, string> valores)
        {
            var resultado = plantilla ?? string.Empty;

            foreach (var par in valores)
                resultado = resultado.Replace("{{" + par.Key + "}}", par.Value ?? string.Empty);

            return resultado;
        }
    }
}
