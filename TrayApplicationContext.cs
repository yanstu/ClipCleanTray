using System;
using System.Drawing;
using System.Windows.Forms;

namespace TrimTray
{
    /// <summary>
    /// 托盘应用上下文 - 管理系统托盘图标和菜单
    /// </summary>
    public class TrayApplicationContext : ApplicationContext
    {
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;
        private ToolStripMenuItem _autoStartMenuItem;
        private ClipboardMonitor _clipboardMonitor;

        public TrayApplicationContext()
        {
            InitializeComponents();
            StartClipboardMonitor();
        }

        private void InitializeComponents()
        {
            // 创建右键菜单
            _contextMenu = new ContextMenuStrip();

            // 开机自启动选项
            _autoStartMenuItem = new ToolStripMenuItem("开机自启动");
            _autoStartMenuItem.Click += AutoStartMenuItem_Click;
            UpdateAutoStartMenuState();
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
                Text = "TrimTray - 自动去除剪贴板首尾空格",
                ContextMenuStrip = _contextMenu,
                Visible = true
            };

            // 双击托盘图标也显示菜单
            _notifyIcon.DoubleClick += (s, e) => _contextMenu.Show(Cursor.Position);
        }

        private void StartClipboardMonitor()
        {
            _clipboardMonitor = new ClipboardMonitor();
        }

        private void UpdateAutoStartMenuState()
        {
            _autoStartMenuItem.Checked = AutoStartManager.IsAutoStartEnabled;
        }

        private void AutoStartMenuItem_Click(object sender, EventArgs e)
        {
            AutoStartManager.Toggle();
            UpdateAutoStartMenuState();
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
                using (var stream = assembly.GetManifestResourceStream("TrimTray.Resources.icon.ico"))
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
