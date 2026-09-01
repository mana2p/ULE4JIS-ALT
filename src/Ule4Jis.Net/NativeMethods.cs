using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ule4Jis.Net
{
    internal static class NativeMethods
    {
        public const int WH_KEYBOARD_LL = 13;

        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;

        // Virtual Key Codes
        public const byte VK_CAPITAL = 0x14; // Caps Lock
        public const byte VK_SHIFT = 0x10;
        public const byte VK_CONTROL = 0x11;
        public const byte VK_MENU = 0x12;  // Alt
        public const byte VK_LSHIFT = 0xA0;
        public const byte VK_RSHIFT = 0xA1;
        public const byte VK_LCONTROL = 0xA2;
        public const byte VK_RCONTROL = 0xA3;
        public const byte VK_LMENU = 0xA4; // Left Alt
        public const byte VK_RMENU = 0xA5; // Right Alt

        // IME Virtual Keys & Messages
        public const byte VK_IME_ON = 0x16;
        public const byte VK_IME_OFF = 0x1A;
        public const byte VK_KANJI = 0x19;

        public const uint WM_IME_CONTROL = 0x0283;
        public const int IMC_GETOPENSTATUS = 0x0005;
        public const int IMC_SETOPENSTATUS = 0x0006;

        // OEM Virtual Keys for JIS / US Layout
        public const byte VK_OEM_1 = 0xBA;   // JIS: :*, US: ;:
        public const byte VK_OEM_PLUS = 0xBB; // JIS: ;+, US: =+
        public const byte VK_OEM_COMMA = 0xBC;
        public const byte VK_OEM_MINUS = 0xBD; // -_
        public const byte VK_OEM_PERIOD = 0xBE;
        public const byte VK_OEM_2 = 0xBF;   // /?
        public const byte VK_OEM_3 = 0xC0;   // JIS: @`, US: `~
        public const byte VK_OEM_4 = 0xDB;   // JIS: [{, US: [{
        public const byte VK_OEM_5 = 0xDC;   // JIS: \|, US: \|
        public const byte VK_OEM_6 = 0xDD;   // JIS: ]}, US: ]}
        public const byte VK_OEM_7 = 0xDE;   // JIS: ^~, US: '"
        public const byte VK_OEM_102 = 0xE2; // JIS: \_
        public const byte VK_OEM_ENLW = 0xF3; // 半角/全角
        public const byte VK_OEM_AUTO = 0xF4; // 半角/全角

        public const uint INPUT_KEYBOARD = 1;
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0200;

        public const IntPtr ExtraInfoMarker = (IntPtr)0x554C4534; // "ULE4" marker

        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
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

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUTUNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public INPUTUNION U;
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

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

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

        public static void SendKey(byte vkCode, bool isDown, bool isExtended = false)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].U.ki.wVk = vkCode;
            inputs[0].U.ki.wScan = 0;
            inputs[0].U.ki.dwFlags = (isDown ? 0u : KEYEVENTF_KEYUP) | (isExtended ? KEYEVENTF_EXTENDEDKEY : 0u);
            inputs[0].U.ki.time = 0;
            inputs[0].U.ki.dwExtraInfo = ExtraInfoMarker;

            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static bool IsShiftPressed()
        {
            return (GetKeyState(VK_SHIFT) & 0x8000) != 0 ||
                   (GetKeyState(VK_LSHIFT) & 0x8000) != 0 ||
                   (GetKeyState(VK_RSHIFT) & 0x8000) != 0;
        }

        public static bool IsCapsLockOn()
        {
            return (GetKeyState(VK_CAPITAL) & 0x0001) != 0;
        }

        public static void DisableCapsLockLed()
        {
            if (IsCapsLockOn())
            {
                SendKey(VK_CAPITAL, true);
                SendKey(VK_CAPITAL, false);
            }
        }

        public static void SendAtomicEmulatedKey(byte targetVkCode, ShiftAction shiftAction, bool isDown)
        {
            bool physShiftPressed = IsShiftPressed();
            List<INPUT> inputList = new List<INPUT>();

            // Shiftの状態調整が必要な場合、1つのSendInputバッファにまとめる
            if (shiftAction == ShiftAction.ReleaseShift && physShiftPressed)
            {
                INPUT shiftUp = new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = VK_LSHIFT,
                            dwFlags = KEYEVENTF_KEYUP,
                            dwExtraInfo = ExtraInfoMarker
                        }
                    }
                };
                inputList.Add(shiftUp);
            }
            else if (shiftAction == ShiftAction.PressShift && !physShiftPressed)
            {
                INPUT shiftDown = new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = VK_LSHIFT,
                            dwFlags = 0,
                            dwExtraInfo = ExtraInfoMarker
                        }
                    }
                };
                inputList.Add(shiftDown);
            }

            // ターゲットキー
            INPUT targetKey = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = targetVkCode,
                        dwFlags = isDown ? 0u : KEYEVENTF_KEYUP,
                        dwExtraInfo = ExtraInfoMarker
                    }
                }
            };
            inputList.Add(targetKey);

            // Shift状態の復元
            if (shiftAction == ShiftAction.ReleaseShift && physShiftPressed)
            {
                INPUT shiftRestoreDown = new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = VK_LSHIFT,
                            dwFlags = 0,
                            dwExtraInfo = ExtraInfoMarker
                        }
                    }
                };
                inputList.Add(shiftRestoreDown);
            }
            else if (shiftAction == ShiftAction.PressShift && !physShiftPressed)
            {
                INPUT shiftRestoreUp = new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = VK_LSHIFT,
                            dwFlags = KEYEVENTF_KEYUP,
                            dwExtraInfo = ExtraInfoMarker
                        }
                    }
                };
                inputList.Add(shiftRestoreUp);
            }

            INPUT[] inputs = inputList.ToArray();
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
