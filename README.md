# 剪贴板净化器

剪贴板净化器是一个轻量级 Windows 托盘工具，用于在复制文本时自动完成常见清理工作。它提供两个可独立切换的处理规则：清理首尾空白、转换为纯文本，让复制出来的内容更干净、更适合继续粘贴使用。

## 核心能力

- **清理首尾空白**：按需去除复制内容前后的空格、制表符和换行。
- **转换为纯文本**：去掉富文本、HTML、Word 样式等格式信息，仅保留文本内容。
- **托盘即时切换**：所有清理规则都可直接在托盘菜单中开关，不再强制默认生效。
- **常驻后台**：启动后常驻系统托盘，不占任务栏。
- **开机自启**：支持通过托盘菜单启用或关闭开机启动。

## 适用场景

- 从网页、文档或聊天工具复制内容时，不想把格式一并带进目标应用。
- 临时需要保留原始文本，想先关闭清理规则，再继续复制。
- 希望常驻一个小工具，而不是每次手动执行“选择性粘贴为纯文本”。

## 系统要求

- Windows 10 / 11
- .NET Framework 4.8

## 使用方式

1. 运行 `ClipCleanTray.exe`
2. 程序会最小化到系统托盘
3. 右键托盘图标，可以切换：
   - `清理首尾空白`
   - `转换为纯文本`
   - `开机自启动`
4. 之后每次复制文本时，程序会按当前启用的规则处理剪贴板内容

默认情况下，两项文本清理规则都会开启；如果你临时需要保留原始内容，可以在托盘菜单里随时关闭。

## 配置存储

- 程序会把托盘开关状态写入 `%AppData%\ClipCleanTray\settings.ini`
- 重启软件后会自动恢复上一次的开关状态

## 编译

```powershell
MSBuild ClipCleanTray.csproj -p:Configuration=Release
```

输出文件位于 `bin\Release\ClipCleanTray.exe`

## 实现概览

- **剪贴板监听**：使用 Win32 API `AddClipboardFormatListener`
- **托盘菜单**：使用 WinForms `NotifyIcon` 和 `ContextMenuStrip`
- **纯文本降级**：检测到非纯文本格式后，回写 Unicode 文本到剪贴板
- **配置持久化**：使用 `%AppData%` 下的轻量 `settings.ini`
- **单实例运行**：使用 `Mutex` 防止重复启动

## 项目结构

```text
ClipCleanTray/
├── Program.cs
├── AppInfo.cs
├── AppSettings.cs
├── TrayApplicationContext.cs
├── ClipboardMonitor.cs
├── AutoStartManager.cs
└── Resources/
    └── icon.ico
```

## 许可证

MIT License
