using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;

namespace GeekDesk.Util
{
    /// <summary>
    /// 托盘图标自愈器。
    /// HandyControl NotifyIcon 的 Init()(由 Loaded 触发)每次执行都会新建消息窗口并
    /// 重复注册图标, 且旧窗口永不销毁、旧图标因句柄更换而无法删除——Loaded 重入后
    /// 同进程出现多个真实托盘图标(退出一个全部消失)。
    /// 本器定期清点本进程 NotifyIcon 消息窗口, 发现重复注册时:
    /// 保留控件当前使用的窗口与其图标, 清除并销毁其余全部泄漏窗口。
    /// </summary>
    public static class TrayIconSanitizer
    {
        private const string WINDOW_CLASS_PREFIX = "HandyControl.Controls.NotifyIcon";
        private const uint NIM_DELETE = 2;
        /// <summary>图标 uID 尝试上限 (NotifyIcon 的静态自增 ID, 正常远小于此值)</summary>
        private const uint MAX_ICON_ID = 16;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NOTIFYICONDATA
        {
            public IntPtr hWnd;
            public uint uID;
            public uint uFlags;
            public uint uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public uint dwState;
            public uint dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public uint uVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public uint dwInfoFlags;
            public Guid guidItem;
            public IntPtr hBalloonIcon;
        }

        /// <summary>
        /// 清点并自愈 (在UI线程调用)
        /// </summary>
        /// <param name="notifyIcon">托盘控件实例</param>
        public static void CheckAndHeal(HandyControl.Controls.NotifyIcon notifyIcon)
        {
            try
            {
                List<IntPtr> windows = EnumSelfNotifyIconWindows();
                if (windows.Count <= 1) return;

                //读取控件当前使用的消息窗口句柄 (反射私有字段, 版本锁定 3.5.1, 失败则本轮跳过)
                IntPtr current = GetCurrentWindowHandle(notifyIcon);
                if (current == IntPtr.Zero) return;

                int removed = 0;
                foreach (IntPtr hwnd in windows)
                {
                    if (hwnd == current) continue;

                    //清除该泄漏窗口注册的图标 (hWnd+uID 组合唯一定位, 尝试有限ID区间, 误组合无副作用)
                    for (uint id = 1; id <= MAX_ICON_ID; id++)
                    {
                        NOTIFYICONDATA data = new NOTIFYICONDATA
                        {
                            hWnd = hwnd,
                            uID = id
                        };
                        Shell_NotifyIcon(NIM_DELETE, ref data);
                    }

                    //销毁泄漏窗口 (均在UI线程创建, 当前即UI线程)
                    try { DestroyWindow(hwnd); } catch (Exception) { }
                    removed++;
                }

                if (removed > 0)
                {
                    LogUtil.WriteLog("托盘图标自愈: 清理 " + removed + " 个重复注册");
                }
            }
            catch (Exception ex)
            {
                LogUtil.WriteErrorLog(ex, "托盘图标自愈异常");
            }
        }

        /// <summary>
        /// 反射读取 NotifyIcon 当前使用的消息窗口句柄
        /// </summary>
        private static IntPtr GetCurrentWindowHandle(HandyControl.Controls.NotifyIcon notifyIcon)
        {
            try
            {
                if (notifyIcon == null) return IntPtr.Zero;
                FieldInfo fi = typeof(HandyControl.Controls.NotifyIcon).GetField("_messageWindowHandle",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (fi == null) return IntPtr.Zero;
                return (IntPtr)fi.GetValue(notifyIcon);
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// 枚举本进程全部 NotifyIcon 消息窗口
        /// </summary>
        private static List<IntPtr> EnumSelfNotifyIconWindows()
        {
            List<IntPtr> result = new List<IntPtr>();
            uint selfPid = (uint)Environment.ProcessId;
            EnumWindows((hWnd, lParam) =>
            {
                uint pid;
                GetWindowThreadProcessId(hWnd, out pid);
                if (pid == selfPid)
                {
                    StringBuilder sb = new StringBuilder(256);
                    if (GetClassName(hWnd, sb, 256) > 0
                        && sb.ToString().StartsWith(WINDOW_CLASS_PREFIX, StringComparison.Ordinal))
                    {
                        result.Add(hWnd);
                    }
                }
                return true;
            }, IntPtr.Zero);
            return result;
        }
    }
}

