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

        private const uint ImeSendMessageTimeoutMs = 50;

        public static bool TryGetStatus(out bool isOpen)
        {
            isOpen = false;
            IntPtr imeWnd = GetImeWindowHandle();
            if (imeWnd == IntPtr.Zero) return false;

            IntPtr sendResult = NativeMethods.SendMessageTimeout(
                imeWnd,
                NativeMethods.WM_IME_CONTROL,
                (IntPtr)NativeMethods.IMC_GETOPENSTATUS,
                IntPtr.Zero,
                NativeMethods.SMTO_ABORTIFHUNG,
                ImeSendMessageTimeoutMs,
                out IntPtr res);

            if (sendResult == IntPtr.Zero) return false;

            isOpen = res != IntPtr.Zero;
            return true;
        }

        public static bool GetStatus()
        {
            return TryGetStatus(out bool isOpen) && isOpen;
        }

        public static void SetStatus(bool enable)
        {
            // 1. WM_IME_CONTROL メッセージによる確実なIME切り替え（ハング中ウィンドウでのフック詰まりを避けるためタイムアウト付き）
            IntPtr imeWnd = GetImeWindowHandle();
            if (imeWnd != IntPtr.Zero)
            {
                NativeMethods.SendMessageTimeout(
                    imeWnd,
                    NativeMethods.WM_IME_CONTROL,
                    (IntPtr)NativeMethods.IMC_SETOPENSTATUS,
                    (IntPtr)(enable ? 1 : 0),
                    NativeMethods.SMTO_ABORTIFHUNG,
                    ImeSendMessageTimeoutMs,
                    out _);
            }

            // 2. メッセージ送信後も状態が一致しない場合のキー送信補填
            // ※タイムアウト等で現在状態が不明な場合は、VK_KANJI（トグル）による意図しない反転を避けるため補填を行わない
            if (!TryGetStatus(out bool currentStatus))
            {
                return;
            }

            if (enable && !currentStatus)
            {
                KeyEmulator.EmulateKey(NativeMethods.VK_IME_ON, up: false);
                KeyEmulator.EmulateKey(NativeMethods.VK_IME_ON, up: true);

                if (TryGetStatus(out currentStatus) && !currentStatus)
                {
                    KeyEmulator.EmulateKey(NativeMethods.VK_KANJI, up: false);
                    KeyEmulator.EmulateKey(NativeMethods.VK_KANJI, up: true);
                }
            }
            else if (!enable && currentStatus)
            {
                KeyEmulator.EmulateKey(NativeMethods.VK_IME_OFF, up: false);
                KeyEmulator.EmulateKey(NativeMethods.VK_IME_OFF, up: true);

                if (TryGetStatus(out currentStatus) && currentStatus)
                {
                    KeyEmulator.EmulateKey(NativeMethods.VK_KANJI, up: false);
                    KeyEmulator.EmulateKey(NativeMethods.VK_KANJI, up: true);
                }
            }
        }
    }
}
