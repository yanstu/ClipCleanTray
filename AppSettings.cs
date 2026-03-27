using System;
using System.IO;

namespace ClipCleanTray
{
    internal sealed class AppSettings
    {
        private static readonly string SettingsDirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppInfo.ProjectName);

        private static readonly string SettingsFilePath = Path.Combine(
            SettingsDirectoryPath,
            AppInfo.SettingsFileName);

        public bool TrimBoundaryWhitespace { get; set; }

        public bool PlainTextOnly { get; set; }

        public static AppSettings Load()
        {
            var settings = new AppSettings
            {
                TrimBoundaryWhitespace = true,
                PlainTextOnly = true
            };

            if (!File.Exists(SettingsFilePath))
            {
                return settings;
            }

            try
            {
                foreach (string line in File.ReadAllLines(SettingsFilePath))
                {
                    string entry = line.Trim();
                    if (entry.Length == 0 || entry.StartsWith("#"))
                    {
                        continue;
                    }

                    int separatorIndex = entry.IndexOf('=');
                    if (separatorIndex <= 0)
                    {
                        continue;
                    }

                    string key = entry.Substring(0, separatorIndex).Trim();
                    string value = entry.Substring(separatorIndex + 1).Trim();

                    bool enabled;
                    if (!bool.TryParse(value, out enabled))
                    {
                        continue;
                    }

                    switch (key)
                    {
                        case "TrimBoundaryWhitespace":
                            settings.TrimBoundaryWhitespace = enabled;
                            break;
                        case "PlainTextOnly":
                            settings.PlainTextOnly = enabled;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                // 配置读取失败时回退到默认值，避免影响托盘常驻。
            }

            return settings;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectoryPath);
                File.WriteAllLines(
                    SettingsFilePath,
                    new[]
                    {
                        "TrimBoundaryWhitespace=" + TrimBoundaryWhitespace,
                        "PlainTextOnly=" + PlainTextOnly
                    });
            }
            catch (Exception)
            {
                // 配置写入失败时保持静默，避免托盘操作打断用户。
            }
        }
    }
}
