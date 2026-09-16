using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ule4JisAlt
{
    public class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ToolStripMenuItem _emulationMenuItem;
        private readonly ToolStripMenuItem _modeExternalUsMenuItem;
        private readonly ToolStripMenuItem _modeExternalJisMenuItem;
        private readonly ToolStripMenuItem _autoDetectMenuItem;
        private readonly ToolStripMenuItem _altImeMenuItem;
        private readonly ToolStripMenuItem _startupMenuItem;
        private readonly RawInputReceiver _rawInputReceiver;

        private Icon? _currentIcon;

        public TrayApplicationContext()
        {
            SettingsManager.Load();

            _rawInputReceiver = new RawInputReceiver();

            _emulationMenuItem = new ToolStripMenuItem("エミュレーション有効", null, OnToggleEmulation)
            {
                Checked = KeyboardHook.EmulationEnabled
            };

            _modeExternalUsMenuItem = new ToolStripMenuItem("  外付けUS化 (PC本体: JIS)", null, OnSelectModeExternalUs)
            {
                Checked = KeyboardHook.CurrentLayoutMode == LayoutMode.ExternalUs,
                Enabled = KeyboardHook.EmulationEnabled
            };

            _modeExternalJisMenuItem = new ToolStripMenuItem("  外付けJIS化 (PC本体: US)", null, OnSelectModeExternalJis)
            {
                Checked = KeyboardHook.CurrentLayoutMode == LayoutMode.ExternalJis,
                Enabled = KeyboardHook.EmulationEnabled
            };

            _autoDetectMenuItem = new ToolStripMenuItem("キーボード自動識別 (内蔵 / 外付け切り替え)", null, OnToggleAutoDetect)
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
            contextMenu.Items.Add(_modeExternalUsMenuItem);
            contextMenu.Items.Add(_modeExternalJisMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator());
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
            _currentIcon = IconGenerator.CreatePixelKeyIcon(KeyboardHook.EmulationEnabled, KeyboardHook.CurrentLayoutMode);
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
            _modeExternalUsMenuItem.Enabled = KeyboardHook.EmulationEnabled;
            _modeExternalJisMenuItem.Enabled = KeyboardHook.EmulationEnabled;
            UpdateTrayIcon();
            SettingsManager.Save();
        }

        private void OnSelectModeExternalUs(object? sender, EventArgs e)
        {
            KeyboardHook.CurrentLayoutMode = LayoutMode.ExternalUs;
            _modeExternalUsMenuItem.Checked = true;
            _modeExternalJisMenuItem.Checked = false;

            // 外付けUS化モード時はAlt空打ちIME切り替えを有効化
            KeyboardHook.AltImeEnabled = true;
            _altImeMenuItem.Checked = true;

            UpdateTrayIcon();
            SettingsManager.Save();
        }

        private void OnSelectModeExternalJis(object? sender, EventArgs e)
        {
            KeyboardHook.CurrentLayoutMode = LayoutMode.ExternalJis;
            _modeExternalUsMenuItem.Checked = false;
            _modeExternalJisMenuItem.Checked = true;

            // 外付けJIS化モード時は物理キー（変換・無変換）があるためAlt空打ちを自動的にOFF
            KeyboardHook.AltImeEnabled = false;
            _altImeMenuItem.Checked = false;

            UpdateTrayIcon();
            SettingsManager.Save();
        }

        private void OnToggleAutoDetect(object? sender, EventArgs e)
        {
            RawInputReceiver.AutoDetectionEnabled = !RawInputReceiver.AutoDetectionEnabled;
            _autoDetectMenuItem.Checked = RawInputReceiver.AutoDetectionEnabled;
            SettingsManager.Save();
        }

        private void OnToggleAltIme(object? sender, EventArgs e)
        {
            KeyboardHook.AltImeEnabled = !KeyboardHook.AltImeEnabled;
            _altImeMenuItem.Checked = KeyboardHook.AltImeEnabled;
            SettingsManager.Save();
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
