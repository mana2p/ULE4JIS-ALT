using System;

namespace Ule4Jis.Net
{
    public static class AltImeSwitcher
    {
        private static bool _leftAltDown = false;
        private static bool _leftAltCombo = false;

        private static bool _rightAltDown = false;
        private static bool _rightAltCombo = false;

        public static bool ProcessKeyEvent(uint vkCode, int msg)
        {
            bool isDown = (msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN);
            bool isUp = (msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP);

            if (isDown)
            {
                if (vkCode == NativeMethods.VK_LMENU)
                {
                    _leftAltDown = true;
                    _leftAltCombo = false;
                }
                else if (vkCode == NativeMethods.VK_RMENU)
                {
                    _rightAltDown = true;
                    _rightAltCombo = false;
                }
                else
                {
                    // Altキー以外のキーが押された場合、組合せ操作と判定
                    if (_leftAltDown) _leftAltCombo = true;
                    if (_rightAltDown) _rightAltCombo = true;
                }
            }
            else if (isUp)
            {
                if (vkCode == NativeMethods.VK_LMENU)
                {
                    bool shouldToggle = _leftAltDown && !_leftAltCombo;
                    _leftAltDown = false;
                    _leftAltCombo = false;

                    if (shouldToggle)
                    {
                        // 左Alt空打ち -> IME OFF
                        ToggleIme(false);
                    }
                }
                else if (vkCode == NativeMethods.VK_RMENU)
                {
                    bool shouldToggle = _rightAltDown && !_rightAltCombo;
                    _rightAltDown = false;
                    _rightAltCombo = false;

                    if (shouldToggle)
                    {
                        // 右Alt空打ち -> IME ON
                        ToggleIme(true);
                    }
                }
            }

            return false;
        }

        private static void ToggleIme(bool enable)
        {
            if (enable)
            {
                // IME ON (かな)
                NativeMethods.SendKey(NativeMethods.VK_IME_ON, true);
                NativeMethods.SendKey(NativeMethods.VK_IME_ON, false);
            }
            else
            {
                // IME OFF (英数)
                NativeMethods.SendKey(NativeMethods.VK_IME_OFF, true);
                NativeMethods.SendKey(NativeMethods.VK_IME_OFF, false);
            }
        }
    }
}
