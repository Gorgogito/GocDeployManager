using System;
using System.Security.Cryptography;
using System.Text;
using GocDeployManager.Domain.Abstractions;

namespace GocDeployManager.Services
{
    /// <summary>
    /// Protege la credencial de Bitbucket de cada usuario con DPAPI atado al
    /// usuario Windows actual (confirmado: laptops personales e intransferibles
    /// por operador — sección 15 del análisis).
    /// </summary>
    public sealed class DpapiCredentialProtector : ICredentialProtector
    {
        private static readonly byte[] EntropiaAdicional = Encoding.UTF8.GetBytes("GocDeployManager.Bitbucket");

        public string Proteger(string textoPlano)
        {
            if (string.IsNullOrEmpty(textoPlano))
                throw new ArgumentException("El texto a proteger no puede estar vacío.", nameof(textoPlano));

            var bytesPlano = Encoding.UTF8.GetBytes(textoPlano);
            var bytesProtegidos = ProtectedData.Protect(bytesPlano, EntropiaAdicional, DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(bytesProtegidos);
        }

        public string Desproteger(string textoProtegido)
        {
            if (string.IsNullOrEmpty(textoProtegido))
                throw new ArgumentException("El texto a desproteger no puede estar vacío.", nameof(textoProtegido));

            var bytesProtegidos = Convert.FromBase64String(textoProtegido);
            var bytesPlano = ProtectedData.Unprotect(bytesProtegidos, EntropiaAdicional, DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(bytesPlano);
        }
    }
}
