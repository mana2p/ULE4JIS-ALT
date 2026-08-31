using System;
using System.Runtime.InteropServices;

namespace Ule4Jis.Net
{
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

            bool isExtended = (flags & 1) != 0;
            bool isLeftAlt = (vkCode == NativeMethods.VK_LMENU) || (vkCode == NativeMethods.VK_MENU && !isExtended);
            bool isRightAlt = (vkCode == NativeMethods.VK_RMENU) || (vkCode == NativeMethods.VK_MENU && isExtended);

            if (isDown)
            {
                if (isLeftAlt)
                {
                    _leftAltDown = true;
                    _leftAltCombo = false;
                }
                else if (isRightAlt)
                {
                    _rightAltDown = true;
                    _rightAltCombo = false;
                }
                else if (!IsModifierKey(vkCode))
                {
                    // 修飾キー以外の通常キーが押された場合のみ、コンボと判定
                    if (_leftAltDown) _leftAltCombo = true;
                    if (_rightAltDown) _rightAltCombo = true;
                }
            }
            else if (isUp)
            {
                if (isLeftAlt)
                {
                    bool shouldToggle = _leftAltDown && !_leftAltCombo;
                    _leftAltDown = false;
                    _leftAltCombo = false;

                    if (shouldToggle)
                    {
                        // 左Alt空打ち -> IME OFF (英数)
                        ToggleIme(false);
                    }
                }
                else if (isRightAlt)
                {
                    bool shouldToggle = _rightAltDown && !_rightAltCombo;
                    _rightAltDown = false;
                    _rightAltCombo = false;

                    if (shouldToggle)
                    {
                        // 右Alt空打ち -> IME ON (かな)
                        ToggleIme(true);
                    }
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
                   vkCode == 0x5B || // Left Windows Key
                   vkCode == 0x5C;   // Right Windows Key
        }

        private static void ToggleIme(bool enable)
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

            // 2. VK_IME_ON / VK_IME_OFF キー送信 (バックアップ)
            byte vk = enable ? NativeMethods.VK_IME_ON : NativeMethods.VK_IME_OFF;
            NativeMethods.SendKey(vk, true);
            NativeMethods.SendKey(vk, false);
        }
    }
}
