using System;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace LidController
{
    static class Program
    {
        internal const uint WM_SHOWME = 0x0401;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // Mutex to ensure single instance
        static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");

        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
                mutex.ReleaseMutex();
            }
            else
            {
                IntPtr hWnd = FindWindow(null, "Lid Behavior Controller");
                if (hWnd != IntPtr.Zero)
                {
                    SendMessage(hWnd, WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
                }
            }
        }
    }
}
