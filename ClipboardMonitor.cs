using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClipCleanTray
{
    /// <summary>
    /// 剪贴板监听器 - 监听剪贴板变化并按当前开关规则清理文本
    /// </summary>
    internal class ClipboardMonitor : NativeWindow, IDisposable
    {
        // Win32 API 常量
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private static readonly HashSet<string> PlainTextFormats = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            DataFormats.Text,
            DataFormats.UnicodeText,
            DataFormats.OemText,
            DataFormats.StringFormat,
            DataFormats.Locale
        };

        // Win32 API 声明
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        private readonly AppSettings _settings;

        // 防止循环触发的标志
        private bool _isProcessing;
        private bool _disposed;

        public ClipboardMonitor(AppSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

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

                if (!_settings.TrimBoundaryWhitespace && !_settings.PlainTextOnly)
                {
                    return;
                }

                // 检查剪贴板是否包含文本
                if (!Clipboard.ContainsText())
                {
                    return;
                }

                string originalText = Clipboard.GetText(TextDataFormat.UnicodeText);
                if (originalText == null)
                {
                    return;
                }

                string processedText = originalText;
                bool shouldUpdateClipboard = false;

                if (_settings.TrimBoundaryWhitespace)
                {
                    string trimmedText = processedText.Trim();
                    if (!string.Equals(trimmedText, processedText, StringComparison.Ordinal))
                    {
                        processedText = trimmedText;
                        shouldUpdateClipboard = true;
                    }
                }

                if (_settings.PlainTextOnly && ContainsNonPlainTextFormats(Clipboard.GetDataObject()))
                {
                    shouldUpdateClipboard = true;
                }

                if (shouldUpdateClipboard)
                {
                    ReplaceClipboardText(processedText);
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

        private static bool ContainsNonPlainTextFormats(IDataObject dataObject)
        {
            if (dataObject == null)
            {
                return false;
            }

            foreach (string format in dataObject.GetFormats(false))
            {
                if (!PlainTextFormats.Contains(format))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ReplaceClipboardText(string text)
        {
            if (text.Length == 0)
            {
                Clipboard.Clear();
                return;
            }

            Clipboard.SetText(text, TextDataFormat.UnicodeText);
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
