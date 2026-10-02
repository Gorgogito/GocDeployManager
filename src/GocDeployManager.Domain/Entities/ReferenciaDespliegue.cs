using System.IO;
using System.Linq;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Qué se va a desplegar: la rama de Bitbucket resultante, ya sea porque
    /// el operador indicó un GOC (rama feature/GOC-00000), eligió una rama
    /// estándar configurada (master, develop...) o escribió otra rama a mano.
    /// El resto del flujo solo ve <see cref="Rama"/>, <see cref="Etiqueta"/>
    /// (lo que se muestra/guarda en el historial) y <see cref="CarpetaTrabajo"/>.
    /// </summary>
    public sealed class ReferenciaDespliegue
    {
        public const int LongitudMaximaRama = 200;
        private const int LongitudMaximaEtiqueta = 50; // columna DeployHistory.Goc

        private static readonly char[] CaracteresProhibidos = { '~', '^', ':', '?', '*', '[', '\\', '"' };

        public string Etiqueta { get; }
        public string Rama { get; }
        public string CarpetaTrabajo { get; }
        public bool EsGoc { get; }

        private ReferenciaDespliegue(string etiqueta, string rama, string carpetaTrabajo, bool esGoc)
        {
            Etiqueta = etiqueta;
            Rama = rama;
            CarpetaTrabajo = carpetaTrabajo;
            EsGoc = esGoc;
        }

        public static ReferenciaDespliegue DesdeGoc(Goc goc)
        {
            Guard.ContraNulo(goc, nameof(goc));
            return new ReferenciaDespliegue(goc.Numero, goc.RamaBitbucket, goc.Numero, esGoc: true);
        }

        /// <summary>
        /// Rama literal (estándar o escrita a mano). Se valida contra las
        /// reglas básicas de nombres de rama de git.
        /// </summary>
        public static Result<ReferenciaDespliegue> DesdeRama(string nombreRama)
        {
            var validacion = ValidarNombreRama(nombreRama);
            if (validacion.IsFailure)
                return Result.Fail<ReferenciaDespliegue>(validacion.Error);

            var rama = nombreRama.Trim();
            var etiqueta = rama.Length > LongitudMaximaEtiqueta ? rama.Substring(0, LongitudMaximaEtiqueta - 3) + "..." : rama;

            return Result.Ok(new ReferenciaDespliegue(etiqueta, rama, NombreDeCarpeta(rama), esGoc: false));
        }

        public static Result ValidarNombreRama(string nombreRama)
        {
            if (string.IsNullOrWhiteSpace(nombreRama))
                return Result.Fail("El nombre de la rama es obligatorio.");

            var rama = nombreRama.Trim();

            if (rama.Length > LongitudMaximaRama)
                return Result.Fail($"El nombre de la rama no puede superar {LongitudMaximaRama} caracteres.");

            if (rama.Any(c => char.IsWhiteSpace(c) || char.IsControl(c)) || rama.IndexOfAny(CaracteresProhibidos) >= 0)
                return Result.Fail($"'{rama}' contiene espacios o caracteres no permitidos en una rama (~ ^ : ? * [ \\ \").");

            if (rama.Contains("..") || rama.Contains("@{") || rama.Contains("//")
                || rama.StartsWith("-") || rama.StartsWith("/") || rama.EndsWith("/")
                || rama.EndsWith(".") || rama.EndsWith(".lock") || rama == "@"
                || rama.Split('/').Any(parte => parte.StartsWith(".")))
                return Result.Fail($"'{rama}' no es un nombre de rama válido.");

            return Result.Ok();
        }

        private static string NombreDeCarpeta(string rama)
        {
            var invalidos = Path.GetInvalidFileNameChars();
            return new string(rama.Select(c => c == '/' || invalidos.Contains(c) ? '_' : c).ToArray());
        }
    }
}
