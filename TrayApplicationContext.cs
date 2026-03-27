using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ClipCleanTray
{
    /// <summary>
    /// 托盘应用上下文 - 管理系统托盘图标和菜单
    /// </summary>
    public class TrayApplicationContext : ApplicationContext
    {
        private readonly AppSettings _settings;
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;
        private ToolStripMenuItem _trimWhitespaceMenuItem;
        private ToolStripMenuItem _plainTextMenuItem;
        private ToolStripMenuItem _autoStartMenuItem;
        private ClipboardMonitor _clipboardMonitor;

        public TrayApplicationContext()
        {
            _settings = AppSettings.Load();
            InitializeComponents();
            StartClipboardMonitor();
        }

        private void InitializeComponents()
        {
            // 创建右键菜单
            _contextMenu = new ContextMenuStrip();

            _trimWhitespaceMenuItem = new ToolStripMenuItem("清理首尾空白");
            _trimWhitespaceMenuItem.Click += TrimWhitespaceMenuItem_Click;
            _contextMenu.Items.Add(_trimWhitespaceMenuItem);

            _plainTextMenuItem = new ToolStripMenuItem("转换为纯文本");
            _plainTextMenuItem.Click += PlainTextMenuItem_Click;
            _contextMenu.Items.Add(_plainTextMenuItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            // 开机自启动选项
            _autoStartMenuItem = new ToolStripMenuItem("开机自启动");
            _autoStartMenuItem.Click += AutoStartMenuItem_Click;
            _contextMenu.Items.Add(_autoStartMenuItem);

            // 分隔线
            _contextMenu.Items.Add(new ToolStripSeparator());

            // 退出选项
            var exitMenuItem = new ToolStripMenuItem("退出");
            exitMenuItem.Click += ExitMenuItem_Click;
            _contextMenu.Items.Add(exitMenuItem);

            // 创建托盘图标
            _notifyIcon = new NotifyIcon
            {
                Icon = CreateDefaultIcon(),
                ContextMenuStrip = _contextMenu,
                Visible = true
            };
            UpdateMenuStates();
            UpdateNotifyIconText();

            // 双击托盘图标也显示菜单
            _notifyIcon.DoubleClick += (s, e) => _contextMenu.Show(Cursor.Position);
        }

        private void StartClipboardMonitor()
        {
            _clipboardMonitor = new ClipboardMonitor(_settings);
        }

        private void UpdateMenuStates()
        {
            _trimWhitespaceMenuItem.Checked = _settings.TrimBoundaryWhitespace;
            _plainTextMenuItem.Checked = _settings.PlainTextOnly;
            _autoStartMenuItem.Checked = AutoStartManager.IsAutoStartEnabled;
        }

        private void UpdateNotifyIconText()
        {
            var enabledFeatures = new List<string>();
            if (_settings.TrimBoundaryWhitespace)
            {
                enabledFeatures.Add("首尾空白");
            }

            if (_settings.PlainTextOnly)
            {
                enabledFeatures.Add("纯文本");
            }

            string statusText = enabledFeatures.Count == 0
                ? "未启用清理规则"
                : "已启用" + string.Join("、", enabledFeatures);

            string trayText = AppInfo.DisplayName + " - " + statusText;
            _notifyIcon.Text = trayText.Length <= 63 ? trayText : AppInfo.DisplayName;
        }

        private void ToggleProcessingOption(Action<AppSettings> toggleAction)
        {
            toggleAction(_settings);
            _settings.Save();
            UpdateMenuStates();
            UpdateNotifyIconText();
        }

        private void TrimWhitespaceMenuItem_Click(object sender, EventArgs e)
        {
            ToggleProcessingOption(settings => settings.TrimBoundaryWhitespace = !settings.TrimBoundaryWhitespace);
        }

        private void PlainTextMenuItem_Click(object sender, EventArgs e)
        {
            ToggleProcessingOption(settings => settings.PlainTextOnly = !settings.PlainTextOnly);
        }

        private void AutoStartMenuItem_Click(object sender, EventArgs e)
        {
            AutoStartManager.Toggle();
            UpdateMenuStates();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            ExitApplication();
        }

        private void ExitApplication()
        {
            // 清理资源
            _clipboardMonitor?.Dispose();

            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();

            Application.Exit();
        }

        /// <summary>
        /// 加载托盘图标（从嵌入资源加载 icon.ico）
        /// </summary>
        private Icon CreateDefaultIcon()
        {
            // 从嵌入资源加载图标
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(AppInfo.IconResourceName))
                {
                    if (stream != null)
                    {
                        return new Icon(stream);
                    }
                }
            }
            catch { }

            // 后备：使用系统默认图标
            return SystemIcons.Application;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _clipboardMonitor?.Dispose();
                _notifyIcon?.Dispose();
                _contextMenu?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
