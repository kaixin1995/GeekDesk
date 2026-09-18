# GeekDesk - Sonic Launcher Custom Edition

> 🚀 **Forked and heavily customized by [kaixin1995](https://github.com/kaixin1995) based on the original [GeekDesk (BookerLiu)](https://github.com/BookerLiu/GeekDesk).**
> Fully upgraded to **.NET 8.0**, bringing the iconic **Sonic Launcher / VStart** workflow back to Windows 10/11 with dual launcher modes, three high-contrast visual themes, modern JSON persistence, and native Win32 interop.
>
> 🙏 **Special thanks to the original author [BookerLiu](https://github.com/BookerLiu)**: this project started from the original GeekDesk. Although the code has been largely rewritten, the original design and foundation remain the root of this project.

- [中文说明](README-zh.md)
- [English Documentation](README.md)

---

## 🌟 Major Improvements in This Fork

### 1. Modern .NET 8.0 SDK Architecture
- Upgraded target framework from legacy `.NET Framework 4.7.2` to **`.NET 8.0` (`net8.0-windows7.0`)**.
- Clean SDK-style project format with Tiered Compilation optimizations enabled.
- Replaced legacy COM `IWshRuntimeLibrary` dependencies with native Win32 `IShellLinkW` / `IPersistFile` COM interfaces.
- Zero warnings, zero errors in both Debug and Release builds.

### 2. Dual Launcher Modes, One-Click Switch
- **Sonic Drawer (centered accordion drawers)**:
  - Category drawers stacked vertically with smooth, mutually exclusive expansion; the expanded drawer fills the viewport.
  - Flowing icon layout with smart ellipsis; drop external files to add items; linked folders sync in real time.
- **XiaoQ Desk card mode (default)**:
  - Left category navigation + right large icon grid; pops up centered right above the taskbar with a bottom-up slide animation.
  - Per-Monitor V2 DPI aware and multi-screen aware — always pops up on the screen where the mouse is.

### 3. Three High-Contrast Visual Themes
- **macOS Acrylic** (default): professional dark space-gray with Apple-blue highlight capsules.
- **Pure Minimal**: clean all-white surfaces with hairline borders.
- **Crystal Skeuomorphic**: classic electroplated-silver gradients with ivory warm tones.
- Classic red/yellow crystal titlebar buttons (hover reveals `✕` / `─`): 🔴 red closes/hides the panel, 🟡 yellow minimizes/retracts to the edge.

### 4. High-Performance JSON Storage & Crash Protection
- Replaced the deprecated `BinaryFormatter` with clean, efficient `Newtonsoft.Json` storage.
- Debounced (500 ms) and re-entrancy-protected persistence to prevent recursive save deadlocks or `StackOverflowException`.
- Automated daily rolling backups (7-day retention by default) with automatic disaster recovery from backups.

### 5. Instant Local Search & Utilities
- Fast keyword and **Pinyin initial letter** filtering (Everything removed in favor of pure local search; floating search bar via button or `Ctrl+F`).
- Scheduled Todo tasks with desktop toast notifications, a dedicated window, and a global hotkey.
- Integrated bottom shortcuts: Task Manager, Control Panel, Calculator, and Todo.
- Extra polish: screen-centered icon property editor, smart cleanup of "- Shortcut" suffixes, an icon font-size slider, top-right global notifications, a screen color picker, and more.
- **Clean & local**: the original auto-update checker, UUID telemetry, and donation QR codes have been removed; the app no longer contacts any remote server in daily use.

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
| **Global Mouse Hook** | MouseKeyHook `5.6.0` |

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

- Licensed under the [Apache License 2.0](LICENSE) (inherited from the original project).
- **Special thanks to [BookerLiu](https://github.com/BookerLiu) and the original [GeekDesk](https://github.com/BookerLiu/GeekDesk) project**: this fork is built on top of the original code and architecture — none of this would exist without it.
- Thanks to the open-source community behind [HandyControl](https://github.com/HandyOrg/HandyControl), [XamlFlair](https://github.com/XamlFlair/XamlFlair), and more.
