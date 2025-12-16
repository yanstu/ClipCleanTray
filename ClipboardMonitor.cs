using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TrimTray
{
    /// <summary>
    /// 剪贴板监听器 - 监听剪贴板变化并自动去除文本首尾空白
    /// </summary>
    public class ClipboardMonitor : NativeWindow, IDisposable
    {
        // Win32 API 常量
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        // Win32 API 声明
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        // 防止循环触发的标志
        private bool _isProcessing;
        private bool _disposed;

        public ClipboardMonitor()
        {
            // 创建隐藏窗口用于接收消息
            CreateHandle(new CreateParams());

            // 注册剪贴板监听
            if (!AddClipboardFormatListener(Handle))
            {
                int error = Marshal.GetLastWin32Error();
                System.Diagnostics.Debug.WriteLine($"AddClipboardFormatListener failed with error: {error}");
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                OnClipboardChanged();
            }
            base.WndProc(ref m);
        }

        private void OnClipboardChanged()
        {
            // 防止循环触发
            if (_isProcessing) return;

            try
            {
                _isProcessing = true;

                // 检查剪贴板是否包含文本
                if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText();

                    // 检查是否需要处理（首尾有空白字符）
                    if (!string.IsNullOrEmpty(text))
                    {
                        string trimmed = text.Trim();

                        // 只有当确实有变化时才更新剪贴板
                        if (trimmed != text)
                        {
                            // 设置处理后的文本
                            Clipboard.SetText(trimmed);
                        }
                    }
                }
            }
            catch (ExternalException)
            {
                // 剪贴板被其他程序占用，忽略
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Clipboard processing error: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (Handle != IntPtr.Zero)
            {
                RemoveClipboardFormatListener(Handle);
                DestroyHandle();
            }

            _disposed = true;
        }

        ~ClipboardMonitor()
        {
            Dispose(false);
        }
    }
}
