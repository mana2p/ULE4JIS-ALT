using System;
using System.Runtime.InteropServices;

namespace Ule4Jis.Net
{
    /// <summary>
    /// 左右 Alt キーの空打ちによるスマートな IME 切り替え制御（macOS風）
    /// - 左 Alt 単体空打ち: VK_NONCONVERT (無変換キー) 送信 -> IME OFF / カタカナ変換
    /// - 右 Alt 単体空打ち: IME ON (かな) 送信
    /// - Alt + 他キーの組み合わせ時: 通常の Alt ショートカットとして通過
    /// </summary>
    public static class AltImeSwitcher
    {
        private static bool _leftAltDown = false;
        private static bool _leftAltCombo = false;

        private static bool _rightAltDown = false;
        private static bool _rightAltCombo = false;

        public static bool ProcessKeyEvent(uint vkCode, uint flags, int msg)
        {
            bool isDown = (msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN);
            bool isUp = (msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP);

            bool isExtended = (flags & NativeMethods.KEYEVENTF_EXTENDEDKEY) != 0 || (flags & 1) != 0;
            bool isLeftAlt = (vkCode == NativeMethods.VK_LMENU) || (vkCode == NativeMethods.VK_MENU && !isExtended);
            bool isRightAlt = (vkCode == NativeMethods.VK_RMENU) || (vkCode == NativeMethods.VK_MENU && isExtended);

            if (isDown)
            {
                if (isLeftAlt)
                {
                    _leftAltDown = true;
                    _leftAltCombo = false;
                    return true; // 左Alt KeyDown をフック消費（メニューバー起動防止）
                }
                else if (isRightAlt)
                {
                    _rightAltDown = true;
                    _rightAltCombo = false;
                    return true; // 右Alt KeyDown をフック消費
                }
                else if (!IsModifierKey(vkCode))
                {
                    // 通常キーが押された場合、Altコンボ（Alt+Tab等）が発生したと判定
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
                        // 左Alt単体空打ち -> 無変換キー (VK_NONCONVERT = 0x1D)
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
                        // 右Alt単体空打ち -> IME ON (かな)
                        SetImeStatus(true);
                    }
                    else if (wasCombo)
                    {
                        NativeMethods.EmulateKey(NativeMethods.VK_RMENU, up: true);
                    }

                    return true;
                }
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
