using System;
using Microsoft.Win32;

namespace ClipCleanTray
{
    /// <summary>
    /// 开机自启动管理器 - 通过注册表管理程序自启动
    /// </summary>
    public static class AutoStartManager
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = AppInfo.AutoStartEntryName;

        /// <summary>
        /// 检查是否已启用开机自启动
        /// </summary>
        public static bool IsAutoStartEnabled
        {
            get
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false))
                    {
                        if (key == null) return false;
                        string value = key.GetValue(AppName) as string;
                        return !string.IsNullOrEmpty(value);
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 启用开机自启动
        /// </summary>
        public static bool Enable()
        {
            try
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key == null) return false;
                    key.SetValue(AppName, $"\"{exePath}\"");
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 禁用开机自启动
        /// </summary>
        public static bool Disable()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key == null) return false;
                    if (key.GetValue(AppName) != null)
                    {
                        key.DeleteValue(AppName, false);
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 切换开机自启动状态
        /// </summary>
        public static bool Toggle()
        {
            if (IsAutoStartEnabled)
            {
                return Disable();
            }
            else
            {
                return Enable();
            }
        }
    }
}
