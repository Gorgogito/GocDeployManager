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
    /// Persistencia de GruposDestinatarios.json — mismo patrón de respaldo que
    /// <see cref="JsonAmbienteRepository"/>.
    /// </summary>
    public sealed class JsonGrupoDestinatariosRepository : IGrupoDestinatariosRepository
    {
        private readonly string _rutaArchivo;

        public JsonGrupoDestinatariosRepository(string rutaArchivo)
        {
            _rutaArchivo = Guard.ContraNuloOVacio(rutaArchivo, nameof(rutaArchivo));
        }

        public IReadOnlyList<GrupoDestinatarios> ObtenerTodos()
        {
            if (!File.Exists(_rutaArchivo))
                return new List<GrupoDestinatarios>().AsReadOnly();

            var json = File.ReadAllText(_rutaArchivo);
            var dtos = JsonConvert.DeserializeObject<List<GrupoDto>>(json) ?? new List<GrupoDto>();

            return dtos.Select(MapearAEntidad).ToList().AsReadOnly();
        }

        public void Guardar(IReadOnlyList<GrupoDestinatarios> grupos)
        {
            Guard.ContraNulo(grupos, nameof(grupos));

            var dtos = grupos.Select(MapearADto).ToList();
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

        private static GrupoDestinatarios MapearAEntidad(GrupoDto dto) =>
            new GrupoDestinatarios(
                dto.Nombre,
                dto.Miembros.Select(m => Destinatario.Crear(m.Nombre, m.CorreoElectronico).Value));

        private static GrupoDto MapearADto(GrupoDestinatarios grupo) => new GrupoDto
        {
            Nombre = grupo.Nombre,
            Miembros = grupo.Miembros.Select(m => new DestinatarioDto
            {
                Nombre = m.Nombre,
                CorreoElectronico = m.CorreoElectronico,
            }).ToList(),
        };

        private sealed class GrupoDto
        {
            public string Nombre { get; set; }
            public List<DestinatarioDto> Miembros { get; set; } = new List<DestinatarioDto>();
        }

        private sealed class DestinatarioDto
        {
            public string Nombre { get; set; }
            public string CorreoElectronico { get; set; }
        }
    }
}
