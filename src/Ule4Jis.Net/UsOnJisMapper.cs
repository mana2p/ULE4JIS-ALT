using System;

namespace Ule4Jis.Net
{
    public enum ShiftAction
    {
        KeepState,       // そのままのShift状態
        PressShift,      // Shiftを押して送信
        ReleaseShift     // Shiftを離して送信
    }

    public class KeyEmulationResult
    {
        public byte TargetVkCode { get; set; }
        public ShiftAction ShiftAction { get; set; }

        public KeyEmulationResult(byte targetVkCode, ShiftAction shiftAction = ShiftAction.KeepState)
        {
            TargetVkCode = targetVkCode;
            ShiftAction = shiftAction;
        }
    }

    public static class UsOnJisMapper
    {
        public static bool TryMapKey(uint vkCode, bool isShift, out KeyEmulationResult? result)
        {
            result = null;

            if (isShift)
            {
                switch (vkCode)
                {
                    case '2': // Shift + 2 -> @ (Release Shift, send VK_OEM_3)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_3, ShiftAction.ReleaseShift);
                        return true;

                    case '6': // Shift + 6 -> ^ (Release Shift, send VK_OEM_7)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_7, ShiftAction.ReleaseShift);
                        return true;

                    case '7': // Shift + 7 -> & (Keep Shift, send '6')
                        result = new KeyEmulationResult((byte)'6', ShiftAction.KeepState);
                        return true;

                    case '8': // Shift + 8 -> * (Keep Shift, send VK_OEM_1)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_1, ShiftAction.KeepState);
                        return true;

                    case '9': // Shift + 9 -> ( (Keep Shift, send '8')
                        result = new KeyEmulationResult((byte)'8', ShiftAction.KeepState);
                        return true;

                    case '0': // Shift + 0 -> ) (Keep Shift, send '9')
                        result = new KeyEmulationResult((byte)'9', ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_MINUS: // Shift + - -> _ (Keep Shift, send VK_OEM_102)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_102, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_7: // Shift + ^ -> + (Keep Shift, send VK_OEM_PLUS)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_PLUS, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_3: // Shift + @ -> { (Keep Shift, send VK_OEM_4)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_4, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_4: // Shift + [ -> } (Keep Shift, send VK_OEM_6)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_6, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_PLUS: // Shift + ; -> : (Release Shift, send VK_OEM_1)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_1, ShiftAction.ReleaseShift);
                        return true;

                    case NativeMethods.VK_OEM_1: // Shift + : -> " (Keep Shift, send '2')
                        result = new KeyEmulationResult((byte)'2', ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_6: // Shift + ] -> | (Keep Shift, send VK_OEM_5)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_5, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_ENLW:
                    case NativeMethods.VK_OEM_AUTO: // Shift + 半角/全角 -> ~ (Press Shift, send VK_OEM_7)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_7, ShiftAction.PressShift);
                        return true;
                }
            }
            else
            {
                switch (vkCode)
                {
                    case NativeMethods.VK_OEM_7: // ^ -> = (Press Shift, send VK_OEM_MINUS)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_MINUS, ShiftAction.PressShift);
                        return true;

                    case NativeMethods.VK_OEM_3: // @ -> [ (Keep Shift, send VK_OEM_4)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_4, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_4: // [ -> ] (Keep Shift, send VK_OEM_6)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_6, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_1: // : -> ' (Press Shift, send '7')
                        result = new KeyEmulationResult((byte)'7', ShiftAction.PressShift);
                        return true;

                    case NativeMethods.VK_OEM_6: // ] -> \ (Keep Shift, send VK_OEM_102)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_102, ShiftAction.KeepState);
                        return true;

                    case NativeMethods.VK_OEM_ENLW:
                    case NativeMethods.VK_OEM_AUTO: // 半角/全角 -> ` (Press Shift, send VK_OEM_3)
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_3, ShiftAction.PressShift);
                        return true;
                }
            }

            return false;
        }

        public static void SendEmulatedKey(byte targetVkCode, ShiftAction shiftAction, bool isDown)
        {
            NativeMethods.SendAtomicEmulatedKey(targetVkCode, shiftAction, isDown);
        }
    }
}
