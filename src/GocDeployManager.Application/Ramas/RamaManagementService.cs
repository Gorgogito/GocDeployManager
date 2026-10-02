using System;
using System.Collections.Generic;
using System.Linq;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Application.Ramas
{
    /// <summary>
    /// Administración de las ramas estándar que ofrece la pantalla principal
    /// (pestaña "Ramas" de Configuración, solo Administrador).
    /// </summary>
    public sealed class RamaManagementService
    {
        private readonly IRamaConfiguradaRepository _ramas;

        public RamaManagementService(IRamaConfiguradaRepository ramas)
        {
            _ramas = Guard.ContraNulo(ramas, nameof(ramas));
        }

        public IReadOnlyList<string> ObtenerTodas() => _ramas.ObtenerTodas();

        public Result Guardar(IEnumerable<string> ramas)
        {
            Guard.ContraNulo(ramas, nameof(ramas));

            var limpias = ramas
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .ToList();

            foreach (var rama in limpias)
            {
                var validacion = ReferenciaDespliegue.ValidarNombreRama(rama);
                if (validacion.IsFailure)
                    return validacion;
            }

            var hayDuplicadas = limpias
                .GroupBy(r => r, StringComparer.Ordinal)
                .Any(grupo => grupo.Count() > 1);

            if (hayDuplicadas)
                return Result.Fail("Hay ramas repetidas en la lista.");

            _ramas.Guardar(limpias);
            return Result.Ok();
        }
    }
}
