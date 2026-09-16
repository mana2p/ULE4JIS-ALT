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
        public static LayoutMode CurrentLayoutMode { get; set; } = LayoutMode.InternalJisExternalUs;
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

                // メッセージキューに溜まっている WM_INPUT を同期的にドレインして最新デバイス情報を確定する
                RawInputReceiver.DrainPendingRawInputMessages();

                uint vkCode = hookStruct.vkCode;
                bool isUp = (msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP);

                // 現在打鍵されているキーボードがUS配列かどうかを判定
                // - InternalJisExternalUs: 外付けキーボードからの入力がUS配列
                // - InternalUsExternalJis: 内蔵キーボードからの入力がUS配列
                bool isExternal = RawInputReceiver.IsLastInputFromExternal;
                bool isUsKeyboard = (CurrentLayoutMode == LayoutMode.InternalJisExternalUs) ? isExternal : !isExternal;
                bool shouldEmulate = !RawInputReceiver.AutoDetectionEnabled || isUsKeyboard;

                // US配列キーボード打鍵時のみエミュレーションを適用（JISキーボードは完全ネイティブスルー）
                if (shouldEmulate)
                {
                    // 1. 左右 Alt 空打ち IME 切り替え処理 (USキーボードのみ適用)
                    if (AltImeEnabled)
                    {
                        bool handled = AltImeSwitcher.ProcessKeyEvent(vkCode, hookStruct.flags, msg);
                        if (handled)
                        {
                            return (IntPtr)1; // イベントを消費
                        }
                    }

                    // 2. キーボード配列マッピング処理 (US on JIS)
                    if (EmulationEnabled)
                    {
                        bool isShift = KeyEmulator.IsShiftPressed();
                        if (KeyMapper.TryMapKey(vkCode, isShift, out var result))
                        {
                            if (result != null)
                            {
                                KeyMapper.SendEmulatedKey(result, isUp);
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
