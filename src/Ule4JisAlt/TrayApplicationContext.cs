using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ule4JisAlt
{
    public class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ToolStripMenuItem _emulationMenuItem;
        private readonly ToolStripMenuItem _autoDetectMenuItem;
        private readonly ToolStripMenuItem _altImeMenuItem;
        private readonly ToolStripMenuItem _startupMenuItem;
        private readonly RawInputReceiver _rawInputReceiver;

        private Icon? _currentIcon;

        public TrayApplicationContext()
        {
            _rawInputReceiver = new RawInputReceiver();

            _emulationMenuItem = new ToolStripMenuItem("US配列エミュレーション (ULE4JIS)", null, OnToggleEmulation)
            {
                Checked = KeyboardHook.EmulationEnabled
            };

            _autoDetectMenuItem = new ToolStripMenuItem("キーボード自動識別 (内蔵JIS/外付けUS)", null, OnToggleAutoDetect)
            {
                Checked = RawInputReceiver.AutoDetectionEnabled
            };

            _altImeMenuItem = new ToolStripMenuItem("左右Alt空打ちIME切り替え", null, OnToggleAltIme)
            {
                Checked = KeyboardHook.AltImeEnabled
            };

            _startupMenuItem = new ToolStripMenuItem("Windows起動時に自動起動 (管理者権限)", null, OnToggleStartup)
            {
                Checked = StartupManager.IsEnabled()
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(_emulationMenuItem);
            contextMenu.Items.Add(_autoDetectMenuItem);
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

            UpdateTrayIcon();
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

        private void OnToggleAutoDetect(object? sender, EventArgs e)
        {
            RawInputReceiver.AutoDetectionEnabled = !RawInputReceiver.AutoDetectionEnabled;
            _autoDetectMenuItem.Checked = RawInputReceiver.AutoDetectionEnabled;
        }

        private void OnToggleAltIme(object? sender, EventArgs e)
        {
            KeyboardHook.AltImeEnabled = !KeyboardHook.AltImeEnabled;
            _altImeMenuItem.Checked = KeyboardHook.AltImeEnabled;
        }

        private void OnToggleStartup(object? sender, EventArgs e)
        {
            bool enable = !StartupManager.IsEnabled();
            StartupManager.SetEnabled(enable);
            _startupMenuItem.Checked = StartupManager.IsEnabled();
        }

        private void OnExit(object? sender, EventArgs e)
        {
            KeyboardHook.Stop();
            _rawInputReceiver.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _currentIcon?.Dispose();
            Application.Exit();
        }
    }
}
