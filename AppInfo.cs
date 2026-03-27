namespace ClipCleanTray
{
    internal static class AppInfo
    {
        public const string ProjectName = "ClipCleanTray";
        public const string DisplayName = "剪贴板净化器";
        public const string Description = "常驻托盘的剪贴板文本清理工具";
        public const string MutexName = ProjectName + "_SingleInstance_Mutex";
        public const string AutoStartEntryName = ProjectName;
        public const string IconResourceName = ProjectName + ".Resources.icon.ico";
        public const string SettingsFileName = "settings.ini";
    }
}
