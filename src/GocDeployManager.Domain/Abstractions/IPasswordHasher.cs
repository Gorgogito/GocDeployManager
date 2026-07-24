namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Hash unidireccional de la contraseña del login propio (PBKDF2 en la
    /// implementación de Services — nunca se guarda ni se recupera en texto plano).
    /// </summary>
    public interface IPasswordHasher
    {
        void Generar(string contrasenaPlano, out string hash, out string sal);

        bool Verificar(string contrasenaPlano, string hash, string sal);
    }
}
