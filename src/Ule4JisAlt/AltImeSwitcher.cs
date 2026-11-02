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

        public static bool IsAltPressed => _leftAltDown || _rightAltDown;

        public static bool ProcessKeyEvent(uint vkCode, uint flags, int msg)
        {
            bool isDown = (msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN);
            bool isUp = (msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP);

            bool isExtended = (flags & NativeMethods.KEYEVENTF_EXTENDEDKEY) != 0;
            bool isLeftAlt = (vkCode == NativeMethods.VK_LMENU) || (vkCode == NativeMethods.VK_MENU && !isExtended);
            bool isRightAlt = (vkCode == NativeMethods.VK_RMENU) || (vkCode == NativeMethods.VK_MENU && isExtended);

            // コンボ成立後（仮想Alt Down送信済み）にUACや画面ロック等でKeyUpが失われ、OS側で既にAltが解除されている場合は自己回復する
            // （※コンボ成立前はフックでAlt押下を消費しているためOS側のGetAsyncKeyStateは0になる。コンボ成立後のみチェックすることでAltコンボを妨げない）
            if (_leftAltCombo && !isLeftAlt &&
                (NativeMethods.GetAsyncKeyState(NativeMethods.VK_LMENU) & 0x8000) == 0 &&
                (NativeMethods.GetAsyncKeyState(NativeMethods.VK_MENU) & 0x8000) == 0)
            {
                _leftAltDown = false;
                _leftAltCombo = false;
            }
            if (_rightAltCombo && !isRightAlt &&
                (NativeMethods.GetAsyncKeyState(NativeMethods.VK_RMENU) & 0x8000) == 0 &&
                (NativeMethods.GetAsyncKeyState(NativeMethods.VK_MENU) & 0x8000) == 0)
            {
                _rightAltDown = false;
                _rightAltCombo = false;
            }

            if (isDown)
            {
                if (isLeftAlt)
                {
                    // 初回押下時のみコンボフラグを初期化（長押し時のキーリピートによるコンボ解除・Altスタックを防止）
                    if (!_leftAltDown)
                    {
                        _leftAltDown = true;
                        _leftAltCombo = false;
                    }
                    return true; // 左Alt KeyDown をフック消費（メニューバー起動防止）
                }
                else if (isRightAlt)
                {
                    // 初回押下時のみコンボフラグを初期化
                    if (!_rightAltDown)
                    {
                        _rightAltDown = true;
                        _rightAltCombo = false;
                    }
                    return true; // 右Alt KeyDown をフック消費
                }
                else if (!IsModifierKey(vkCode))
                {
                    // 通常キーが押された場合、Altコンボ（Alt+Tab等）が発生したと判定
                    if (_leftAltDown && !_leftAltCombo)
                    {
                        _leftAltCombo = true;
                        KeyEmulator.EmulateKey(NativeMethods.VK_LMENU, up: false);
                    }
                    if (_rightAltDown && !_rightAltCombo)
                    {
                        _rightAltCombo = true;
                        KeyEmulator.EmulateKey(NativeMethods.VK_RMENU, up: false);
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
                    else if (wasCombo)
                    {
                        KeyEmulator.EmulateKey(NativeMethods.VK_LMENU, up: true);
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
                        ImeController.SetStatus(true);
                    }
                    else if (wasCombo)
                    {
                        KeyEmulator.EmulateKey(NativeMethods.VK_RMENU, up: true);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// マウス操作（ホイール回転・クリック等）発生時に呼ばれ、Altキーが物理的に押下中ならAltコンボとして成立させる
        /// </summary>
        public static void NotifyMouseCombo()
        {
            if (_leftAltDown && !_leftAltCombo)
            {
                _leftAltCombo = true;
                KeyEmulator.EmulateKey(NativeMethods.VK_LMENU, up: false);
            }
            if (_rightAltDown && !_rightAltCombo)
            {
                _rightAltCombo = true;
                KeyEmulator.EmulateKey(NativeMethods.VK_RMENU, up: false);
            }
        }

        /// <summary>
        /// モード切替・機能無効化・終了時にAlt状態を安全にリセットする。
        /// コンボ中で既にOSへ仮想Alt押下を送信していた場合は、フラグを消した上で仮想Alt離し（KeyUp）を送信する。
        /// </summary>
        public static void Reset()
        {
            bool wasLeftCombo = _leftAltCombo;
            bool wasRightCombo = _rightAltCombo;

            _leftAltDown = false;
            _leftAltCombo = false;
            _rightAltDown = false;
            _rightAltCombo = false;

            if (wasLeftCombo)
            {
                KeyEmulator.EmulateKey(NativeMethods.VK_LMENU, up: true);
            }
            if (wasRightCombo)
            {
                KeyEmulator.EmulateKey(NativeMethods.VK_RMENU, up: true);
            }
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
                   vkCode == NativeMethods.VK_LWIN ||
                   vkCode == NativeMethods.VK_RWIN;
        }
    }
}
