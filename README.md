# GeekDesk - Sonic Launcher Custom Edition

> 🚀 **Forked and customized by [kaixin1995](https://github.com/kaixin1995) based on the original [GeekDesk (BookerLiu)](https://github.com/BookerLiu/GeekDesk).**  
> Fully upgraded to **.NET 8.0**, bringing the iconic **Sonic Launcher / VStart** workflow back to Windows 10/11 with modern WPF architecture, high-performance JSON persistence, and zero compilation warnings.

- [中文说明](README-zh.md)
- [English Documentation](README.md)

---

## 🌟 Major Improvements in This Fork

### 1. Modern .NET 8.0 SDK Architecture
- Upgraded target framework from legacy `.NET Framework 4.7.2` to **`.NET 8.0` (`net8.0-windows7.0`)**.
- Clean SDK-style project format with Tiered Compilation optimizations enabled.
- Replaced legacy COM `IWshRuntimeLibrary` dependencies with native Win32 `IShellLinkW` / `IPersistFile` COM interfaces.
- Zero warnings, zero errors in both Debug and Release builds.

### 2. Authentic Sonic Launcher / VStart Experience
- **Accordion Drawer Layout**: Vertically stacked category drawers with exclusive smooth expansion that automatically fill the viewport.
- **Silver Metallic Crystal Titlebar**: 
  - Left-top crystal buttons with dynamic hover icons:
    - 🔴 **Red** (`✕`): Close / Hide main window.
    - 🟡 **Yellow** (`─`): Minimize / Retract to edge.
    - 🟢 **Green** (`＋`): **Left-click directly opens Settings**, right-click for context menu.
- **Smart Edge Docking**: Drag to screen edges to dock; touch screen edge to summon with smooth animation.
- **Removed mouse-follow drift**: Stable window placement and reliable edge docking.

### 3. High-Performance JSON Storage & Crash Protection
- Replaced deprecated `BinaryFormatter` with clean, efficient `Newtonsoft.Json` storage.
- Added re-entrancy lock protection in data persistence to prevent recursive save deadlocks or `StackOverflowException`.
- Automated rolling backups and disaster recovery.

### 4. Modern Settings Window & Vector Icons
- Clean white card layout with subtle borders and shadows.
- Full SVG vector icons across navigation items and toolbar actions.
- Memory and rendering optimization: `BitmapCacheOption.OnLoad` + `.Freeze()` for fast startup and minimal RAM footprint.

### 5. Instant Filtering, Everything Search & Utilities
- Fast keyword and **Pinyin initial letter** filtering.
- Native Everything integration for full-disk file search.
- Scheduled Todo tasks with toast notifications.
- Integrated bottom shortcuts: Task Manager, Control Panel, Calculator, and Lock Workstation.

---

## 🛠️ Tech Stack & Dependencies

| Component | Description |
|---|---|
| **Target Framework** | `.NET 8.0` (`net8.0-windows7.0`) |
| **Fork Author / Maintainer** | **kaixin1995** |
| **Upstream Original Project** | [BookerLiu/GeekDesk](https://github.com/BookerLiu/GeekDesk) |
| **UI Components** | HandyControl `3.5.1` + XamlFlair.WPF `1.2.13` |
| **Serialization** | Newtonsoft.Json `13.0.3` |
| **Task Scheduler** | Quartz `3.8.1` |
| **Pinyin Engine** | NPinyin.Core `3.0.0` |
| **Search Engine** | Everything SDK (x86/x64 native DLLs) |

---

## 💻 Build & Run

### Prerequisites
- Windows 10 / Windows 11 (x64 / x86)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 / VS Code / JetBrains Rider

### Commands
```bash
git clone https://github.com/kaixin1995/GeekDesk.git
cd GeekDesk

# Build Release
dotnet build GeekDesk.csproj -c Release

# Run
dotnet run --project GeekDesk.csproj
```

---

## 📄 License & Acknowledgments

- Licensed under the [MIT License](LICENSE).
- Special thanks to [BookerLiu](https://github.com/BookerLiu) for the original GeekDesk foundation.
- Thanks to the open-source community behind HandyControl, XamlFlair, and Everything.
