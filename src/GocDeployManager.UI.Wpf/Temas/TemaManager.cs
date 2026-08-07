using MaterialDesignThemes.Wpf;

namespace GocDeployManager.UI.Temas
{
    public enum TemaBase { Claro, Oscuro }

    public static class TemaManager
    {
        private static readonly PaletteHelper _paletteHelper = new PaletteHelper();

        public static TemaBase Actual { get; private set; } = TemaBase.Claro;

        public static void Establecer(TemaBase tema)
        {
            Actual = tema;
            var theme = _paletteHelper.GetTheme();
            theme.SetBaseTheme(tema == TemaBase.Oscuro ? Theme.Dark : Theme.Light);
            _paletteHelper.SetTheme(theme);
        }
    }
}
