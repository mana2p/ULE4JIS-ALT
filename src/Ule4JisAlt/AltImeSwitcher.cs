using System;

namespace Ule4JisAlt
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

        public static void NotifyCombo()
        {
            if (_leftAltDown) _leftAltCombo = true;
            if (_rightAltDown) _rightAltCombo = true;
        }

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
                    return false; // 左Alt KeyDown をそのまま通過（Alt+ホイールやAlt+クリック等を正常動作させる）
                }
                else if (isRightAlt)
                {
                    _rightAltDown = true;
                    _rightAltCombo = false;
                    return false; // 右Alt KeyDown をそのまま通過
                }
                else if (!IsAltKey(vkCode))
                {
                    // Alt以外のキーが押された場合、Altコンボが発生したと判定
                    NotifyCombo();
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
                        // メニューバーフォーカス抑制（ダミーキー0x07送信）
                        CancelMenuFocus();

                        // 左Alt単体空打ち -> 無変換キー (VK_NONCONVERT = 0x1D)
                        // 【設計上の理由】
                        // 直接 ImeController.SetStatus(false) を呼ぶのではなく VK_NONCONVERT を送信する理由:
                        // Windows IME (Microsoft IME) では、文字入力中（未確定文字列あり）に無変換キーを押すと「カタカナ変換」を行い、
                        // 入力していない通常時に押すと「IME オフ」になる仕様がある。
                        // 直接 SetStatus(false) を呼ぶと入力中文字列が強制終了されてカタカナ変換が使えなくなるため、
                        // カタカナ変換の操作感を維持するためにあえて VK_NONCONVERT を送信している。
                        // （※IME設定で無変換キーを「IME-オフ」に設定しておくことでこの両立動作が実現する）
                        KeyEmulator.EmulateKey(NativeMethods.VK_NONCONVERT, up: false);
                        KeyEmulator.EmulateKey(NativeMethods.VK_NONCONVERT, up: true);
                    }

                    return false; // 左Alt KeyUp もそのまま通過
                }
                else if (isRightAlt)
                {
                    bool wasDown = _rightAltDown;
                    bool wasCombo = _rightAltCombo;
                    _rightAltDown = false;
                    _rightAltCombo = false;

                    if (wasDown && !wasCombo)
                    {
                        // メニューバーフォーカス抑制（ダミーキー0x07送信）
                        CancelMenuFocus();

                        // 右Alt単体空打ち -> IME ON (かな)
                        ImeController.SetStatus(true);
                    }

                    return false; // 右Alt KeyUp もそのまま通過
                }
            }

            return false;
        }

        private static void CancelMenuFocus()
        {
            // Windows に「Alt + 0x07」が押されたと認識させ、メニューバーのアクティブ化 (SC_KEYMENU) をキャンセルする
            KeyEmulator.EmulateKey(NativeMethods.VK_DUMMY_MENU, up: false);
            KeyEmulator.EmulateKey(NativeMethods.VK_DUMMY_MENU, up: true);
        }

        private static bool IsAltKey(uint vkCode)
        {
            return vkCode == NativeMethods.VK_MENU ||
                   vkCode == NativeMethods.VK_LMENU ||
                   vkCode == NativeMethods.VK_RMENU;
        }
    }
}
