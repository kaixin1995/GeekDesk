using GeekDesk.Constant;
using GeekDesk.MyThread;
using GeekDesk.Util;
using GeekDesk.ViewModel;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace GeekDesk
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        static System.Threading.Mutex mutex;

        private void App_Startup(object sender, StartupEventArgs e)
        {
            TryAcquireSingleInstance();
        }

        /// <summary>
        /// 单实例判定: 已有实例可唤醒则唤醒退出; 联系不上(已有实例正在退出, 如睡眠唤醒
        /// 自动重启的竞态窗口)则等待接管, 避免双实例双托盘图标或"点了没反应程序消失"
        /// </summary>
        private void TryAcquireSingleInstance()
        {
            mutex = new System.Threading.Mutex(true, Constants.MY_NAME, out bool isNewInstance);
            if (isNewInstance) return;

            //已存在实例: 尝试唤醒后退出
            bool awakened = MessageUtil.SendMsgByWName(Constants.MAIN_WINDOW_TITLE, "ShowApp")
                  | MessageUtil.SendMsgByPName(Constants.MY_NAME, "ShowApp");
            if (awakened)
            {
                Environment.Exit(0);
                return;
            }

            //唤醒失败但互斥锁仍被占用: 已有实例多半正处于退出过程中,
            //等待其退出后接管单实例 (最多约5秒)
            mutex.Dispose();
            mutex = null;
            for (int i = 0; i < 50; i++)
            {
                System.Threading.Thread.Sleep(100);
                mutex = new System.Threading.Mutex(true, Constants.MY_NAME, out isNewInstance);
                if (isNewInstance)
                {
                    LogUtil.WriteLog("等待后接管单实例 (前一实例退出竞态)");
                    return;
                }
                mutex.Dispose();
                mutex = null;
            }
            //仍联系不上也接管不了: 异常状态, 退出以防双实例
            Environment.Exit(0);
        }

        /// <summary>
        /// 重启场景下先释放单实例互斥锁 (在UI线程执行, 与获取线程保持亲和)
        /// </summary>
        public static void ReleaseSingleInstanceMutex()
        {
            if (Application.Current == null) return;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try { mutex?.ReleaseMutex(); } catch (Exception) { }
                try { mutex?.Dispose(); } catch (Exception) { }
                mutex = null;
            }));
        }

        public App()
        {
            // 配置 .NET 8 程序集动态解析，兼容 lib 路径
            AssemblyLoadContext.Default.Resolving += (context, assemblyName) =>
            {
                string[] searchDirs = new string[]
                {
                    AppDomain.CurrentDomain.BaseDirectory,
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib")
                };

                foreach (string dir in searchDirs)
                {
                    if (Directory.Exists(dir))
                    {
                        string path = Path.Combine(dir, $"{assemblyName.Name}.dll");
                        if (File.Exists(path))
                        {
                            return context.LoadFromAssemblyPath(path);
                        }
                    }
                }
                return null;
            };

            this.Startup += new StartupEventHandler(App_Startup);
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            //分辨率/缩放/显示器拓扑变更: 主动恢复窗口几何, 防止面板困在屏幕外或贴边状态失效
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        }

        // 电源监听
        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    LogUtil.WriteLog("System resumed from sleep.");
                    ProcessUtil.ReStartApp();
                    break;
                case PowerModes.Suspend:
                    LogUtil.WriteLog("System is going to sleep.");
                    break;
            }
        }

        /// <summary>
        /// 显示设置变更 (分辨率/缩放/增删显示器): 清除位置动画残留,
        /// 贴边隐藏状态按新几何重评, 屏幕外窗口拉回可视区, 并强制重建呈现
        /// </summary>
        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    GeekDesk.MainWindow w = GeekDesk.MainWindow.mainWindow;
                    if (w == null) return;

                    //清掉可能持有 Left/Top 的旧动画时钟
                    w.BeginAnimation(Window.LeftProperty, null);
                    w.BeginAnimation(Window.TopProperty, null);

                    if (MarginHide.IS_HIDE)
                    {
                        //按新分辨率重新滑出并重评停靠状态
                        MarginHide.ForceShowFromMargin();
                    }
                    else if (w.Visibility == Visibility.Visible)
                    {
                        //窗口大部分落在可视区之外时拉回 (分辨率缩小/移除显示器)
                        double vw = SystemParameters.VirtualScreenWidth;
                        double vh = SystemParameters.VirtualScreenHeight;
                        double vl = SystemParameters.VirtualScreenLeft;
                        double vt = SystemParameters.VirtualScreenTop;
                        bool outside = w.Left > vl + vw - 40
                                    || w.Left < vl - w.Width + 40
                                    || w.Top > vt + vh - 40
                                    || w.Top < vt - w.Height + 40;
                        if (outside)
                        {
                            double nl = Math.Max(vl, Math.Min(w.Left, vl + vw - w.Width));
                            double nt = Math.Max(vt, Math.Min(w.Top, vt + vh - w.Height));
                            if (nl + w.Width > vl + vw) nl = vl;
                            if (nt + w.Height > vt + vh) nt = vt;
                            w.Left = nl;
                            w.Top = nt;
                        }
                    }

                    //强制分层窗口重建呈现 (缩放变更后表面可能失效)
                    WindowUtil.ForceRefreshSurface(w);
                    LogUtil.WriteLog("显示设置变更: 已重评窗口几何 Vis=" + w.Visibility
                        + " IS_HIDE=" + MarginHide.IS_HIDE
                        + " Pos=(" + (int)w.Left + "," + (int)w.Top + ")");
                }
                catch (Exception ex)
                {
                    LogUtil.WriteErrorLog(ex, "处理显示设置变更异常");
                }
            }));
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Mouse.OverrideCursor = null;
            LogUtil.WriteErrorLog(e, "未捕获异常!");
            if (Constants.DEV)
            {
                MessageBox.Show("GeekDesk遇到一个问题, 不用担心, 这不影响其它操作!");
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //记录真正的异常对象 (EventArgs 本身不含堆栈信息)
            LogUtil.WriteErrorLog(e.ExceptionObject, "严重异常!");
        }

        public static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(
                    delegate (object f)
                    {
                        ((DispatcherFrame)f).Continue = false;
                        return null;
                    }), frame);
            Dispatcher.PushFrame(frame);
        }
    }
}
