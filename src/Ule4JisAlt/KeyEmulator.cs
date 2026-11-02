using System;
using System.Runtime.InteropServices;

namespace Ule4JisAlt
{
    /// <summary>
    /// キーエミュレーション送信のヘルパー（NativeMethods.SendInput を使用）
    /// </summary>
    internal static class KeyEmulator
    {
        private static readonly int InputStructSize = Marshal.SizeOf<NativeMethods.INPUT>();

        /// <summary>
        /// 単一キーエミュレーション送信（識別用 EmulatorMarker 付き）
        /// </summary>
        public static void EmulateKey(byte vkCode, bool up)
        {
            var inputs = new NativeMethods.INPUT[] { CreateKeyInput(vkCode, up) };
            NativeMethods.SendInput(1, inputs, InputStructSize);
        }

        /// <summary>
        /// 複数キーイベントを1回の SendInput でアトミックに一括送信する
        /// </summary>
        public static void EmulateKeys(ReadOnlySpan<(byte vkCode, bool up)> keys)
        {
            if (keys.IsEmpty)
            {
                return;
            }

            var inputs = new NativeMethods.INPUT[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                inputs[i] = CreateKeyInput(keys[i].vkCode, keys[i].up);
            }

            NativeMethods.SendInput((uint)inputs.Length, inputs, InputStructSize);
        }

        internal static NativeMethods.INPUT CreateKeyInput(byte vkCode, bool up)
        {
            uint flags = up ? NativeMethods.KEYEVENTF_KEYUP : 0;
            ushort scanCode = (vkCode == NativeMethods.VK_CAPITAL) ? (ushort)0x3A : (ushort)0;
            if (IsExtendedKey(vkCode))
            {
                flags |= NativeMethods.KEYEVENTF_EXTENDEDKEY;
            }

            return new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_KEYBOARD,
                u = new NativeMethods.InputUnion
                {
                    ki = new NativeMethods.KEYBDINPUT
                    {
                        wVk = vkCode,
                        wScan = scanCode,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = NativeMethods.EmulatorMarker
                    }
                }
            };
        }

        internal static bool IsExtendedKey(byte vkCode)
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
            return (NativeMethods.GetAsyncKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0 ||
                   (NativeMethods.GetAsyncKeyState(NativeMethods.VK_LSHIFT) & 0x8000) != 0 ||
                   (NativeMethods.GetAsyncKeyState(NativeMethods.VK_RSHIFT) & 0x8000) != 0;
        }
    }
}
