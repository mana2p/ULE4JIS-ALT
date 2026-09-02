using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Ule4JisAlt
{
    /// <summary>
    /// Windows 起動時の自動起動（タスクスケジューラ / レジストリ）を管理する
    /// </summary>
    internal static class StartupManager
    {
        private const string AppName = "ULE4JIS-ALT";
        private const string RegistryRunPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string TaskSchedulerName = "ULE4JIS-ALT";

        public static bool IsEnabled()
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

        public static void SetEnabled(bool enable)
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
    }
}
