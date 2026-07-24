using System.Collections.Generic;
using System.IO;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using Newtonsoft.Json;

namespace GocDeployManager.Infrastructure.Json
{
    public sealed class JsonExclusionRulesRepository : IExclusionRulesRepository
    {
        private readonly string _rutaArchivo;

        public JsonExclusionRulesRepository(string rutaArchivo)
        {
            _rutaArchivo = Guard.ContraNuloOVacio(rutaArchivo, nameof(rutaArchivo));
        }

        public IReadOnlyList<string> ObtenerPatrones()
        {
            if (!File.Exists(_rutaArchivo))
                return new List<string> { "web.config" }.AsReadOnly();

            var json = File.ReadAllText(_rutaArchivo);
            var patrones = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();

            return patrones.AsReadOnly();
        }
    }
}
