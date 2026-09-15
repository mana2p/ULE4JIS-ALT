using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Ule4JisAlt
{
    /// <summary>
    /// Windows 起動時の自動起動（タスクスケジューラ）を管理する
    /// </summary>
    internal static class StartupManager
    {
        private const string AppName = "ULE4JIS-ALT";
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

            return false;
        }

        public static void SetEnabled(bool enable)
        {
            string exePath = Application.ExecutablePath;

            if (enable)
            {
                try
                {
                    string safeExePath = exePath.Replace("'", "''");
                    string psCommand = $"$action = New-ScheduledTaskAction -Execute '{safeExePath}'; " +
                                      $"$trigger = New-ScheduledTaskTrigger -AtLogOn; " +
                                      $"$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit 0; " +
                                      $"Register-ScheduledTask -TaskName '{TaskSchedulerName}' -Action $action -Trigger $trigger -Settings $settings -RunLevel Highest -Force";

                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{psCommand}\"",
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
