using System;

namespace Ule4Jis.Net
{
    public enum ShiftAction
    {
        KeepState,       // そのままのShift状態
        PressShift,      // Shiftを押して送信
        ReleaseShift     // Shiftを離して送信
    }

    /// <summary>
    /// 半角全角キーの特殊動作を表すフラグ。
    /// オリジナルC++の PressAndReleaseDecorator に相当。
    /// Down/Up 両方で「Press→Release」をセットで送信する。
    /// </summary>
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

    public static class UsOnJisMapper
    {
        /// <summary>
        /// JIS配列のVKコード+Shift状態を、US配列で期待される出力に変換するためのマッピングを返す。
        /// オリジナルC++版 USonJISStrategy.cpp のマッピングテーブルと完全一致。
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

                    // ~ (Shift+半角全角)
                    case NativeMethods.VK_OEM_AUTO: // VK_OEM_AUTO: NOP (無視)
                        result = null;
                        return true; // handled = true だが result=null → 元キーを消費して何もしない

                    case NativeMethods.VK_OEM_ENLW: // VK_OEM_ENLW: PressAndRelease(Normal(VK_OEM_7))
                        // オリジナル: PressAndRelease(ShiftRelease不要、Shift維持のまま VK_OEM_7)
                        // JIS: Shift+VK_OEM_7 = ~ (チルダ) ← USの~と同じ
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

                    // ` (半角全角キー)
                    case NativeMethods.VK_OEM_AUTO: // VK_OEM_AUTO: NOP (無視)
                        result = null;
                        return true; // handled = true だが result=null → 元キーを消費して何もしない

                    case NativeMethods.VK_OEM_ENLW: // VK_OEM_ENLW: PressAndRelease(ShiftPress(Normal(VK_OEM_3)))
                        // JIS: Shift+VK_OEM_3 = ` (バッククオート) ← USの`と同じ
                        result = new KeyEmulationResult(NativeMethods.VK_OEM_3, ShiftAction.PressShift, EmulationMode.PressAndRelease);
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// エミュレートされたキーを送信する。
        /// オリジナルC++版の設計に忠実に：
        /// - Shift操作 (ShiftRelease/ShiftPress) は KeyDown時のみ実行
        /// - KeyUp時は素のキーUpだけ送信（Shift操作なし）
        /// - PressAndReleaseモードでは Down+Up をセットで送信
        /// </summary>
        public static void SendEmulatedKey(KeyEmulationResult result, bool isDown)
        {
            if (result.Mode == EmulationMode.PressAndRelease)
            {
                // PressAndReleaseDecorator: Down/Up両方で Down+Up をフルセット送信
                SendKeyWithShiftAction(result.TargetVkCode, result.ShiftAction, isDown: true);
                NativeMethods.SendKey(result.TargetVkCode, isDown: false);
            }
            else if (isDown)
            {
                // KeyDown: Shift操作付きでキーDown送信
                SendKeyWithShiftAction(result.TargetVkCode, result.ShiftAction, isDown: true);
            }
            else
            {
                // KeyUp: Shift操作なし、素のキーUpだけ送信
                NativeMethods.SendKey(result.TargetVkCode, isDown: false);
            }
        }

        /// <summary>
        /// Shift操作付きでキーを送信する（KeyDown時のみ使用）。
        /// オリジナルC++版のShiftReleaseDecorator/ShiftPressDecoratorと同じ動作。
        /// </summary>
        private static void SendKeyWithShiftAction(byte targetVkCode, ShiftAction shiftAction, bool isDown)
        {
            if (shiftAction == ShiftAction.ReleaseShift)
            {
                // ShiftReleaseDecorator相当:
                // 左右Shiftの実際の状態を取得して、押されている側を解除→キー送信→復元
                bool lshift = (NativeMethods.GetKeyState(NativeMethods.VK_LSHIFT) & 0x8000) != 0;
                bool rshift = (NativeMethods.GetKeyState(NativeMethods.VK_RSHIFT) & 0x8000) != 0;

                if (lshift) NativeMethods.SendKey(NativeMethods.VK_LSHIFT, isDown: false);
                if (rshift) NativeMethods.SendKey(NativeMethods.VK_RSHIFT, isDown: false);

                NativeMethods.SendKey(targetVkCode, isDown);

                if (lshift) NativeMethods.SendKey(NativeMethods.VK_LSHIFT, isDown: true);
                if (rshift) NativeMethods.SendKey(NativeMethods.VK_RSHIFT, isDown: true);
            }
            else if (shiftAction == ShiftAction.PressShift)
            {
                // ShiftPressDecorator相当:
                // LShiftを押す→キー送信→LShiftを離す
                NativeMethods.SendKey(NativeMethods.VK_LSHIFT, isDown: true);
                NativeMethods.SendKey(targetVkCode, isDown);
                NativeMethods.SendKey(NativeMethods.VK_LSHIFT, isDown: false);
            }
            else
            {
                // KeepState: そのままキー送信
                NativeMethods.SendKey(targetVkCode, isDown);
            }
        }
    }
}
