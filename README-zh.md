# GeekDesk - 音速启动重构定制版 (Sonic Launcher Edition)

> 🚀 **本项目由 [kaixin1995](https://github.com/kaixin1995) 基于原版 [GeekDesk (BookerLiu)](https://github.com/BookerLiu/GeekDesk) 进行二次开发与深度重构。**
> 全面升级至 **.NET 8.0**，复刻**经典音速启动（Sonic Launcher / VStart）**的视觉体验与操作手感，提供双启动器模式与三套高反差视觉主题，并重构了底层数据持久化架构与原生 Win32 互操作。
>
> 🙏 **特别致谢原作者 [BookerLiu](https://github.com/BookerLiu)**：本项目始于原版 GeekDesk，虽然当前代码已经大幅重构，但原版的架构思想与基础模块仍是本项目的根基。

- [中文说明](README-zh.md)
- [English Documentation](README.md)

---

## 🌟 二次开发重大重构与特性升级

### 1. 全面迁移至现代 .NET 8.0 SDK 架构
- **现代化运行时**：由旧版 `.NET Framework 4.7.2` 全面升级为现代 **`.NET 8` (`net8.0-windows7.0`)** SDK 格式。
- **极致性能与编译质量**：开启分层编译优化（Tiered Compilation），实现工程 **0 错误、0 警告** 纯净构建。
- **原生 Win32 互操作**：重写快捷方式解析模块，通过原生 Win32 `IShellLinkW` / `IPersistFile` 免注册调用，摆脱 COM 依赖限制。

### 2. 双启动器模式，一键切换
- **音速启动居中手风琴抽屉（Sonic Drawer）**：
  - 分类抽屉纵向堆叠，点击平滑互斥展开，当前展开项自适应填满中间视口。
  - 图标流式排布、智能省略，支持外部文件拖入添加与关联文件夹实时同步。
- **小Q书桌卡片模式（XiaoQ Desk，默认）**：
  - 左侧分类导航 + 右侧大图标网格，呼出时自动贴靠任务栏正上方居中，自下而上平滑弹出。
  - 自动感知多屏与 DPI 缩放（PerMonitor V2），永远弹在鼠标所在的屏幕。

### 3. 三套高反差视觉主题
- **macOS 磨砂质感**（默认）：深空灰专业风，苹果蓝高光胶囊。
- **纯净极简现代**：极致纯白平铺，极细线条与克制留白。
- **经典金属微拟物**：电镀银/拉丝渐变高光与复古象牙暖米色。
- 标题栏为经典红、黄水晶圆钮（鼠标移入显现 `✕` / `─` 符号）：🔴 红色关闭/隐藏面板，🟡 黄色最小化/贴边收起。

### 4. 数据持久化与容灾备份重构
- **安全高效的 JSON 存储**：彻底替换 .NET 中已废弃的 `BinaryFormatter`，采用纯净的 JSON 序列化。
- **防抖 + 防重入保存**：高频属性变更 500ms 防抖合并写入，内置线程安全与防递归重入锁。
- **自动灾备与恢复**：每日自动轮转备份（默认保留 7 天），主数据文件损坏时自动从备份平滑恢复。

### 5. 极速本地搜索与实用工具
- **实时拼音首字母检索**：输入关键字或拼音首字母即时过滤已有项目（已移除 Everything 依赖，改为纯本地搜索，独立搜索按钮 + `Ctrl+F` 呼出）。
- **日程待办提醒**：定时桌面气泡通知，独立待办窗口与全局快捷键。
- **底部常用工具条**：集成任务管理器、控制面板、计算器、待办事项。
- **更多细节优化**：图标属性编辑窗口屏幕全局居中、快捷方式“- 快捷方式”后缀智能清理、图标文字大小滑动条、屏幕右上角全局通知、屏幕拾色器等。
- **纯净本地运行**：已移除原版的自动更新检测、UUID 统计上报与赞赏二维码，日常运行不再请求任何远程服务器。

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
| **全局鼠标钩子** | MouseKeyHook `5.6.0` |

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

- 本项目基于 [Apache License 2.0](LICENSE) 开源（沿用原版许可证）。
- **特别感谢原作者 [BookerLiu](https://github.com/BookerLiu) 与原版 [GeekDesk](https://github.com/BookerLiu/GeekDesk) 项目**：本项目在其代码与架构基础上二次开发而来，没有原版就没有本项目的今天。
- 感谢 [HandyControl](https://github.com/HandyOrg/HandyControl)、[XamlFlair](https://github.com/XamlFlair/XamlFlair) 等开源社区提供的优秀组件。
