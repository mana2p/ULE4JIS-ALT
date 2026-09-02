using System;
using System.Runtime.InteropServices;

namespace Ule4JisAlt
{
    /// <summary>
    /// IME 状態の取得・設定を行うヘルパー
    /// </summary>
    internal static class ImeController
    {
        /// <summary>
        /// フォアグラウンドウィンドウの IME ウィンドウハンドルを取得する。
        /// GetStatus / SetStatus に共通する処理を集約。
        /// </summary>
        private static IntPtr GetImeWindowHandle()
        {
            IntPtr fgWnd = NativeMethods.GetForegroundWindow();
            if (fgWnd == IntPtr.Zero) return IntPtr.Zero;

            uint threadId = NativeMethods.GetWindowThreadProcessId(fgWnd, out _);
            IntPtr targetWnd = fgWnd;

            NativeMethods.GUITHREADINFO gti = new NativeMethods.GUITHREADINFO();
            gti.cbSize = Marshal.SizeOf(typeof(NativeMethods.GUITHREADINFO));
            if (NativeMethods.GetGUIThreadInfo(threadId, ref gti) && gti.hwndFocus != IntPtr.Zero)
            {
                targetWnd = gti.hwndFocus;
            }

            return NativeMethods.ImmGetDefaultIMEWnd(targetWnd);
        }

        public static bool GetStatus()
        {
            IntPtr imeWnd = GetImeWindowHandle();
            if (imeWnd == IntPtr.Zero) return false;

            IntPtr res = NativeMethods.SendMessage(imeWnd, NativeMethods.WM_IME_CONTROL, (IntPtr)NativeMethods.IMC_GETOPENSTATUS, IntPtr.Zero);
            return res != IntPtr.Zero;
        }

        public static void SetStatus(bool enable)
        {
            // 1. WM_IME_CONTROL メッセージによる確実なIME切り替え
            IntPtr imeWnd = GetImeWindowHandle();
            if (imeWnd != IntPtr.Zero)
            {
                NativeMethods.SendMessage(imeWnd, NativeMethods.WM_IME_CONTROL, (IntPtr)NativeMethods.IMC_SETOPENSTATUS, (IntPtr)(enable ? 1 : 0));
            }

            // 2. メッセージ送信後も状態が一致しない場合のキー送信補填
            bool currentStatus = GetStatus();
            if (enable && !currentStatus)
            {
                KeyEmulator.EmulateKey(NativeMethods.VK_IME_ON, up: false);
                KeyEmulator.EmulateKey(NativeMethods.VK_IME_ON, up: true);

                if (!GetStatus())
                {
                    KeyEmulator.EmulateKey(NativeMethods.VK_KANJI, up: false);
                    KeyEmulator.EmulateKey(NativeMethods.VK_KANJI, up: true);
                }
            }
            else if (!enable && currentStatus)
            {
                KeyEmulator.EmulateKey(NativeMethods.VK_IME_OFF, up: false);
                KeyEmulator.EmulateKey(NativeMethods.VK_IME_OFF, up: true);

                if (GetStatus())
                {
                    KeyEmulator.EmulateKey(NativeMethods.VK_KANJI, up: false);
                    KeyEmulator.EmulateKey(NativeMethods.VK_KANJI, up: true);
                }
            }
        }
    }
}
