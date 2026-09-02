using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ule4JisAlt
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

                // 自分が keybd_event で送ったエミュレートイベント（dwExtraInfo == EmulatorMarker）はそのまま通す
                if (hookStruct.dwExtraInfo == NativeMethods.EmulatorMarker)
                {
                    return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
                }

                uint vkCode = hookStruct.vkCode;
                bool isUp = (msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP);

                // 1. 左右 Alt 空打ち IME 切り替え処理
                if (AltImeEnabled)
                {
                    bool handled = AltImeSwitcher.ProcessKeyEvent(vkCode, hookStruct.flags, msg);
                    if (handled)
                    {
                        return (IntPtr)1; // イベントを消費
                    }
                }

                // 2. ULE4JIS US配列マッピング処理
                if (EmulationEnabled)
                {
                    bool isShift = KeyEmulator.IsShiftPressed();
                    if (UsOnJisMapper.TryMapKey(vkCode, isShift, out var result))
                    {
                        if (RawInputReceiver.AutoDetectionEnabled)
                        {
                            // 自動判別有効時: WM_INPUT 到着までキーを保留（初回誤判別を完全防止）
                            KeyPendingManager.Enqueue(vkCode, isUp, isShift, result);
                            return (IntPtr)1; // イベントをフック消費
                        }
                        else if (RawInputReceiver.IsLastInputFromExternal || !RawInputReceiver.AutoDetectionEnabled)
                        {
                            // 自動判別無効（常にUSエミュレーション）の場合: 即座に送信
                            if (result != null)
                            {
                                UsOnJisMapper.SendEmulatedKey(result, isUp);
                            }
                            return (IntPtr)1; // イベントを消費
                        }
                    }
                }
            }

            return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
