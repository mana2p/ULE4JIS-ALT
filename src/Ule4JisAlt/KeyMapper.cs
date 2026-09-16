using System;

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
        /// エミュレートされたキーを送信する。
        /// </summary>
        public static void SendEmulatedKey(KeyEmulationResult result, bool isUp)
        {
            if (result.Mode == EmulationMode.PressAndRelease)
            {
                ExecuteWithShiftAction(result.TargetVkCode, result.ShiftAction, up: false);
                KeyEmulator.EmulateKey(result.TargetVkCode, up: true);
            }
            else if (!isUp)
            {
                ExecuteWithShiftAction(result.TargetVkCode, result.ShiftAction, up: false);
            }
            else
            {
                KeyEmulator.EmulateKey(result.TargetVkCode, up: true);
            }
        }

        /// <summary>
        /// Shift操作付きでキーを送信する。
        /// </summary>
        private static void ExecuteWithShiftAction(byte targetVkCode, ShiftAction shiftAction, bool up)
        {
            if (shiftAction == ShiftAction.ReleaseShift)
            {
                bool lshift = (NativeMethods.GetKeyState(NativeMethods.VK_LSHIFT) & 0x8000) != 0;
                bool rshift = (NativeMethods.GetKeyState(NativeMethods.VK_RSHIFT) & 0x8000) != 0;

                if (lshift) KeyEmulator.EmulateKey(NativeMethods.VK_LSHIFT, up: true);
                if (rshift) KeyEmulator.EmulateKey(NativeMethods.VK_RSHIFT, up: true);

                KeyEmulator.EmulateKey(targetVkCode, up);

                if (lshift) KeyEmulator.EmulateKey(NativeMethods.VK_LSHIFT, up: false);
                if (rshift) KeyEmulator.EmulateKey(NativeMethods.VK_RSHIFT, up: false);
            }
            else if (shiftAction == ShiftAction.PressShift)
            {
                KeyEmulator.EmulateKey(NativeMethods.VK_LSHIFT, up: false);
                KeyEmulator.EmulateKey(targetVkCode, up);
                KeyEmulator.EmulateKey(NativeMethods.VK_LSHIFT, up: true);
            }
            else
            {
                KeyEmulator.EmulateKey(targetVkCode, up);
            }
        }
    }
}
