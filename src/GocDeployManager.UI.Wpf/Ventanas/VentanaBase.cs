using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace GocDeployManager.UI.Ventanas
{
    public class VentanaBase : Window
    {
        public VentanaBase()
        {
            BorderThickness = new Thickness(1);
            SetResourceReference(BorderBrushProperty, "PrimaryHueMidBrush");
        }

        // ─── Maximizar respetando la barra de tareas ──────────────────────

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource.FromHwnd(new WindowInteropHelper(this).Handle)
                      ?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0024) // WM_GETMINMAXINFO
            {
                CorregirMaximizado(hwnd, lParam);
                handled = true;
            }
            return IntPtr.Zero;
        }

        private static void CorregirMaximizado(IntPtr hwnd, IntPtr lParam)
        {
            var mmi = (MinMaxInfo)Marshal.PtrToStructure(lParam, typeof(MinMaxInfo));

            var hMonitor = MonitorFromWindow(hwnd, 2 /* MONITOR_DEFAULTTONEAREST */);
            var mi = new MonitorInfo { cbSize = Marshal.SizeOf(typeof(MonitorInfo)) };
            GetMonitorInfo(hMonitor, ref mi);

            // ptMaxPosition es relativo a la esquina superior izquierda del monitor
            mmi.ptMaxPosition.x = Math.Abs(mi.rcWork.Left   - mi.rcMonitor.Left);
            mmi.ptMaxPosition.y = Math.Abs(mi.rcWork.Top    - mi.rcMonitor.Top);
            mmi.ptMaxSize.x     = Math.Abs(mi.rcWork.Right  - mi.rcWork.Left);
            mmi.ptMaxSize.y     = Math.Abs(mi.rcWork.Bottom - mi.rcWork.Top);

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        // ─── Chrome handlers ─────────────────────────────────────────────

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

        // ─── P/Invoke ────────────────────────────────────────────────────

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        [StructLayout(LayoutKind.Sequential)]
        private struct WinPoint { public int x; public int y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct WinRect { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MinMaxInfo
        {
            public WinPoint ptReserved;
            public WinPoint ptMaxSize;
            public WinPoint ptMaxPosition;
            public WinPoint ptMinTrackSize;
            public WinPoint ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MonitorInfo
        {
            public int    cbSize;
            public WinRect rcMonitor;
            public WinRect rcWork;
            public uint   dwFlags;
        }
    }
}
