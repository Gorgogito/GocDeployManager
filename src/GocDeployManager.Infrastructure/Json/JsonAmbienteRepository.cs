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
    /// <summary>
    /// Persistencia de Ambientes.json. Antes de sobrescribir, guarda un respaldo
    /// con timestamp del archivo anterior (sección 16 del análisis).
    /// </summary>
    public sealed class JsonAmbienteRepository : IAmbienteRepository
    {
        private readonly string _rutaArchivo;

        public JsonAmbienteRepository(string rutaArchivo)
        {
            _rutaArchivo = Guard.ContraNuloOVacio(rutaArchivo, nameof(rutaArchivo));
        }

        public IReadOnlyList<Ambiente> ObtenerTodos()
        {
            if (!File.Exists(_rutaArchivo))
                return new List<Ambiente>().AsReadOnly();

            var json = File.ReadAllText(_rutaArchivo);
            var dtos = JsonConvert.DeserializeObject<List<AmbienteDto>>(json) ?? new List<AmbienteDto>();

            return dtos.Select(MapearAEntidad).ToList().AsReadOnly();
        }

        public void Guardar(IReadOnlyList<Ambiente> ambientes)
        {
            Guard.ContraNulo(ambientes, nameof(ambientes));

            var dtos = ambientes.Select(MapearADto).ToList();
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

        private static Ambiente MapearAEntidad(AmbienteDto dto) =>
            new Ambiente(
                dto.Nombre,
                dto.Sistemas.Select(s => new AmbienteSistema(new Sistema(s.CodigoSistema, s.NombreSistema), s.RutaDestino)),
                dto.NotificacionesHabilitadas);

        private static AmbienteDto MapearADto(Ambiente ambiente) => new AmbienteDto
        {
            Nombre = ambiente.Nombre,
            Sistemas = ambiente.Sistemas.Select(s => new AmbienteSistemaDto
            {
                CodigoSistema = s.Sistema.Codigo,
                NombreSistema = s.Sistema.Nombre,
                RutaDestino = s.RutaDestino,
            }).ToList(),
            NotificacionesHabilitadas = ambiente.NotificacionesHabilitadas,
        };

        private sealed class AmbienteDto
        {
            public string Nombre { get; set; }
            public List<AmbienteSistemaDto> Sistemas { get; set; } = new List<AmbienteSistemaDto>();
            public bool NotificacionesHabilitadas { get; set; } = true;
        }

        private sealed class AmbienteSistemaDto
        {
            public string CodigoSistema { get; set; }
            public string NombreSistema { get; set; }
            public string RutaDestino { get; set; }
        }
    }
}
