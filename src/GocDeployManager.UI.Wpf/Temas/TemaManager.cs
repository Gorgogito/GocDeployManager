using System.Configuration;
using MaterialDesignThemes.Wpf;

namespace GocDeployManager.UI.Temas
{
    public enum TemaBase { Claro, Oscuro }

    public static class TemaManager
    {
        private const string ConfigKey = "TemaBase";
        private static readonly PaletteHelper _paletteHelper = new PaletteHelper();

        public static TemaBase Actual { get; private set; } = TemaBase.Claro;

        // Llamar una vez en App.OnStartup, antes de mostrar cualquier ventana.
        public static void Cargar()
        {
            var valor = ConfigurationManager.AppSettings[ConfigKey] ?? "Claro";
            Actual = valor == "Oscuro" ? TemaBase.Oscuro : TemaBase.Claro;
            AplicarSinGuardar();
        }

        // Cambia el tema y lo persiste en App.config.
        public static void Establecer(TemaBase tema)
        {
            Actual = tema;
            AplicarSinGuardar();
            Persistir();
        }

        private static void AplicarSinGuardar()
        {
            var theme = _paletteHelper.GetTheme();
            theme.SetBaseTheme(Actual == TemaBase.Oscuro ? Theme.Dark : Theme.Light);
            _paletteHelper.SetTheme(theme);
        }

        private static void Persistir()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                if (config.AppSettings.Settings[ConfigKey] == null)
                    config.AppSettings.Settings.Add(ConfigKey, Actual.ToString());
                else
                    config.AppSettings.Settings[ConfigKey].Value = Actual.ToString();

                config.Save(ConfigurationSaveMode.Modified);
            }
            catch
            {
                // Si el directorio de instalación es de solo lectura el tema
                // igual queda activo en la sesión, solo no se guarda.
            }
        }
    }
}
