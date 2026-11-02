using System;
using System.Collections.Generic;

namespace Ule4JisAlt
{
    public enum LayoutMode
    {
        InternalJisExternalUs = 0, // 内蔵: JIS / 外付け: US (デフォルト)
        InternalUsExternalJis = 1, // 内蔵: US / 外付け: JIS

        // 互換用エイリアス
        ExternalUs = 0,
        ExternalJis = 1
    }

    public enum ShiftAction
    {
        KeepState,       // そのままのShift状態
        PressShift,      // Shiftを押して送信
        ReleaseShift     // Shiftを離して送信
    }

    public enum EmulationMode
    {
        Normal,          // 通常: KeyDown→送Down, KeyUp→送Up
        PressAndRelease  // 特殊: KeyDown→送Down+Up, KeyUp→送Down+Up
    }

    public class KeyEmulationResult
    {
        public byte TargetVkCode { get; set; }
        public ShiftAction ShiftAction { get; set; }
        public EmulationMode Mode { get; set; }

        public KeyEmulationResult(byte targetVkCode, ShiftAction shiftAction = ShiftAction.KeepState, EmulationMode mode = EmulationMode.Normal)
        {
            TargetVkCode = targetVkCode;
            ShiftAction = shiftAction;
            Mode = mode;
        }
    }

    public static class KeyMapper
    {
        /// <summary>
        /// US配列キーボードをJIS配列OS上で打鍵したときのマッピング判定を行う。
        /// オリジナル ULE4JIS USonJISStrategy と完全互換。
        /// </summary>
        public static bool TryMapKey(uint vkCode, bool isShift, out KeyEmulationResult? result)
        {
            result = null;

            if (isShift)
            {
                switch (vkCode)
                {
                    case '2': // Shift+2 -> @ : ShiftRelease + VK_OEM_3
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_3, ShiftAction.ReleaseShift);
                        return true;

                    case '6': // Shift+6 -> ^ : ShiftRelease + VK_OEM_7
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_7, ShiftAction.ReleaseShift);
                        return true;

                    case '7': // Shift+7 -> & : Normal '6' (Shift維持)
                        result = new KeyEmulationResult((byte)'6', ShiftAction.KeepState);
                        return true;

                    case '8': // Shift+8 -> * : Normal VK_OEM_1 (Shift維持)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_1, ShiftAction.KeepState);
                        return true;

                    case '9': // Shift+9 -> ( : Normal '8' (Shift維持)
                        result = new KeyEmulationResult((byte)'8', ShiftAction.KeepState);
                        return true;

                    case '0': // Shift+0 -> ) : Normal '9' (Shift維持)
                        result = new KeyEmulationResult((byte)'9', ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_MINUS: // Shift+- -> _ : Normal VK_OEM_102 (Shift維持)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_102, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_7: // Shift+^ -> + : Normal VK_OEM_PLUS (Shift維持)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_PLUS, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_3: // Shift+@ -> { : Normal VK_OEM_4 (Shift維持)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_4, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_4: // Shift+[ -> } : Normal VK_OEM_6 (Shift維持)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_6, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_PLUS: // Shift+; -> : : ShiftRelease + VK_OEM_1
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_1, ShiftAction.ReleaseShift);
                        return true;

                    case NativeMethods.VK_OEM_1: // Shift+: -> " : Normal '2' (Shift維持)
                        result = new KeyEmulationResult((byte)'2', ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_6: // Shift+] -> | : Normal VK_OEM_5 (Shift維持)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_5, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_AUTO:
                        result = null;
                        return true; // NOP

                    case NativeMethods.VK_OEM_ENLW:
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_7, ShiftAction.KeepState, EmulationMode.PressAndRelease);
                        return true;
                }
            }
            else
            {
                switch (vkCode)
                {
                    case NativeMethods.VK_OEM_7: // ^ -> = : ShiftPress + VK_OEM_MINUS
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_MINUS, ShiftAction.PressShift);
                        return true;

                    case NativeMethods.VK_OEM_3: // @ -> [ : Normal VK_OEM_4
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_4, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_4: // [ -> ] : Normal VK_OEM_6
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_6, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_1: // : -> ' : ShiftPress + '7'
                        result = new KeyEmulationResult((byte)'7', ShiftAction.PressShift);
                        return true;

                    case NativeMethods.VK_OEM_6: // ] -> \ : Normal VK_OEM_102
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_102, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_AUTO:
                        result = null;
                        return true; // NOP

                    case NativeMethods.VK_OEM_ENLW:
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_3, ShiftAction.PressShift, EmulationMode.PressAndRelease);
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// エミュレートされたキーを1回の SendInput でアトミックに送信する。
        /// </summary>
        public static void SendEmulatedKey(KeyEmulationResult result, bool isUp)
        {
            bool lshift = false;
            bool rshift = false;
            if (result.ShiftAction == ShiftAction.ReleaseShift && (result.Mode == EmulationMode.PressAndRelease || !isUp))
            {
                lshift = (NativeMethods.GetAsyncKeyState(NativeMethods.VK_LSHIFT) & 0x8000) != 0;
                rshift = (NativeMethods.GetAsyncKeyState(NativeMethods.VK_RSHIFT) & 0x8000) != 0;
            }

            Span<(byte vkCode, bool up)> buffer = stackalloc (byte vkCode, bool up)[6];
            int count = BuildKeySequence(result, isUp, lshift, rshift, buffer);
            KeyEmulator.EmulateKeys(buffer.Slice(0, count));
        }

        /// <summary>
        /// 送信すべき (vkCode, up) イベント列を構築する（アロケーションなし、単体テスト可能）。
        /// </summary>
        internal static int BuildKeySequence(
            KeyEmulationResult result,
            bool isUp,
            bool lshiftPressed,
            bool rshiftPressed,
            Span<(byte vkCode, bool up)> destination)
        {
            int idx = 0;

            if (result.Mode == EmulationMode.PressAndRelease)
            {
                idx += AppendShiftWrappedKey(result.TargetVkCode, result.ShiftAction, up: false, lshiftPressed, rshiftPressed, destination);
                destination[idx++] = (result.TargetVkCode, true);
            }
            else if (!isUp)
            {
                idx += AppendShiftWrappedKey(result.TargetVkCode, result.ShiftAction, up: false, lshiftPressed, rshiftPressed, destination);
            }
            else
            {
                destination[idx++] = (result.TargetVkCode, true);
            }

            return idx;
        }

        private static int AppendShiftWrappedKey(
            byte targetVkCode,
            ShiftAction shiftAction,
            bool up,
            bool lshiftPressed,
            bool rshiftPressed,
            Span<(byte vkCode, bool up)> destination)
        {
            int idx = 0;

            if (shiftAction == ShiftAction.ReleaseShift)
            {
                if (lshiftPressed) destination[idx++] = (NativeMethods.VK_LSHIFT, true);
                if (rshiftPressed) destination[idx++] = (NativeMethods.VK_RSHIFT, true);

                destination[idx++] = (targetVkCode, up);

                if (lshiftPressed) destination[idx++] = (NativeMethods.VK_LSHIFT, false);
                if (rshiftPressed) destination[idx++] = (NativeMethods.VK_RSHIFT, false);
            }
            else if (shiftAction == ShiftAction.PressShift)
            {
                destination[idx++] = (NativeMethods.VK_LSHIFT, false);
                destination[idx++] = (targetVkCode, up);
                destination[idx++] = (NativeMethods.VK_LSHIFT, true);
            }
            else
            {
                destination[idx++] = (targetVkCode, up);
            }

            return idx;
        }
    }
}
