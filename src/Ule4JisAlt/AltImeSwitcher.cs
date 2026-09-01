using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ule4Jis.Net
{
    public enum CapsLockMode
    {
        Disabled,   // 通常の CapsLock として動作
        ImeToggle   // CapsLock 短押しで IME トグル、長押しで本来の CapsLock
    }

    /// <summary>
    /// 低レベルキーボードフックからのイベントを処理し、Alt単押しでのIME切替およびタイマー駆動のCapsLock長押し/短押し判定を行う。
    /// 業界標準の Tap-Hold (Dual-Role Key) パターンを採用。
    /// </summary>
    public static class AltImeSwitcher
    {
        private static bool _leftAltDown = false;
        private static bool _leftAltCombo = false;

        private static bool _rightAltDown = false;
        private static bool _rightAltCombo = false;

        // CapsLock の Tap-Hold 管理変数
        private static bool _capsDown = false;
        private static bool _capsLongPressFired = false;
        private static System.Threading.Timer? _capsTimer = null;
        private const int CapsLongPressDelayMs = 300; // 300ms で長押し判定

        public static CapsLockMode CurrentCapsLockMode { get; set; } = CapsLockMode.ImeToggle;

        public static bool ProcessKeyEvent(uint vkCode, uint flags, int msg)
        {
            bool isDown = (msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN);
            bool isUp = (msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP);

            bool isExtended = (flags & NativeMethods.KEYEVENTF_EXTENDEDKEY) != 0 || (flags & 1) != 0;
            bool isLeftAlt = (vkCode == NativeMethods.VK_LMENU) || (vkCode == NativeMethods.VK_MENU && !isExtended);
            bool isRightAlt = (vkCode == NativeMethods.VK_RMENU) || (vkCode == NativeMethods.VK_MENU && isExtended);
            bool isCapsLock = (vkCode == NativeMethods.VK_CAPITAL);

            if (isDown)
            {
                if (isLeftAlt)
                {
                    _leftAltDown = true;
                    _leftAltCombo = false;
                    return true; // 左Alt KeyDown をフック消費
                }
                else if (isRightAlt)
                {
                    _rightAltDown = true;
                    _rightAltCombo = false;
                    return true; // 右Alt KeyDown をフック消費
                }
                else if (isCapsLock && CurrentCapsLockMode == CapsLockMode.ImeToggle)
                {
                    if (!_capsDown)
                    {
                        _capsDown = true;
                        _capsLongPressFired = false;

                        // 300ms タイマーを開始。キーが離される前にタイマーが発火すれば「長押し」と判定
                        _capsTimer?.Dispose();
                        _capsTimer = new System.Threading.Timer(OnCapsLockLongPressTimer, null, CapsLongPressDelayMs, Timeout.Infinite);
                    }
                    return true; // CapsLock KeyDown をフック消費
                }
                else if (!IsModifierKey(vkCode))
                {
                    // 他キーが押された場合コンボ判定
                    if (_leftAltDown && !_leftAltCombo)
                    {
                        _leftAltCombo = true;
                        NativeMethods.EmulateKey(NativeMethods.VK_LMENU, up: false);
                    }
                    if (_rightAltDown && !_rightAltCombo)
                    {
                        _rightAltCombo = true;
                        NativeMethods.EmulateKey(NativeMethods.VK_RMENU, up: false);
                    }
                    if (_capsDown)
                    {
                        // CapsLock を押しながら他キーが押された場合も長押し扱いにしてタイマー即時発火
                        TriggerCapsLockLongPress();
                    }
                }
            }
            else if (isUp)
            {
                if (isLeftAlt)
                {
                    bool wasDown = _leftAltDown;
                    bool wasCombo = _leftAltCombo;
                    _leftAltDown = false;
                    _leftAltCombo = false;

                    if (wasDown && !wasCombo)
                    {
                        // 左Alt空打ち -> 無変換キー (VK_NONCONVERT = 0x1D)
                        NativeMethods.EmulateKey(NativeMethods.VK_NONCONVERT, up: false);
                        NativeMethods.EmulateKey(NativeMethods.VK_NONCONVERT, up: true);
                    }
                    else if (wasCombo)
                    {
                        NativeMethods.EmulateKey(NativeMethods.VK_LMENU, up: true);
                    }

                    return true;
                }
                else if (isRightAlt)
                {
                    bool wasDown = _rightAltDown;
                    bool wasCombo = _rightAltCombo;
                    _rightAltDown = false;
                    _rightAltCombo = false;

                    if (wasDown && !wasCombo)
                    {
                        // 右Alt空打ち -> IME ON
                        SetImeStatus(true);
                    }
                    else if (wasCombo)
                    {
                        NativeMethods.EmulateKey(NativeMethods.VK_RMENU, up: true);
                    }

                    return true;
                }
                else if (isCapsLock && CurrentCapsLockMode == CapsLockMode.ImeToggle)
                {
                    _capsTimer?.Dispose();
                    _capsTimer = null;

                    bool wasDown = _capsDown;
                    bool alreadyFired = _capsLongPressFired;
                    _capsDown = false;

                    if (wasDown && !alreadyFired)
                    {
                        // タイマー発火前にキーが離された -> 「短押し (Tap)」: IME トグル切り替え
                        ToggleImeStatus();
                    }

                    return true;
                }
            }

            if (isCapsLock && CurrentCapsLockMode == CapsLockMode.ImeToggle)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// CapsLock 長押しタイマー発火コールバック
        /// </summary>
        private static void OnCapsLockLongPressTimer(object? state)
        {
            TriggerCapsLockLongPress();
        }

        /// <summary>
        /// CapsLock の長押し（本来の CapsLock トグル機能）を発動する
        /// </summary>
        private static void TriggerCapsLockLongPress()
        {
            if (_capsDown && !_capsLongPressFired)
            {
                _capsLongPressFired = true;
                _capsTimer?.Dispose();
                _capsTimer = null;

                // フックの同期処理から脱出し、非同期で確実に CapsLock をトグル
                Task.Run(() =>
                {
                    try
                    {
                        SendKeys.SendWait("{CAPSLOCK}");
                    }
                    catch
                    {
                        NativeMethods.ToggleCapsLockHardware();
                    }
                });
            }
        }

        private static bool IsModifierKey(uint vkCode)
        {
            return vkCode == NativeMethods.VK_SHIFT ||
                   vkCode == NativeMethods.VK_LSHIFT ||
                   vkCode == NativeMethods.VK_RSHIFT ||
                   vkCode == NativeMethods.VK_CONTROL ||
                   vkCode == NativeMethods.VK_LCONTROL ||
                   vkCode == NativeMethods.VK_RCONTROL ||
                   vkCode == NativeMethods.VK_MENU ||
                   vkCode == NativeMethods.VK_LMENU ||
                   vkCode == NativeMethods.VK_RMENU ||
                   vkCode == NativeMethods.VK_CAPITAL ||
                   vkCode == 0x5B || // Left Windows Key
                   vkCode == 0x5C;   // Right Windows Key
        }

        public static bool GetImeStatus()
        {
            IntPtr fgWnd = NativeMethods.GetForegroundWindow();
            if (fgWnd == IntPtr.Zero) return false;

            uint threadId = NativeMethods.GetWindowThreadProcessId(fgWnd, out _);
            IntPtr targetWnd = fgWnd;

            NativeMethods.GUITHREADINFO gti = new NativeMethods.GUITHREADINFO();
            gti.cbSize = Marshal.SizeOf(typeof(NativeMethods.GUITHREADINFO));
            if (NativeMethods.GetGUIThreadInfo(threadId, ref gti) && gti.hwndFocus != IntPtr.Zero)
            {
                targetWnd = gti.hwndFocus;
            }

            IntPtr imeWnd = NativeMethods.ImmGetDefaultIMEWnd(targetWnd);
            if (imeWnd != IntPtr.Zero)
            {
                IntPtr res = NativeMethods.SendMessage(imeWnd, NativeMethods.WM_IME_CONTROL, (IntPtr)NativeMethods.IMC_GETOPENSTATUS, IntPtr.Zero);
                return res != IntPtr.Zero;
            }

            return false;
        }

        public static void ToggleImeStatus()
        {
            bool currentStatus = GetImeStatus();
            SetImeStatus(!currentStatus);
        }

        public static void SetImeStatus(bool enable)
        {
            // 1. WM_IME_CONTROL メッセージによる確実なIME切り替え
            IntPtr fgWnd = NativeMethods.GetForegroundWindow();
            if (fgWnd != IntPtr.Zero)
            {
                uint threadId = NativeMethods.GetWindowThreadProcessId(fgWnd, out _);
                IntPtr targetWnd = fgWnd;

                NativeMethods.GUITHREADINFO gti = new NativeMethods.GUITHREADINFO();
                gti.cbSize = Marshal.SizeOf(typeof(NativeMethods.GUITHREADINFO));
                if (NativeMethods.GetGUIThreadInfo(threadId, ref gti) && gti.hwndFocus != IntPtr.Zero)
                {
                    targetWnd = gti.hwndFocus;
                }

                IntPtr imeWnd = NativeMethods.ImmGetDefaultIMEWnd(targetWnd);
                if (imeWnd != IntPtr.Zero)
                {
                    NativeMethods.SendMessage(imeWnd, NativeMethods.WM_IME_CONTROL, (IntPtr)NativeMethods.IMC_SETOPENSTATUS, (IntPtr)(enable ? 1 : 0));
                }
            }

            // 2. メッセージ送信後も状態が一致しない場合のキー送信補填
            bool currentStatus = GetImeStatus();
            if (enable && !currentStatus)
            {
                NativeMethods.EmulateKey(NativeMethods.VK_IME_ON, up: false);
                NativeMethods.EmulateKey(NativeMethods.VK_IME_ON, up: true);

                if (!GetImeStatus())
                {
                    NativeMethods.EmulateKey(NativeMethods.VK_KANJI, up: false);
                    NativeMethods.EmulateKey(NativeMethods.VK_KANJI, up: true);
                }
            }
            else if (!enable && currentStatus)
            {
                NativeMethods.EmulateKey(NativeMethods.VK_IME_OFF, up: false);
                NativeMethods.EmulateKey(NativeMethods.VK_IME_OFF, up: true);

                if (GetImeStatus())
                {
                    NativeMethods.EmulateKey(NativeMethods.VK_KANJI, up: false);
                    NativeMethods.EmulateKey(NativeMethods.VK_KANJI, up: true);
                }
            }
        }
    }
}
