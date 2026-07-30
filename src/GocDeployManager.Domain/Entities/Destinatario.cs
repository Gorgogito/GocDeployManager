using System.Text.RegularExpressions;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Una persona destinataria de notificaciones (miembro de un
    /// <see cref="GrupoDestinatarios"/>, o agregada ad-hoc al iniciar un
    /// despliegue). Se construye vía <see cref="Crear"/> porque el correo lo
    /// escribe un Administrador u operador a mano.
    /// </summary>
    public sealed class Destinatario
    {
        private static readonly Regex FormatoCorreoValido =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public string Nombre { get; }
        public string CorreoElectronico { get; }

        private Destinatario(string nombre, string correoElectronico)
        {
            Nombre = nombre;
            CorreoElectronico = correoElectronico;
        }

        public static Result<Destinatario> Crear(string nombre, string correoElectronico)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return Result.Fail<Destinatario>("El nombre del destinatario es obligatorio.");

            if (string.IsNullOrWhiteSpace(correoElectronico))
                return Result.Fail<Destinatario>("El correo electrónico es obligatorio.");

            var correoNormalizado = correoElectronico.Trim();
            if (!FormatoCorreoValido.IsMatch(correoNormalizado))
                return Result.Fail<Destinatario>($"'{correoElectronico}' no es un correo electrónico válido.");

            return Result.Ok(new Destinatario(nombre.Trim(), correoNormalizado));
        }
    }
}
