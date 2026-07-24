namespace DesktopComponents.Theming
{
    /// <summary>
    /// Todo control/formulario de DesktopComponents lo implementa para
    /// repintarse cuando cambia el tema activo.
    /// </summary>
    public interface IThemedControl
    {
        void AplicarTema(Theme tema);
    }
}
