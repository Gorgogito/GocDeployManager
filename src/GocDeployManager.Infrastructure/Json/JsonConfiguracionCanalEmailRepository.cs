using System;
using System.IO;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;
using Newtonsoft.Json;

namespace GocDeployManager.Infrastructure.Json
{
    /// <summary>Persistencia de CanalEmail.json — mismo patrón de respaldo que <see cref="JsonAmbienteRepository"/>.</summary>
    public sealed class JsonConfiguracionCanalEmailRepository : IConfiguracionCanalEmailRepository
    {
        private readonly string _rutaArchivo;

        public JsonConfiguracionCanalEmailRepository(string rutaArchivo)
        {
            _rutaArchivo = Guard.ContraNuloOVacio(rutaArchivo, nameof(rutaArchivo));
        }

        public ConfiguracionCanalEmail Obtener()
        {
            if (!File.Exists(_rutaArchivo))
                return null;

            var json = File.ReadAllText(_rutaArchivo);
            var dto = JsonConvert.DeserializeObject<ConfiguracionDto>(json);
            if (dto == null)
                return null;

            return new ConfiguracionCanalEmail(dto.Host, dto.Puerto, dto.Remitente, dto.NombreRemitente);
        }

        public void Guardar(ConfiguracionCanalEmail configuracion)
        {
            Guard.ContraNulo(configuracion, nameof(configuracion));

            var dto = new ConfiguracionDto
            {
                Host = configuracion.Host,
                Puerto = configuracion.Puerto,
                Remitente = configuracion.Remitente,
                NombreRemitente = configuracion.NombreRemitente,
            };
            var json = JsonConvert.SerializeObject(dto, Formatting.Indented);

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

        private sealed class ConfiguracionDto
        {
            public string Host { get; set; }
            public int Puerto { get; set; }
            public string Remitente { get; set; }
            public string NombreRemitente { get; set; }
        }
    }
}
