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
            LogUtil.WriteErrorLog(e, "严重异常!");
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
