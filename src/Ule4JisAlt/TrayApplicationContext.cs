using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Ule4Jis.Net
{
    public class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ToolStripMenuItem _emulationMenuItem;
        private readonly ToolStripMenuItem _altImeMenuItem;
        private readonly ToolStripMenuItem _startupMenuItem;

        private Icon? _currentIcon;
        private const string AppName = "Ule4Jis.Net";
        private const string RegistryRunPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public TrayApplicationContext()
        {
            _emulationMenuItem = new ToolStripMenuItem("US配列エミュレーション (ULE4JIS)", null, OnToggleEmulation)
            {
                Checked = KeyboardHook.EmulationEnabled
            };

            _altImeMenuItem = new ToolStripMenuItem("左右Alt空打ちIME切り替え", null, OnToggleAltIme)
            {
                Checked = KeyboardHook.AltImeEnabled
            };

            _startupMenuItem = new ToolStripMenuItem("Windows起動時に自動起動", null, OnToggleStartup)
            {
                Checked = IsStartupEnabled()
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(_emulationMenuItem);
            contextMenu.Items.Add(_altImeMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(_startupMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("終了 (&X)", null, OnExit);

            _notifyIcon = new NotifyIcon
            {
                Text = "ULE4JIS-ALT",
                ContextMenuStrip = contextMenu,
                Visible = true
            };

            // 初期アイコン更新
            UpdateTrayIcon();

            // フック開始
            KeyboardHook.Start();
        }

        private void UpdateTrayIcon()
        {
            Icon oldIcon = _currentIcon!;
            _currentIcon = IconGenerator.CreatePixelKeyIcon(KeyboardHook.EmulationEnabled);
            _notifyIcon.Icon = _currentIcon;

            if (oldIcon != null)
            {
                oldIcon.Dispose();
            }
        }

        private void OnToggleEmulation(object? sender, EventArgs e)
        {
            KeyboardHook.EmulationEnabled = !KeyboardHook.EmulationEnabled;
            _emulationMenuItem.Checked = KeyboardHook.EmulationEnabled;
            UpdateTrayIcon();
        }

        private void OnToggleAltIme(object? sender, EventArgs e)
        {
            KeyboardHook.AltImeEnabled = !KeyboardHook.AltImeEnabled;
            _altImeMenuItem.Checked = KeyboardHook.AltImeEnabled;
        }

        private void OnToggleStartup(object? sender, EventArgs e)
        {
            bool enable = !IsStartupEnabled();
            SetStartup(enable);
            _startupMenuItem.Checked = enable;
        }

        private static bool IsStartupEnabled()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, false);
            return key?.GetValue(AppName) != null;
        }

        private static void SetStartup(bool enable)
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, true);
            if (key == null) return;

            if (enable)
            {
                string exePath = Application.ExecutablePath;
                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }

        private void OnExit(object? sender, EventArgs e)
        {
            KeyboardHook.Stop();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _currentIcon?.Dispose();
            Application.Exit();
        }
    }
}
