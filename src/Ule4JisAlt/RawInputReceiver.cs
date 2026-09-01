using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Ule4Jis.Net
{
    public class RawInputReceiver : NativeWindow, IDisposable
    {
        private const int WM_INPUT = 0x00FF;
        private const uint RID_INPUT = 0x10000003;
        private const uint RIDI_DEVICENAME = 0x20000007;

        private const ushort HID_USAGE_PAGE_GENERIC = 0x01;
        private const ushort HID_USAGE_KEYBOARD = 0x06;
        private const uint RIDEV_INPUTSINK = 0x0100;

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, [Out] StringBuilder pData, ref uint pcbSize);

        private static readonly ConcurrentDictionary<IntPtr, bool> _deviceIsExternalCache = new ConcurrentDictionary<IntPtr, bool>();

        /// <summary>
        /// 直近にキーが入力された物理キーボードが外付けキーボードかどうか
        /// </summary>
        public static bool IsLastInputFromExternal { get; private set; } = false;

        /// <summary>
        /// 直近の物理キーボードのデバイスパス
        /// </summary>
        public static string LastDevicePath { get; private set; } = string.Empty;

        /// <summary>
        /// 自動判別機能の有効/無効
        /// </summary>
        public static bool AutoDetectionEnabled { get; set; } = true;

        public RawInputReceiver()
        {
            CreateHandle(new CreateParams());

            RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];
            rid[0].usUsagePage = HID_USAGE_PAGE_GENERIC;
            rid[0].usUsage = HID_USAGE_KEYBOARD;
            rid[0].dwFlags = RIDEV_INPUTSINK; // バックグラウンドでも受信
            rid[0].hwndTarget = Handle;

            RegisterRawInputDevices(rid, 1, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE)));
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_INPUT && AutoDetectionEnabled)
            {
                ProcessRawInput(m.LParam);
            }
            base.WndProc(ref m);
        }

        private static void ProcessRawInput(IntPtr hRawInput)
        {
            uint dwSize = 0;
            uint headerSize = (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER));

            GetRawInputData(hRawInput, RID_INPUT, IntPtr.Zero, ref dwSize, headerSize);
            if (dwSize == 0) return;

            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
            try
            {
                if (GetRawInputData(hRawInput, RID_INPUT, buffer, ref dwSize, headerSize) == dwSize)
                {
                    RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>(buffer);
                    IntPtr hDevice = header.hDevice;

                    if (hDevice != IntPtr.Zero)
                    {
                        bool isExternal = _deviceIsExternalCache.GetOrAdd(hDevice, CheckIfDeviceIsExternal);
                        IsLastInputFromExternal = isExternal;
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static bool CheckIfDeviceIsExternal(IntPtr hDevice)
        {
            uint pcbSize = 0;
            GetRawInputDeviceInfo(hDevice, RIDI_DEVICENAME, null!, ref pcbSize);
            if (pcbSize == 0) return false;

            StringBuilder sb = new StringBuilder((int)pcbSize);
            if (GetRawInputDeviceInfo(hDevice, RIDI_DEVICENAME, sb, ref pcbSize) > 0)
            {
                string path = sb.ToString();
                LastDevicePath = path;

                string upperPath = path.ToUpperInvariant();

                // 1. ノートPC内蔵キーボード識別子 (PS/2, ACPI, I2C 内蔵バス)
                if (upperPath.Contains("ACPI") ||
                    upperPath.Contains("PNP0303") ||
                    upperPath.Contains("PNP030B") ||
                    upperPath.Contains("PNP0C50") ||
                    upperPath.Contains("RDP_KBD") ||
                    upperPath.Contains("ROOT_KBD") ||
                    upperPath.Contains("I2C"))
                {
                    return false; // 内蔵キーボード
                }

                // 2. 外付け USB / Bluetooth キーボード識別子
                if (upperPath.Contains("USB") ||
                    upperPath.Contains("BTHENUM") ||
                    upperPath.Contains("BLUETOOTH") ||
                    upperPath.Contains("VID_"))
                {
                    return true; // 外付けキーボード
                }
            }

            return false;
        }

        public void Dispose()
        {
            DestroyHandle();
            GC.SuppressFinalize(this);
        }
    }
}
