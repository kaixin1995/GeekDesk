# GeekDesk - 音速启动重构定制版 (Sonic Launcher Edition)

> 🚀 **本项目由 [kaixin1995](https://github.com/kaixin1995) 基于原版 [GeekDesk (BookerLiu)](https://github.com/BookerLiu/GeekDesk) 进行二次开发与现代化深度重构。**  
> 全面升级至 **.NET 8.0**，完美复刻**经典音速启动（Sonic Launcher / VStart）**的视觉体验与操作手感，重构底层数据持久化架构与原生 Win32 互操作，带来极致流畅、无内存泄漏的高性能桌面快速启动体验。

- [中文说明](README-zh.md)
- [English Documentation](README.md)

---

## 🌟 二次开发重大重构与特性升级

### 1. 全面迁移至现代 .NET 8.0 SDK 架构
- **现代化运行时**：由旧版 `.NET Framework 4.7.2` 全面升级为现代 **`.NET 8` (`net8.0-windows7.0`)** SDK 格式。
- **极致性能与编译质量**：开启分层编译优化（Tiered Compilation），全面修复旧版 API 废弃与多义性，实现工程 **0 错误、0 警告** 纯净构建。
- **原生 Win32 互操作**：重写快捷方式解析模块，通过原生 Win32 `IShellLinkW` / `IPersistFile` 免注册调用，摆脱 COM 依赖限制。

### 2. 深度复刻经典音速启动（Sonic Launcher）交互
- **居中手风琴抽屉布局（Accordion Drawer）**：
  - 分类抽屉纵向堆叠，点击平滑互斥展开。
  - 当前展开项自适应填满中间视口，折叠项整齐停靠于上下两侧。
  - 纯白图标展示区，图标与文字紧凑排布、智能省略，支持多级分类、右键菜单与拖拽排版。
- **经典银灰水晶金属标题栏**：
  - 质感金属银灰水晶渐变。
  - 左上角经典红、黄、绿三色水晶圆钮，**鼠标移入即显现经典操作符号**：
    - 🔴 **红色**（悬停显现 `✕`）：关闭/隐藏主面板。
    - 🟡 **黄色**（悬停显现 `─`）：最小化/贴边收起。
    - 🟢 **绿色**（悬停显现 `＋`）：**左键直接进入设置窗口**，右键唤出快捷菜单。
- **屏幕边缘停靠 & 滑出呼出**：
  - 拖拽至屏幕边缘自动停靠并缩进隐藏；鼠标触碰屏幕边缘立即平滑滑出，移开自动缩回。
  - **彻底移除鼠标跟随的漂移抖动**，停靠操作更加稳健。

### 3. 数据持久化与容灾备份重构
- **安全高效的 JSON 存储**：彻底替换 .NET 8 中已废弃的 `BinaryFormatter`，采用纯净的 JSON 序列化，并标明 UI 瞬态属性忽略（`[JsonIgnore]`）。
- **防重入保存锁**：内置线程安全与防递归重入锁，彻底消除在属性联动保存时可能触发的 `StackOverflowException`。
- **自动灾备与恢复**：内置自动轮转备份，主数据文件损坏时可自动平滑恢复。

### 4. 现代化纯净白底设置面板
- **现代扁平卡片设计**：设置窗口重构为精致的白底扁平卡片风格，搭配柔和阴影与细腻边框。
- **全矢量 SVG 图标**：侧边栏（关于、外观与主题、动作与交互、系统选项）全面采用高清 SVG 矢量图标。
- **WPF 内存与渲染调优**：所有图像采用 `BitmapCacheOption.OnLoad` 与 `.Freeze()` 冻结机制，大幅减少 GDI 句柄与内存占用。

### 5. 极速过滤、Everything 全盘搜索与待办日程
- **实时拼音首字母检索**：输入关键字或拼音首字母秒级过滤已有项目。
- **Everything 插件无缝联动**：支持毫秒级全盘文件检索。
- **日程待办提醒**：支持定时与 CRON 表达式提醒，到期桌面气泡通知。
- **底部常用工具条**：集成任务管理器、控制面板、计算器、待办事项与一键锁屏。

---

## 🛠️ 技术栈与依赖版本

| 组件 / 依赖项 | 说明 |
|---|---|
| **目标框架** | `.NET 8.0` (`net8.0-windows7.0`) |
| **二次开发者** | **kaixin1995** |
| **上游原项目** | [BookerLiu/GeekDesk](https://github.com/BookerLiu/GeekDesk) |
| **UI 控件库** | HandyControl `3.5.1` + XamlFlair.WPF `1.2.13` |
| **数据序列化** | Newtonsoft.Json `13.0.3` |
| **任务调度** | Quartz `3.8.1` |
| **拼音匹配** | NPinyin.Core `3.0.0` |
| **全盘搜索引擎** | Everything SDK (x86 / x64 原生 DLL 输出) |

---

## 💻 源码构建与运行指南

### 环境要求
- Windows 10 / Windows 11 (x64 / x86)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 / VS Code / JetBrains Rider

### 构建指令
```bash
# 克隆仓库
git clone https://github.com/kaixin1995/GeekDesk.git
cd GeekDesk

# 使用 .NET 8 CLI 编译 Release 版本
dotnet build GeekDesk.csproj -c Release

# 启动运行
dotnet run --project GeekDesk.csproj
```

---

## 📄 鸣谢与开源许可

- 本项目基于 [MIT 许可证](LICENSE) 开源。
- 特别感谢原作者 [BookerLiu](https://github.com/BookerLiu) 创建的原版 GeekDesk 项目。
- 感谢 [HandyControl](https://github.com/HandyOrg/HandyControl)、[XamlFlair](https://github.com/XamlFlair/XamlFlair) 等开源社区提供的优秀组件。
