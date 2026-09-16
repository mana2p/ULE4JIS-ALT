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

        private static bool _henkanSpaceDown = false;
        private static int _composingCharCount = 0;
        private static IntPtr _lastActiveWindow = IntPtr.Zero;

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

                // アクティブウィンドウが変わったら未確定文字カウントをリセット
                IntPtr fgWnd = NativeMethods.GetForegroundWindow();
                if (fgWnd != _lastActiveWindow)
                {
                    _lastActiveWindow = fgWnd;
                    _composingCharCount = 0;
                }

                // IME未確定入力状態の追跡 (Enter/Esc/Tabで確定・キャンセル、Backspaceで文字数減退)
                if (!isUp)
                {
                    if (ImeController.GetStatus())
                    {
                        if (vkCode == NativeMethods.VK_RETURN || vkCode == NativeMethods.VK_ESCAPE || vkCode == NativeMethods.VK_TAB)
                        {
                            _composingCharCount = 0;
                        }
                        else if (vkCode == NativeMethods.VK_BACK)
                        {
                            if (_composingCharCount > 0) _composingCharCount--;
                        }
                        else if (IsInputCharKey(vkCode))
                        {
                            _composingCharCount++;
                        }
                    }
                    else
                    {
                        _composingCharCount = 0;
                    }
                }

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
                        // 外付けJIS化モード時の特殊IMEキー処理
                        if (CurrentLayoutMode == LayoutMode.ExternalJis)
                        {
                            bool isShift = KeyEmulator.IsShiftPressed();

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

                            // 2. 変換キー (VK_CONVERT または scanCode 0x79)
                            // IMEがOFFのときは「IME ON」にし、すでにIMEがONのときは「Space（未確定時は漢字変換）」として動作
                            if (vkCode == NativeMethods.VK_CONVERT || hookStruct.scanCode == 0x79)
                            {
                                if (!isUp)
                                {
                                    if (!ImeController.GetStatus())
                                    {
                                        _henkanSpaceDown = false;
                                        ImeController.SetStatus(true);
                                    }
                                    else
                                    {
                                        _henkanSpaceDown = true;
                                        KeyEmulator.EmulateKey(NativeMethods.VK_SPACE, up: false);
                                    }
                                }
                                else
                                {
                                    if (_henkanSpaceDown)
                                    {
                                        KeyEmulator.EmulateKey(NativeMethods.VK_SPACE, up: true);
                                        _henkanSpaceDown = false;
                                    }
                                }
                                return (IntPtr)1; // イベントを消費
                            }

                            // 3. 無変換キー (VK_NONCONVERT または scanCode 0x7B)
                            // IMEで変換中（未確定文字あり）のときはカタカナ変換 (F7)、変換していない時はIME OFF
                            if (vkCode == NativeMethods.VK_NONCONVERT || hookStruct.scanCode == 0x7B)
                            {
                                if (!isUp)
                                {
                                    bool isImeOn = ImeController.GetStatus();
                                    if (isImeOn && _composingCharCount > 0)
                                    {
                                        // 日本語変換中（未確定文字あり）: カタカナ変換 (F7)
                                        KeyEmulator.EmulateKey(NativeMethods.VK_F7, up: false);
                                        KeyEmulator.EmulateKey(NativeMethods.VK_F7, up: true);
                                    }
                                    else
                                    {
                                        // 変換していない時: IME OFF
                                        ImeController.SetStatus(false);
                                        _composingCharCount = 0;
                                    }
                                }
                                return (IntPtr)1; // イベントを消費
                            }

                            // 4. ひらがな/カタカナキー (VK_KANA または scanCode 0x70) -> IME ON (ひらがな)
                            if (vkCode == NativeMethods.VK_KANA || hookStruct.scanCode == 0x70)
                            {
                                if (!isUp)
                                {
                                    ImeController.SetStatus(true);
                                }
                                return (IntPtr)1; // イベントを消費
                            }
                        }

                        bool isShiftPressed = KeyEmulator.IsShiftPressed();
                        if (KeyMapper.TryMapKey(CurrentLayoutMode, vkCode, isShiftPressed, out var result))
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

        private static bool IsInputCharKey(uint vkCode)
        {
            // A-Z, 0-9
            if ((vkCode >= 'A' && vkCode <= 'Z') || (vkCode >= '0' && vkCode <= '9')) return true;

            // Numpad
            if (vkCode >= 0x60 && vkCode <= 0x6F) return true;

            // OEM 記号キー (JIS/US 記号キー群)
            if (vkCode >= NativeMethods.VK_OEM_1 && vkCode <= NativeMethods.VK_OEM_102) return true;

            // Spaceキー
            if (vkCode == NativeMethods.VK_SPACE) return true;

            return false;
        }
    }
}
