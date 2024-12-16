using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WinFocusLogger;

public static partial class Program
{
    [LibraryImport("user32.dll")]
    private static partial IntPtr GetForegroundWindow();

    [LibraryImport("user32.dll")]
    private static partial uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    public static void Main()
    {
        uint lastActiveProcId = 0;
        uint activeProcId = 0;

        var program = new StringBuilder();
        var programName = new StringBuilder();
        var buffer = new StringBuilder();

        uint activeProcId = 0;
        var program = string.Empty;
        var programName = string.Empty;

        var buffer = new StringBuilder();

        Console.WriteLine("{0,-25} {1,-10} {2,-20} {3,-0}\n", "Name", "PID", "Date", "Location");
        while (true)
        {
            Thread.Sleep(100);

            if (GetForegroundWindow() != IntPtr.Zero)
            {
                if (GetWindowThreadProcessId(GetForegroundWindow(), out activeProcId) == 0) continue;

                if (activeProcId == 0)
                {
                    Console.WriteLine($"GetWindowThreadProcessId had error {Marshal.GetLastWin32Error()}");
                    continue;
                }

                Process hProc;
                try
                {
                    hProc = Process.GetProcessById((int)activeProcId);
                }
                catch (Win32Exception e)
                {
                    Console.WriteLine($"GetProcessByID had error {e.NativeErrorCode}");
                    continue;
                }

                try
                {
                    if (hProc.MainModule != null)
                    {
                        program.Clear();
                        program.Append(hProc.MainModule.FileName);
                    }
                }
                catch (Win32Exception e)
                {
                    Console.WriteLine($"MainModule.FileName had error {e.NativeErrorCode}");
                    continue;
                }

                if (hProc.MainModule != null)
                {
                    programName.Clear();
                    programName.Append(hProc.ProcessName);
                }
            }

            if (activeProcId == lastActiveProcId || activeProcId == 0) continue;
            buffer.Clear();
            buffer.Append($"{DateTime.Now:yy-MMM-dd HH:mm:ss}");
            Console.WriteLine("{0,-25} {1,-10} {2,-20} {3,-0}", programName, activeProcId.ToString(), buffer, program);

            lastActiveProcId = activeProcId;
        }
    }
}