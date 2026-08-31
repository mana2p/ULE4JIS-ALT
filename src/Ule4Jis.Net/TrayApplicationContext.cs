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
        private readonly ToolStripMenuItem _capsLockMenu;
        private readonly ToolStripMenuItem _capsImeToggleMenuItem;
        private readonly ToolStripMenuItem _capsDisabledMenuItem;
        private readonly ToolStripMenuItem _startupMenuItem;

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

            _capsImeToggleMenuItem = new ToolStripMenuItem("IMEをトグル切り替え (ON/OFF)", null, (s, e) => SetCapsLockMode(CapsLockMode.ImeToggle))
            {
                Checked = (AltImeSwitcher.CurrentCapsLockMode == CapsLockMode.ImeToggle)
            };

            _capsDisabledMenuItem = new ToolStripMenuItem("無効 (通常のCapsLock)", null, (s, e) => SetCapsLockMode(CapsLockMode.Disabled))
            {
                Checked = (AltImeSwitcher.CurrentCapsLockMode == CapsLockMode.Disabled)
            };

            _capsLockMenu = new ToolStripMenuItem("CapsLockの動作");
            _capsLockMenu.DropDownItems.Add(_capsImeToggleMenuItem);
            _capsLockMenu.DropDownItems.Add(_capsDisabledMenuItem);

            _startupMenuItem = new ToolStripMenuItem("Windows起動時に自動起動", null, OnToggleStartup)
            {
                Checked = IsStartupEnabled()
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(_emulationMenuItem);
            contextMenu.Items.Add(_altImeMenuItem);
            contextMenu.Items.Add(_capsLockMenu);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(_startupMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("終了 (&X)", null, OnExit);

            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "ULE4JIS + Alt-IME (.NET 9)",
                ContextMenuStrip = contextMenu,
                Visible = true
            };

            // フック開始
            KeyboardHook.Start();
        }

        private void SetCapsLockMode(CapsLockMode mode)
        {
            AltImeSwitcher.CurrentCapsLockMode = mode;
            _capsImeToggleMenuItem.Checked = (mode == CapsLockMode.ImeToggle);
            _capsDisabledMenuItem.Checked = (mode == CapsLockMode.Disabled);
        }

        private void OnToggleEmulation(object? sender, EventArgs e)
        {
            KeyboardHook.EmulationEnabled = !KeyboardHook.EmulationEnabled;
            _emulationMenuItem.Checked = KeyboardHook.EmulationEnabled;
        }

        private void OnToggleAltIme(object? sender, EventArgs e)
        {
            KeyboardHook.AltImeEnabled = !KeyboardHook.AltImeEnabled;
            _altImeMenuItem.Checked = KeyboardHook.AltImeEnabled;
            _capsLockMenu.Enabled = KeyboardHook.AltImeEnabled;
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
            Application.Exit();
        }
    }
}
