using System;
using System.Security.Cryptography;
using GocDeployManager.Domain.Abstractions;

namespace GocDeployManager.Services
{
    /// <summary>
    /// Hash unidireccional de la contraseña del login propio (PBKDF2 / Rfc2898DeriveBytes,
    /// incluido en .NET Framework, sin dependencias nuevas). Nunca se guarda ni se
    /// recupera la contraseña en texto plano — solo se puede verificar o resetear.
    /// </summary>
    public sealed class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int TamanoSalBytes = 16;
        private const int TamanoHashBytes = 32;
        private const int Iteraciones = 100_000;

        public void Generar(string contrasenaPlano, out string hash, out string sal)
        {
            if (string.IsNullOrEmpty(contrasenaPlano))
                throw new ArgumentException("La contraseña no puede estar vacía.", nameof(contrasenaPlano));

            var salBytes = new byte[TamanoSalBytes];
            using (var generador = RandomNumberGenerator.Create())
                generador.GetBytes(salBytes);

            var hashBytes = CalcularHash(contrasenaPlano, salBytes);

            sal = Convert.ToBase64String(salBytes);
            hash = Convert.ToBase64String(hashBytes);
        }

        public bool Verificar(string contrasenaPlano, string hash, string sal)
        {
            if (string.IsNullOrEmpty(contrasenaPlano) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(sal))
                return false;

            var salBytes = Convert.FromBase64String(sal);
            var hashCalculado = CalcularHash(contrasenaPlano, salBytes);
            var hashEsperado = Convert.FromBase64String(hash);

            return SonIguales(hashCalculado, hashEsperado);
        }

        private static byte[] CalcularHash(string contrasenaPlano, byte[] sal)
        {
            using (var derivador = new Rfc2898DeriveBytes(contrasenaPlano, sal, Iteraciones, HashAlgorithmName.SHA256))
                return derivador.GetBytes(TamanoHashBytes);
        }

        private static bool SonIguales(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            var diferencia = 0;
            for (var i = 0; i < a.Length; i++)
                diferencia |= a[i] ^ b[i];

            return diferencia == 0;
        }
    }
}
