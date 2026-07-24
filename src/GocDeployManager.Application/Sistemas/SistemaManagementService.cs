using System;
using System.Collections.Generic;
using System.Linq;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Application.Sistemas
{
    /// <summary>
    /// CRUD de Sistemas.json: repositorio Bitbucket, carpeta precompilada y
    /// pasos de build por sistema (pantalla "Configuración › Bitbucket",
    /// sección 10 del análisis).
    /// </summary>
    public sealed class SistemaManagementService
    {
        private readonly ISistemaRepository _sistemas;

        public SistemaManagementService(ISistemaRepository sistemas)
        {
            _sistemas = Guard.ContraNulo(sistemas, nameof(sistemas));
        }

        public IReadOnlyList<ConfiguracionSistema> ObtenerTodos() => _sistemas.ObtenerTodasLasConfiguraciones();

        public Result Guardar(IReadOnlyList<ConfiguracionSistema> configuraciones)
        {
            if (configuraciones == null || configuraciones.Count == 0)
                return Result.Fail("Debe existir al menos un sistema configurado.");

            var hayCodigosDuplicados = configuraciones
                .GroupBy(c => c.Sistema.Codigo, StringComparer.OrdinalIgnoreCase)
                .Any(grupo => grupo.Count() > 1);

            if (hayCodigosDuplicados)
                return Result.Fail("Hay sistemas con el mismo código.");

            _sistemas.Guardar(configuraciones);
            return Result.Ok();
        }
    }
}
