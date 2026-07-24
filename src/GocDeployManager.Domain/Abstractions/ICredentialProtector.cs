namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Protección reversible de la credencial de Bitbucket de cada usuario
    /// (DPAPI atado al usuario Windows actual en la implementación de Services).
    /// </summary>
    public interface ICredentialProtector
    {
        string Proteger(string textoPlano);

        string Desproteger(string textoProtegido);
    }
}
