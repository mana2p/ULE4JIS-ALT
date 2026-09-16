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
        public static LayoutMode CurrentLayoutMode { get; set; } = LayoutMode.ExternalUs;
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

                // 1. 左右 Alt 空打ち IME 切り替え処理
                if (AltImeEnabled)
                {
                    bool handled = AltImeSwitcher.ProcessKeyEvent(vkCode, hookStruct.flags, msg);
                    if (handled)
                    {
                        return (IntPtr)1; // イベントを消費
                    }
                }

                // 2. キーボード配列マッピング処理 (ULE4JIS / ULE4US)
                if (EmulationEnabled)
                {
                    bool shouldEmulate = !RawInputReceiver.AutoDetectionEnabled || RawInputReceiver.IsLastInputFromExternal;

                    if (shouldEmulate)
                    {
                        bool isShift = KeyEmulator.IsShiftPressed();

                        // 外付けJIS化モード時の特殊IMEキー処理
                        if (CurrentLayoutMode == LayoutMode.ExternalJis)
                        {
                            // 1. 半角/全角キー (US配列設定のOSでは ` (VK_OEM_3) と誤認されるため、IMEトグルに変換)
                            if (!isShift && vkCode == NativeMethods.VK_OEM_3)
                            {
                                if (!isUp) // KeyDown 時にトグル
                                {
                                    bool currentStatus = ImeController.GetStatus();
                                    ImeController.SetStatus(!currentStatus);
                                }
                                return (IntPtr)1; // ` 文字入力を防ぐため消費
                            }

                            // 2. 変換キー / ひらがなキー -> 確実に IME ON (かな)
                            if (vkCode == NativeMethods.VK_CONVERT || hookStruct.scanCode == 0x79 ||
                                vkCode == NativeMethods.VK_KANA || hookStruct.scanCode == 0x70)
                            {
                                if (!isUp)
                                {
                                    ImeController.SetStatus(true);
                                }
                                return (IntPtr)1; // イベントを消費
                            }

                            // 3. 無変換キー (VK_NONCONVERT または scanCode 0x7B) -> 確実に IME OFF (英数)
                            if (vkCode == NativeMethods.VK_NONCONVERT || hookStruct.scanCode == 0x7B)
                            {
                                if (!isUp)
                                {
                                    ImeController.SetStatus(false);
                                }
                                return (IntPtr)1; // イベントを消費
                            }
                        }

                        if (KeyMapper.TryMapKey(CurrentLayoutMode, vkCode, isShift, out var result))
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
