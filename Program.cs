using System;
using System.Windows.Forms;
using System.Threading;

namespace LidController
{
    static class Program
    {
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
                MessageBox.Show("Lid Controller is already running. Check your system tray.", "Lid Controller", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
