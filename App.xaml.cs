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
        System.Threading.Mutex mutex;

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

        private void App_Startup(object sender, StartupEventArgs e)
        {
            mutex = new System.Threading.Mutex(true, Constants.MY_NAME, out bool isNewInstance);
            if (!isNewInstance)
            {
                // 如果已经存在运行中的实例，向其发送 ShowApp 激活呼出指令，并直接退出当前进程
                MessageUtil.SendMsgByWName("GeekDesk_Main_" + Constants.MY_UUID, "ShowApp");
                MessageUtil.SendMsgByPName(Constants.MY_NAME, "ShowApp");
                Environment.Exit(0);
            }
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
