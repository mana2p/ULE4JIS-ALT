using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Ule4Jis.Net
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
        private const string AppName = "ULE4JIS-ALT";
        private const string RegistryRunPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string TaskSchedulerName = "ULE4JIS-ALT";

        public TrayApplicationContext()
        {
            // RawInput レシーバーの初期化
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
                Checked = IsStartupEnabled()
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
            bool enable = !IsStartupEnabled();
            SetStartup(enable);
            _startupMenuItem.Checked = IsStartupEnabled();
        }

        private static bool IsStartupEnabled()
        {
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/query /tn \"{TaskSchedulerName}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                process.Start();
                process.WaitForExit();
                if (process.ExitCode == 0) return true;
            }
            catch { }

            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, false);
                if (key?.GetValue(AppName) != null) return true;
            }
            catch { }

            return false;
        }

        private static void SetStartup(bool enable)
        {
            string exePath = Application.ExecutablePath;

            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, true);
                key?.DeleteValue(AppName, false);
            }
            catch { }

            if (enable)
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = $"/create /tn \"{TaskSchedulerName}\" /tr \"\\\"{exePath}\\\"\" /sc onlogon /rl highest /f",
                        Verb = "runas",
                        UseShellExecute = true
                    };
                    using var p = Process.Start(psi);
                    p?.WaitForExit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"自動起動の登録に失敗しました:\n{ex.Message}", "ULE4JIS-ALT", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = $"/delete /tn \"{TaskSchedulerName}\" /f",
                        Verb = "runas",
                        UseShellExecute = true
                    };
                    using var p = Process.Start(psi);
                    p?.WaitForExit();
                }
                catch { }
            }
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
