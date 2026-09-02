using System;

namespace Ule4JisAlt
{
    /// <summary>
    /// キーエミュレーション送信のヘルパー（NativeMethods の P/Invoke を使用）
    /// </summary>
    internal static class KeyEmulator
    {
        /// <summary>
        /// キーエミュレーション送信（識別用 EmulatorMarker 付き）
        /// </summary>
        public static void EmulateKey(byte vkCode, bool up)
        {
            uint flags = up ? NativeMethods.KEYEVENTF_KEYUP : 0;
            byte scanCode = (vkCode == NativeMethods.VK_CAPITAL) ? (byte)0x3A : (byte)0;
            if (IsExtendedKey(vkCode))
            {
                flags |= NativeMethods.KEYEVENTF_EXTENDEDKEY;
            }
            NativeMethods.keybd_event(vkCode, scanCode, flags, NativeMethods.EmulatorMarker);
        }

        private static bool IsExtendedKey(byte vkCode)
        {
            switch (vkCode)
            {
                case NativeMethods.VK_RCONTROL:
                case NativeMethods.VK_RMENU:
                case NativeMethods.VK_RSHIFT:
                case NativeMethods.VK_INSERT:
                case NativeMethods.VK_DELETE:
                case NativeMethods.VK_HOME:
                case NativeMethods.VK_END:
                case NativeMethods.VK_PRIOR:
                case NativeMethods.VK_NEXT:
                case NativeMethods.VK_UP:
                case NativeMethods.VK_DOWN:
                case NativeMethods.VK_RIGHT:
                case NativeMethods.VK_LEFT:
                case NativeMethods.VK_NUMLOCK:
                case NativeMethods.VK_CANCEL:
                case NativeMethods.VK_PRINT:
                case NativeMethods.VK_DIVIDE:
                case NativeMethods.VK_SEPARATOR:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsShiftPressed()
        {
            return (NativeMethods.GetKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0 ||
                   (NativeMethods.GetKeyState(NativeMethods.VK_LSHIFT) & 0x8000) != 0 ||
                   (NativeMethods.GetKeyState(NativeMethods.VK_RSHIFT) & 0x8000) != 0;
        }
    }
}
