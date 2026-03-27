using System;
using System.Threading;
using System.Windows.Forms;

namespace ClipCleanTray
{
    static class Program
    {
        // 用于确保单实例运行的互斥锁
        private static Mutex _mutex;

        /// <summary>
        /// 应用程序的主入口点
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 检查是否已有实例在运行
            bool createdNew;
            _mutex = new Mutex(true, AppInfo.MutexName, out createdNew);

            if (!createdNew)
            {
                // 已有实例在运行，退出
                MessageBox.Show(AppInfo.DisplayName + " 已在运行中", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // 使用托盘应用上下文运行（无主窗口）
                Application.Run(new TrayApplicationContext());
            }
            finally
            {
                // 释放互斥锁
                _mutex?.ReleaseMutex();
                _mutex?.Dispose();
            }
        }
    }
}
