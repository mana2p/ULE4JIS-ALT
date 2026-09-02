using System;
using System.Runtime.InteropServices;

namespace Ule4JisAlt
{
    internal static class NativeMethods
    {
        public const int WH_KEYBOARD_LL = 13;

        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;

        // Virtual Key Codes
        public const byte VK_CAPITAL = 0x14;   // Caps Lock
        public const byte VK_SHIFT = 0x10;
        public const byte VK_CONTROL = 0x11;
        public const byte VK_MENU = 0x12;      // Alt
        public const byte VK_LSHIFT = 0xA0;
        public const byte VK_RSHIFT = 0xA1;
        public const byte VK_LCONTROL = 0xA2;
        public const byte VK_RCONTROL = 0xA3;
        public const byte VK_LMENU = 0xA4;     // Left Alt
        public const byte VK_RMENU = 0xA5;     // Right Alt

        // IME Virtual Keys & Messages
        public const byte VK_IME_ON = 0x16;
        public const byte VK_IME_OFF = 0x1A;
        public const byte VK_KANJI = 0x19;     // 漢字 / 半角全角
        public const byte VK_NONCONVERT = 0x1D; // 無変換キー
        public const byte VK_CONVERT = 0x1C;    // 変換キー

        public const uint WM_IME_CONTROL = 0x0283;
        public const int IMC_GETOPENSTATUS = 0x0005;
        public const int IMC_SETOPENSTATUS = 0x0006;

        // OEM Virtual Keys for JIS / US Layout
        public const byte VK_OEM_1 = 0xBA;
        public const byte VK_OEM_PLUS = 0xBB;
        public const byte VK_OEM_COMMA = 0xBC;
        public const byte VK_OEM_MINUS = 0xBD;
        public const byte VK_OEM_PERIOD = 0xBE;
        public const byte VK_OEM_2 = 0xBF;
        public const byte VK_OEM_3 = 0xC0;
        public const byte VK_OEM_4 = 0xDB;
        public const byte VK_OEM_5 = 0xDC;
        public const byte VK_OEM_6 = 0xDD;
        public const byte VK_OEM_7 = 0xDE;
        public const byte VK_OEM_102 = 0xE2;
        public const byte VK_OEM_ENLW = 0xF3;
        public const byte VK_OEM_AUTO = 0xF4;

        // Windows Keys
        public const byte VK_LWIN = 0x5B;
        public const byte VK_RWIN = 0x5C;

        // Navigation / Editing Keys
        public const byte VK_PRIOR     = 0x21;  // Page Up
        public const byte VK_NEXT      = 0x22;  // Page Down
        public const byte VK_END       = 0x23;
        public const byte VK_HOME      = 0x24;
        public const byte VK_LEFT      = 0x25;
        public const byte VK_UP        = 0x26;
        public const byte VK_RIGHT     = 0x27;
        public const byte VK_DOWN      = 0x28;
        public const byte VK_PRINT     = 0x2C;
        public const byte VK_INSERT    = 0x2D;
        public const byte VK_DELETE    = 0x2E;
        public const byte VK_CANCEL    = 0x03;

        // Numpad Keys
        public const byte VK_NUMLOCK   = 0x90;
        public const byte VK_DIVIDE    = 0x6F;
        public const byte VK_SEPARATOR = 0x6C;

        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0002;

        public static readonly UIntPtr EmulatorMarker = new UIntPtr(0x554C4534); // "ULE4"

        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GUITHREADINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgti);

        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    }
}
