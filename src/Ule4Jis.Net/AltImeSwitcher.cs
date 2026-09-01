using System;
using System.Runtime.InteropServices;

namespace Ule4Jis.Net
{
    public enum CapsLockMode
    {
        Disabled,   // 通常の CapsLock として動作
        ImeToggle   // CapsLock 単体押しで IME トグル切り替え
    }

    public static class AltImeSwitcher
    {
        private static bool _leftAltDown = false;
        private static bool _leftAltCombo = false;

        private static bool _rightAltDown = false;
        private static bool _rightAltCombo = false;

        private static bool _capsDown = false;
        private static bool _capsCombo = false;

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
                    // 左AltのKeyDownをOSに渡さない（フック消費）。
                    // これによりOSのメニューバー起動や未確定文字の自動確定を完全に阻止！
                    return true;
                }
                else if (isRightAlt)
                {
                    _rightAltDown = true;
                    _rightAltCombo = false;
                    // 右AltのKeyDownもフック消費
                    return true;
                }
                else if (isCapsLock && CurrentCapsLockMode == CapsLockMode.ImeToggle)
                {
                    _capsDown = true;
                    _capsCombo = false;
                    return true;
                }
                else if (!IsModifierKey(vkCode))
                {
                    // 通常キーが押された場合、Altコンボ（Alt+Tabなど）が発生したと判定
                    if (_leftAltDown)
                    {
                        if (!_leftAltCombo)
                        {
                            _leftAltCombo = true;
                            // ショートカットキーのために抑止していた Alt Down を遅延送信
                            NativeMethods.EmulateKey(NativeMethods.VK_LMENU, up: false);
                        }
                    }
                    if (_rightAltDown)
                    {
                        if (!_rightAltCombo)
                        {
                            _rightAltCombo = true;
                            NativeMethods.EmulateKey(NativeMethods.VK_RMENU, up: false);
                        }
                    }
                    if (_capsDown) _capsCombo = true;
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
                        // 左Altの単体空打ち
                        // IMEがONで、かつ入力中の未確定文字列がある場合（またはIME ON時）
                        if (GetImeStatus() && HasCompositionString())
                        {
                            // 未確定文字を入力中 -> 無変換キー (VK_NONCONVERT = カタカナ変換) 送信！
                            NativeMethods.EmulateKey(NativeMethods.VK_NONCONVERT, up: false);
                            NativeMethods.EmulateKey(NativeMethods.VK_NONCONVERT, up: true);
                        }
                        else
                        {
                            // 未確定文字がない状態 -> IME OFF (英数)
                            SetImeStatus(false);
                        }
                    }
                    else if (wasCombo)
                    {
                        // コンボ入力だった場合は遅延していた Alt Up を送信
                        NativeMethods.EmulateKey(NativeMethods.VK_LMENU, up: true);
                    }

                    return true; // 左Alt Upイベントをフック消費
                }
                else if (isRightAlt)
                {
                    bool wasDown = _rightAltDown;
                    bool wasCombo = _rightAltCombo;
                    _rightAltDown = false;
                    _rightAltCombo = false;

                    if (wasDown && !wasCombo)
                    {
                        // 右Altの単体空打ち -> IME ON (かな)
                        SetImeStatus(true);
                    }
                    else if (wasCombo)
                    {
                        NativeMethods.EmulateKey(NativeMethods.VK_RMENU, up: true);
                    }

                    return true; // 右Alt Upイベントをフック消費
                }
                else if (isCapsLock && CurrentCapsLockMode == CapsLockMode.ImeToggle)
                {
                    bool shouldToggle = _capsDown && !_capsCombo;
                    _capsDown = false;
                    _capsCombo = false;

                    if (shouldToggle)
                    {
                        ToggleImeStatus();
                        return true;
                    }
                }
            }

            if (isCapsLock && CurrentCapsLockMode == CapsLockMode.ImeToggle)
            {
                return true;
            }

            return false;
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

        /// <summary>
        /// 現在アクティブな入力フォーカスで未確定文字列（IME Composition String）が存在するかどうか判定する
        /// </summary>
        public static bool HasCompositionString()
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

            IntPtr hIMC = NativeMethods.ImmGetContext(targetWnd);
            if (hIMC != IntPtr.Zero)
            {
                try
                {
                    int len = NativeMethods.ImmGetCompositionString(hIMC, NativeMethods.GCS_COMPSTR, null, 0);
                    return len > 0;
                }
                catch
                {
                    // 無視
                }
                finally
                {
                    NativeMethods.ImmReleaseContext(targetWnd, hIMC);
                }
            }

            return false;
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

            // 2. メッセージ送信後も状態が一致しない場合の補填
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
