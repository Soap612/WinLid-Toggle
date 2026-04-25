using System;
using System.Runtime.InteropServices;
public class Test {
    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    public static void Main() {
        IntPtr hwnd = FindWindow(null, "Lid Behavior Controller v1.4");
        Console.WriteLine("Found: " + hwnd);
    }
}
