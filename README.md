# TrimTray

一个轻量级 Windows 托盘工具，自动去除剪贴板文本的首尾空格和换行符。

## 功能特性

- **自动处理** - 监听剪贴板变化，自动去除复制文本的首尾空白字符
- **常驻后台** - 最小化运行，不占用任务栏空间
- **托盘图标** - 在系统托盘区域显示，右键可操作
- **开机自启** - 支持设置开机自动启动
- **超轻量级** - 程序仅 12KB，无需额外运行时

## 系统要求

- Windows 10 / 11
- .NET Framework 4.8（系统自带）

## 使用方法

1. 运行 `TrimTray.exe`
2. 程序自动最小化到系统托盘
3. 复制任何文本时，首尾空格/换行会被自动去除
4. 右键托盘图标可以：
   - 切换开机自启动
   - 退出程序

## 编译

```bash
# 使用 MSBuild 编译
MSBuild TrimTray.csproj -p:Configuration=Release
```

输出文件位于 `bin\Release\TrimTray.exe`

## 技术实现

- **剪贴板监听** - 使用 Win32 API `AddClipboardFormatListener`
- **托盘图标** - 使用 WinForms `NotifyIcon`
- **开机自启** - 通过注册表 `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- **单实例** - 使用 `Mutex` 互斥锁确保只运行一个实例

## 项目结构

```
TrimTray/
├── Program.cs                # 程序入口
├── TrayApplicationContext.cs # 托盘应用上下文
├── ClipboardMonitor.cs       # 剪贴板监听器
├── AutoStartManager.cs       # 开机自启动管理
└── Resources/
    └── icon.ico              # 托盘图标
```

## 许可证

MIT License
