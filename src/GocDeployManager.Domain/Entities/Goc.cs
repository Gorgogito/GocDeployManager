using System.Text.RegularExpressions;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Un GOC (corrección o mejora), identificado por un número con formato GOC-00000.
    /// Se construye vía <see cref="Crear"/> (no un constructor público) porque el
    /// número lo escribe el operador a mano: es exactamente el caso de "falla esperada,
    /// no excepcional" que debe modelarse con Result&lt;T&gt;.
    /// </summary>
    public sealed class Goc
    {
        private static readonly Regex FormatoValido = new Regex(@"^GOC-\d{5}$", RegexOptions.Compiled);

        public string Numero { get; }
        public string RamaBitbucket => $"feature/{Numero}";

        private Goc(string numero)
        {
            Numero = numero;
        }

        public static Result<Goc> Crear(string numeroCrudo)
        {
            if (string.IsNullOrWhiteSpace(numeroCrudo))
                return Result.Fail<Goc>("El número de GOC es obligatorio.");

            var normalizado = numeroCrudo.Trim().ToUpperInvariant();

            if (!FormatoValido.IsMatch(normalizado))
                return Result.Fail<Goc>($"'{numeroCrudo}' no tiene el formato esperado (GOC-00000).");

            return Result.Ok(new Goc(normalizado));
        }
    }
}
