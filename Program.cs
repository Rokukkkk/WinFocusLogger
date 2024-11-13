using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

        Console.WriteLine("{0,-25} {1,-10} {2,-20} {3,-0}\n", "Name", "PID", "Date", "Location");
        while (true)
        {
            Thread.Sleep(100);
            uint activeProcId = 0;
            var program = string.Empty;
            var programName = string.Empty;

            var hWnd = GetForegroundWindow();
            if (hWnd != IntPtr.Zero)
            {
                if (GetWindowThreadProcessId(hWnd, out activeProcId) == 0) continue;

                if (activeProcId == 0)
                {
                    Console.WriteLine("GetWindowThreadProcessId had error " + Marshal.GetLastWin32Error());
                    continue;
                }

                Process hProc;
                try
                {
                    hProc = Process.GetProcessById((int)activeProcId);
                }
                catch (Win32Exception e)
                {
                    Console.WriteLine("GetProcessByID had error " + e.NativeErrorCode);
                    continue;
                }

                try
                {
                    if (hProc.MainModule != null) program = hProc.MainModule.FileName;
                }
                catch (Win32Exception e)
                {
                    Console.WriteLine("MainModule.FileName had error " + e.NativeErrorCode);
                    continue;
                }

                programName = hProc.ProcessName;
            }

            if (activeProcId == lastActiveProcId || activeProcId == 0) continue;
            var date = DateTime.Now.ToString("yy-MMM-dd HH:mm:ss");
            Console.WriteLine("{0,-25} {1,-10} {2,-20} {3,-0}", programName, activeProcId, date, program);

            lastActiveProcId = activeProcId;
        }
    }
}