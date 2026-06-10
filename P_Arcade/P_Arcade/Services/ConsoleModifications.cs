using System;
using System.Runtime.InteropServices;

namespace P_Arcade.Services
{
    internal class ConsoleModifications
    {
        // Import the necessary functions from kernel32.dll and user32.dll
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        // Constants used for console-related modifications
        const int SW_MAXIMIZE = 3;

        // Virtual key codes for the zoom shortcuts
        const byte VK_CONTROL = 0x11;
        const byte VK_OEM_PLUS = 0xBB;
        const byte VK_OEM_MINUS = 0xBD;

        const uint KEYEVENTF_KEYUP = 0x0002;

        /// <summary>
        /// Structure used to get the screen's size
        /// </summary>
        struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary>
        /// Get the IntPtr Handle of the current window
        /// </summary>
        static IntPtr GetHandle()
        {
            SetForegroundWindow(GetConsoleWindow());
            return GetForegroundWindow();
        }

        /// <summary>
        /// Fullscreen the app using user32.dll and kernel32.dll methods
        /// </summary>
        public static void Fullscreen()
        {
            // Push the console window to the front, then immediately read it back
            // with GetForegroundWindow so we're guaranteed to have the right handle
            IntPtr consoleWindowHandle = GetHandle();

            // Maximize the console window
            ShowWindow(consoleWindowHandle, SW_MAXIMIZE);

            // Get the screen size
            GetWindowRect(consoleWindowHandle, out Rect screenRect);

            // Resize and reposition the console window to fill the screen
            int intWidth = screenRect.Right - screenRect.Left;
            int intHeight = screenRect.Bottom - screenRect.Top;

            MoveWindow(consoleWindowHandle, screenRect.Left, screenRect.Top, intWidth, intHeight, true);
        }

        /// <summary>
        /// Zooms the console text in or out by sending Ctrl+scroll events.
        /// </summary>
        /// <param name="intSteps">Positive zooms in, negative zooms out</param>
        public static void Zoom(int intSteps)
        {
            if (intSteps == 0)
                return;

            // Bring the console to the front so the key events land in the right window
            SetForegroundWindow(GetConsoleWindow());

            byte bytZoomKey = intSteps > 0 ? VK_OEM_PLUS : VK_OEM_MINUS;

            for (int i = 0; i < Math.Abs(intSteps); i++)
            {
                keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero); // Ctrl down
                keybd_event(bytZoomKey, 0, 0, UIntPtr.Zero); // +/- down
                keybd_event(bytZoomKey, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // +/- up
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Ctrl up
            }
        }
    }
}
