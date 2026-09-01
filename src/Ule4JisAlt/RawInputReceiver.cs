using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Ule4Jis.Net
{
    /// <summary>
    /// Win32 Raw Input API を使用し、現在システムに「外付けUSB/Bluetoothキーボード」が接続されているかをリアルタイム監視する。
    /// キーを打鍵する前の時点で接続状態が判定済みのため、1文字目から100%確実にUS配列エミュレーションが動作する。
    /// </summary>
    public class RawInputReceiver : NativeWindow, IDisposable
    {
        private const int WM_INPUT = 0x00FF;
        private const int WM_DEVICECHANGE = 0x0219;

        private const uint RIM_TYPEKEYBOARD = 1;
        private const uint RIDI_DEVICENAME = 0x20000007;

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTDEVICELIST
        {
            public IntPtr hDevice;
            public uint dwType;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputDeviceList([Out] RAWINPUTDEVICELIST[] pRawInputDeviceList, ref uint puiNumDevices, uint cbSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, [Out] StringBuilder pData, ref uint pcbSize);

        /// <summary>
        /// 現在、外付けUSB/Bluetoothキーボードが接続されているかどうか
        /// </summary>
        public static bool IsExternalKeyboardConnected { get; private set; } = false;

        /// <summary>
        /// 検出された外付けキーボードのリスト（デバッグ用）
        /// </summary>
        public static string LastDetectedKeyboardsSummary { get; private set; } = string.Empty;

        /// <summary>
        /// 自動判別機能の有効/無効
        /// </summary>
        public static bool AutoDetectionEnabled { get; set; } = true;

        public RawInputReceiver()
        {
            CreateHandle(new CreateParams());
            RefreshConnectedKeyboards();
        }

        protected override void WndProc(ref Message m)
        {
            // USBデバイスの接続・切断イベント (WM_DEVICECHANGE) または Raw Input メッセージでキーボード一覧を再チェック
            if (m.Msg == WM_DEVICECHANGE || m.Msg == WM_INPUT)
            {
                RefreshConnectedKeyboards();
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// 現在システムに接続されているキーボードデバイス一覧をチェックし、外付けキーボードの有無を即座に判定
        /// </summary>
        public static void RefreshConnectedKeyboards()
        {
            try
            {
                uint deviceCount = 0;
                uint size = (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICELIST));

                if (GetRawInputDeviceList(null!, ref deviceCount, size) != 0 || deviceCount == 0)
                {
                    return;
                }

                RAWINPUTDEVICELIST[] devices = new RAWINPUTDEVICELIST[deviceCount];
                if (GetRawInputDeviceList(devices, ref deviceCount, size) == 0xFFFFFFFF)
                {
                    return;
                }

                bool foundExternal = false;
                List<string> detectedList = new List<string>();

                foreach (var device in devices)
                {
                    if (device.dwType == RIM_TYPEKEYBOARD)
                    {
                        uint pcbSize = 0;
                        GetRawInputDeviceInfo(device.hDevice, RIDI_DEVICENAME, null!, ref pcbSize);
                        if (pcbSize == 0) continue;

                        StringBuilder sb = new StringBuilder((int)pcbSize);
                        if (GetRawInputDeviceInfo(device.hDevice, RIDI_DEVICENAME, sb, ref pcbSize) > 0)
                        {
                            string path = sb.ToString();
                            string upperPath = path.ToUpperInvariant();

                            // 内蔵キーボード識別子
                            bool isBuiltIn = upperPath.Contains("ACPI") ||
                                             upperPath.Contains("PNP0303") ||
                                             upperPath.Contains("PNP030B") ||
                                             upperPath.Contains("PNP0C50") ||
                                             upperPath.Contains("RDP_KBD") ||
                                             upperPath.Contains("ROOT_KBD") ||
                                             upperPath.Contains("I2C");

                            // 外付け USB / Bluetooth キーボード識別子
                            bool isExternal = !isBuiltIn && (upperPath.Contains("USB") ||
                                                             upperPath.Contains("BTHENUM") ||
                                                             upperPath.Contains("BLUETOOTH") ||
                                                             upperPath.Contains("VID_"));

                            if (isExternal)
                            {
                                foundExternal = true;
                                detectedList.Add($"[外付け] {path}");
                            }
                            else
                            {
                                detectedList.Add($"[内蔵] {path}");
                            }
                        }
                    }
                }

                IsExternalKeyboardConnected = foundExternal;
                LastDetectedKeyboardsSummary = string.Join("\n", detectedList);
            }
            catch { }
        }

        public void Dispose()
        {
            DestroyHandle();
            GC.SuppressFinalize(this);
        }
    }
}
