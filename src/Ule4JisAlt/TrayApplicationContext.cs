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
        private readonly ToolStripMenuItem _altImeMenuItem;
        private readonly ToolStripMenuItem _startupMenuItem;

        private Icon? _currentIcon;
        private const string AppName = "ULE4JIS-ALT";
        private const string RegistryRunPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string TaskSchedulerName = "ULE4JIS-ALT";

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

            _startupMenuItem = new ToolStripMenuItem("Windows起動時に自動起動 (管理者権限)", null, OnToggleStartup)
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
            _startupMenuItem.Checked = IsStartupEnabled();
        }

        /// <summary>
        /// タスクスケジューラまたはレジストリに自動起動が登録されているかチェック
        /// </summary>
        private static bool IsStartupEnabled()
        {
            // 1. タスクスケジューラの最上位特権タスクをチェック
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

            // 2. 旧レジストリ Run キーのチェック
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, false);
                if (key?.GetValue(AppName) != null) return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// タスクスケジューラを使って最上位特権 (管理者権限・サイレント自動起動) タスクを全自動作成/削除
        /// UIPI (管理者権限のVSやターミナルでフックが無効化される問題) を完全防止！
        /// </summary>
        private static void SetStartup(bool enable)
        {
            string exePath = Application.ExecutablePath;

            // 古いレジストリ Run キーからのクリーンアップ
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, true);
                key?.DeleteValue(AppName, false);
            }
            catch { }

            if (enable)
            {
                // schtasks /create /tn "ULE4JIS-ALT" /tr "\"<exePath>\"" /sc onlogon /rl highest /f
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
                // schtasks /delete /tn "ULE4JIS-ALT" /f
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
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _currentIcon?.Dispose();
            Application.Exit();
        }
    }
}
