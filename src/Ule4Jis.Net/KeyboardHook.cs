using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ule4Jis.Net
{
    public static class KeyboardHook
    {
        private static NativeMethods.HookProc? _hookDelegate;
        private static IntPtr _hookID = IntPtr.Zero;

        public static bool EmulationEnabled { get; set; } = true;
        public static bool AltImeEnabled { get; set; } = true;

        public static void Start()
        {
            if (_hookID != IntPtr.Zero) return;

            _hookDelegate = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule? curModule = curProcess.MainModule)
            {
                IntPtr hMod = NativeMethods.GetModuleHandle(curModule?.ModuleName);
                _hookID = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, _hookDelegate, hMod, 0);
            }
        }

        public static void Stop()
        {
            if (_hookID != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
                _hookDelegate = null;
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = (int)wParam;
                NativeMethods.KBDLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);

                // 自身がSendInputで送ったキーイベント（dwExtraInfo == 0x554C4534）はそのまま通す
                if (hookStruct.dwExtraInfo == (IntPtr)0x554C4534)
                {
                    return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
                }

                uint vkCode = hookStruct.vkCode;
                bool isDown = (msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN);

                // 1. Alt-IME 切り替え処理
                if (AltImeEnabled)
                {
                    AltImeSwitcher.ProcessKeyEvent(vkCode, msg);
                }

                // 2. ULE4JIS US配列マッピング処理
                if (EmulationEnabled)
                {
                    bool isShift = NativeMethods.IsShiftPressed();
                    if (UsOnJisMapper.TryMapKey(vkCode, isShift, out var result) && result != null)
                    {
                        // 元の入力キーを打ち消して、エミュレートキーを送信
                        UsOnJisMapper.SendEmulatedKey(result.TargetVkCode, result.ShiftAction, isDown);
                        return (IntPtr)1; // 元のイベントを消費
                    }
                }
            }

            return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
