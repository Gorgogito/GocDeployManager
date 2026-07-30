using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;
using Newtonsoft.Json;

namespace GocDeployManager.Infrastructure.Json
{
    /// <summary>Persistencia de CanalTeams.json — mismo patrón de respaldo que <see cref="JsonAmbienteRepository"/>.</summary>
    public sealed class JsonConfiguracionCanalTeamsRepository : IConfiguracionCanalTeamsRepository
    {
        private readonly string _rutaArchivo;

        public JsonConfiguracionCanalTeamsRepository(string rutaArchivo)
        {
            _rutaArchivo = Guard.ContraNuloOVacio(rutaArchivo, nameof(rutaArchivo));
        }

        public IReadOnlyList<MapeoCanalTeams> ObtenerTodos()
        {
            if (!File.Exists(_rutaArchivo))
                return new List<MapeoCanalTeams>().AsReadOnly();

            var json = File.ReadAllText(_rutaArchivo);
            var dtos = JsonConvert.DeserializeObject<List<MapeoDto>>(json) ?? new List<MapeoDto>();

            return dtos.Select(d => new MapeoCanalTeams(d.Sistema, d.Ambiente, d.UrlWebhook)).ToList().AsReadOnly();
        }

        public void Guardar(IReadOnlyList<MapeoCanalTeams> mapeos)
        {
            Guard.ContraNulo(mapeos, nameof(mapeos));

            var dtos = mapeos.Select(m => new MapeoDto { Sistema = m.Sistema, Ambiente = m.Ambiente, UrlWebhook = m.UrlWebhook }).ToList();
            var json = JsonConvert.SerializeObject(dtos, Formatting.Indented);

            RespaldarArchivoExistente();

            var carpeta = Path.GetDirectoryName(_rutaArchivo);
            if (!string.IsNullOrEmpty(carpeta))
                Directory.CreateDirectory(carpeta);

            File.WriteAllText(_rutaArchivo, json);
        }

        private void RespaldarArchivoExistente()
        {
            if (!File.Exists(_rutaArchivo))
                return;

            var rutaRespaldo = $"{_rutaArchivo}.bak-{DateTime.Now:yyyyMMddHHmmss}";
            File.Copy(_rutaArchivo, rutaRespaldo, overwrite: true);
        }

        private sealed class MapeoDto
        {
            public string Sistema { get; set; }
            public string Ambiente { get; set; }
            public string UrlWebhook { get; set; }
        }
    }
}
