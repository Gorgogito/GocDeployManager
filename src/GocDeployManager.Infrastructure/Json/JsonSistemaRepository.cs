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
    /// Persistencia de Sistemas.json: catálogo de sistemas conocidos, cada uno
    /// con su propio repositorio Bitbucket, carpeta precompilada y secuencia
    /// de build (confirmado que difieren entre SIT, IDI y ProPag).
    /// </summary>
    public sealed class JsonSistemaRepository : ISistemaRepository
    {
        private readonly string _rutaArchivo;
        private List<ConfiguracionSistemaDto> _cache;

        public JsonSistemaRepository(string rutaArchivo)
        {
            _rutaArchivo = Guard.ContraNuloOVacio(rutaArchivo, nameof(rutaArchivo));
        }

        public IReadOnlyList<Sistema> ObtenerSistemasConocidos()
        {
            CargarSiEsNecesario();
            return _cache.Select(dto => new Sistema(dto.CodigoSistema, dto.NombreSistema)).ToList().AsReadOnly();
        }

        public Result<ConfiguracionSistema> ObtenerConfiguracion(Sistema sistema)
        {
            Guard.ContraNulo(sistema, nameof(sistema));
            CargarSiEsNecesario();

            var dto = _cache.FirstOrDefault(d => string.Equals(d.CodigoSistema, sistema.Codigo, StringComparison.OrdinalIgnoreCase));
            if (dto == null)
                return Result.Fail<ConfiguracionSistema>($"No hay configuración registrada para el sistema '{sistema.Codigo}'.");

            return Result.Ok(MapearAEntidad(dto));
        }

        public IReadOnlyList<ConfiguracionSistema> ObtenerTodasLasConfiguraciones()
        {
            CargarSiEsNecesario();
            return _cache.Select(MapearAEntidad).ToList().AsReadOnly();
        }

        public void Guardar(IReadOnlyList<ConfiguracionSistema> configuraciones)
        {
            Guard.ContraNulo(configuraciones, nameof(configuraciones));

            _cache = configuraciones.Select(MapearADto).ToList();
            var json = JsonConvert.SerializeObject(_cache, Formatting.Indented);

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

        private static ConfiguracionSistema MapearAEntidad(ConfiguracionSistemaDto dto)
        {
            var sistema = new Sistema(dto.CodigoSistema, dto.NombreSistema);
            var pasos = dto.Pasos.Select(p => new PasoDeBuild(p.Orden, p.CarpetaProyecto, p.ParametrosMsBuild));
            var secuencia = new SecuenciaDeBuild(sistema, pasos);

            return new ConfiguracionSistema(sistema, dto.RepositorioUrl, dto.CarpetaPrecompilada, secuencia);
        }

        private static ConfiguracionSistemaDto MapearADto(ConfiguracionSistema configuracion) => new ConfiguracionSistemaDto
        {
            CodigoSistema = configuracion.Sistema.Codigo,
            NombreSistema = configuracion.Sistema.Nombre,
            RepositorioUrl = configuracion.RepositorioUrl,
            CarpetaPrecompilada = configuracion.CarpetaPrecompilada,
            Pasos = configuracion.SecuenciaDeBuild.Pasos.Select(p => new PasoDeBuildDto
            {
                Orden = p.Orden,
                CarpetaProyecto = p.CarpetaProyecto,
                ParametrosMsBuild = p.ParametrosMsBuild,
            }).ToList(),
        };

        private void CargarSiEsNecesario()
        {
            if (_cache != null)
                return;

            if (!File.Exists(_rutaArchivo))
            {
                _cache = new List<ConfiguracionSistemaDto>();
                return;
            }

            var json = File.ReadAllText(_rutaArchivo);
            _cache = JsonConvert.DeserializeObject<List<ConfiguracionSistemaDto>>(json) ?? new List<ConfiguracionSistemaDto>();
        }

        private sealed class ConfiguracionSistemaDto
        {
            public string CodigoSistema { get; set; }
            public string NombreSistema { get; set; }
            public string RepositorioUrl { get; set; }
            public string CarpetaPrecompilada { get; set; }
            public List<PasoDeBuildDto> Pasos { get; set; } = new List<PasoDeBuildDto>();
        }

        private sealed class PasoDeBuildDto
        {
            public int Orden { get; set; }
            public string CarpetaProyecto { get; set; }
            public string ParametrosMsBuild { get; set; }
        }
    }
}
