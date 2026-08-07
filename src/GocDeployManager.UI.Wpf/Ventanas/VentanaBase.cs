using System.Windows;
using System.Windows.Input;

namespace GocDeployManager.UI.Ventanas
{
    /// <summary>
    /// Base para todas las ventanas de la app. Provee el chrome personalizado
    /// (drag, botones de control) sin depender de WinForms MetroForm.
    /// </summary>
    public class VentanaBase : Window
    {
        protected void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        protected void MinimizeButton_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        protected void MaximizeButton_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

        protected void CloseButton_Click(object sender, RoutedEventArgs e)
            => Close();
    }
}
