using System.Text;
using System.Runtime.InteropServices;

namespace WinFocusLogger;

public static class Program
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    [DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool
        GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, int nSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    private const uint ProcessQueryInformation = 0x400;
    private const uint ProcessVmRead = 0x10;

    public static void Main()
    {
        uint lastActiveProcId = 0;

        Console.OutputEncoding = Encoding.Unicode;

        while (true)
        {
            Thread.Sleep(100);
            uint activeProcId = 0;
            var program = new StringBuilder(260);

            var hWnd = GetForegroundWindow();
            if (hWnd != IntPtr.Zero)
            {
                GetWindowThreadProcessId(hWnd, out activeProcId);

                if (activeProcId == 0)
                {
                    Console.WriteLine("GetWindowThreadProcessId had error " + Marshal.GetLastWin32Error());
                    continue;
                }

                var hProc = OpenProcess(ProcessQueryInformation | ProcessVmRead, false, activeProcId);
                if (hProc == IntPtr.Zero)
                {
                    Console.WriteLine("OpenProcess had error " + Marshal.GetLastWin32Error());
                    continue;
                }

                var rc = GetModuleFileNameEx(hProc, IntPtr.Zero, program, program.Capacity);
                if (rc == false)
                {
                    Console.WriteLine("GetModuleFileNameEx had error " + Marshal.GetLastWin32Error());
                    CloseHandle(hProc);
                    continue;
                }

                CloseHandle(hProc);
            }

            if (activeProcId == lastActiveProcId) continue;
            var date = DateTime.Now.ToString("dd MMM HH:mm:ss");
            Console.WriteLine(activeProcId == 0
                ? $"{date} \n No foreground application"
                : $"PID:{activeProcId}\t{date} \n {program}");

            lastActiveProcId = activeProcId;
        }
    }
}