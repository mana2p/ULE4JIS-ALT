using System;
using System.IO;
using System.Text.Json;

namespace Ule4JisAlt
{
    public class AppSettings
    {
        public bool EmulationEnabled { get; set; } = true;
        public bool AutoDetectionEnabled { get; set; } = true;
        public bool AltImeEnabled { get; set; } = true;
    }

    internal static class SettingsManager
    {
        private static readonly string SettingsDirPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ULE4JIS-ALT");

        private static readonly string SettingsFilePath = Path.Combine(SettingsDirPath, "settings.json");

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static void Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                    if (settings != null)
                    {
                        KeyboardHook.EmulationEnabled = settings.EmulationEnabled;
                        RawInputReceiver.AutoDetectionEnabled = settings.AutoDetectionEnabled;
                        KeyboardHook.AltImeEnabled = settings.AltImeEnabled;
                        return;
                    }
                }
            }
            catch { }

            // ファイルが存在しない、または読み込み失敗時はデフォルト値で保存作成
            Save();
        }

        public static void Save()
        {
            try
            {
                if (!Directory.Exists(SettingsDirPath))
                {
                    Directory.CreateDirectory(SettingsDirPath);
                }

                var settings = new AppSettings
                {
                    EmulationEnabled = KeyboardHook.EmulationEnabled,
                    AutoDetectionEnabled = RawInputReceiver.AutoDetectionEnabled,
                    AltImeEnabled = KeyboardHook.AltImeEnabled
                };

                string json = JsonSerializer.Serialize(settings, JsonOptions);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch { }
        }
    }
}
