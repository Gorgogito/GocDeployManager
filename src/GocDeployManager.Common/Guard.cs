using System;

namespace GocDeployManager.Common
{
    /// <summary>
    /// Validaciones de invariantes reutilizadas por las entidades de Domain.
    /// </summary>
    public static class Guard
    {
        public static string ContraNuloOVacio(string valor, string nombreParametro)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("El valor no puede estar vacío.", nombreParametro);

            return valor;
        }

        public static T ContraNulo<T>(T valor, string nombreParametro) where T : class
        {
            if (valor is null)
                throw new ArgumentNullException(nombreParametro);

            return valor;
        }

        public static int Positivo(int valor, string nombreParametro)
        {
            if (valor <= 0)
                throw new ArgumentOutOfRangeException(nombreParametro, valor, "El valor debe ser mayor que cero.");

            return valor;
        }
    }
}
