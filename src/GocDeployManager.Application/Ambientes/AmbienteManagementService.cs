using System;
using System.Collections.Generic;
using System.Linq;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Application.Ambientes
{
    /// <summary>
    /// CRUD de Ambientes.json (pantalla "Configuración de ambientes", sección 10).
    /// </summary>
    public sealed class AmbienteManagementService
    {
        private readonly IAmbienteRepository _ambientes;

        public AmbienteManagementService(IAmbienteRepository ambientes)
        {
            _ambientes = Guard.ContraNulo(ambientes, nameof(ambientes));
        }

        public IReadOnlyList<Ambiente> ObtenerTodos() => _ambientes.ObtenerTodos();

        public Result Guardar(IReadOnlyList<Ambiente> ambientes)
        {
            if (ambientes == null || ambientes.Count == 0)
                return Result.Fail("Debe existir al menos un ambiente configurado.");

            var hayNombresDuplicados = ambientes
                .GroupBy(a => a.Nombre, StringComparer.OrdinalIgnoreCase)
                .Any(grupo => grupo.Count() > 1);

            if (hayNombresDuplicados)
                return Result.Fail("Hay ambientes con el mismo nombre.");

            _ambientes.Guardar(ambientes);
            return Result.Ok();
        }
    }
}
