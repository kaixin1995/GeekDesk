using GeekDesk.Constant;
using GeekDesk.MyThread;
using GeekDesk.Util;
using GeekDesk.ViewModel;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GeekDesk.Control.Windows
{
    /// <summary>
    /// 托盘图标独立宿主窗口。
    /// 托盘图标从主窗口视觉树中分离: 主面板的显示/隐藏/贴边动画/尺寸微调等操作
    /// 不再触发 NotifyIcon 的 Loaded 重入, 从源头规避 HandyControl NotifyIcon
    /// 在 Loaded 重入时"新建窗口并重复注册图标、旧图标无法删除"的缺陷
    /// (表现为同进程出现多个真实托盘图标, 退出一个全部消失)。
    /// </summary>
    public partial class TrayHostWindow : Window
    {
        private static TrayHostWindow instance;

        /// <summary>
        /// 托盘图标自愈定时器: 定期清点本进程 NotifyIcon 消息窗口,
        /// 发现重复注册时清光图标并干净重建一个, 兜底任何未知触发路径
        /// </summary>
        private readonly DispatcherTimer healTimer;

        private TrayHostWindow()
        {
            InitializeComponent();
            this.DataContext = MainWindow.appData;
            BarIcon.Text = Constants.MY_NAME;

            healTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            healTimer.Tick += (s, e) => TrayIconSanitizer.CheckAndHeal(BarIcon);
            healTimer.Start();
        }

        /// <summary>
        /// 创建并显示托盘宿主 (只调用一次)
        /// </summary>
        public static void EnsureTray()
        {
            if (instance != null) return;
            instance = new TrayHostWindow();
            //Show 触发视觉树加载(NotifyIcon 注册图标), 随即隐藏窗口本身
            instance.Show();
            instance.Visibility = Visibility.Hidden;
#if DEBUG
            //测试钩子: 设置环境变量 GEEKDESK_TEST_TRAY_LEAK=1 时模拟 HandyControl
            //NotifyIcon 的 Loaded 重入(重复 Init 产生多窗口+多图标), 用于验证自愈器
            if (Environment.GetEnvironmentVariable("GEEKDESK_TEST_TRAY_LEAK") == "1")
            {
                System.Threading.ThreadPool.QueueUserWorkItem(_ =>
                {
                    System.Threading.Thread.Sleep(3000);
                    instance.Dispatcher.Invoke(() =>
                    {
                        instance.BarIcon.Init();
                        instance.BarIcon.Init();
                        LogUtil.WriteLog("[测试] 已模拟托盘图标重复注册 x2");
                    });
                });
            }
#endif
        }

        /// <summary>
        /// 托盘图标单击: 切换主面板显示
        /// </summary>
        private void NotifyIcon_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CheckShouldShowApp())
            {
                MainWindow.ShowApp();
            }
            else
            {
                MainWindow.HideApp();
            }
        }

        private void ShowPanel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowApp();
        }

        private void ColorPicker_Click(object sender, RoutedEventArgs e)
        {
            GlobalColorPickerWindow.CreateNoShow();
        }

        private void BakDataFile_Click(object sender, RoutedEventArgs e)
        {
            System.Threading.Thread t = new System.Threading.Thread(() =>
            {
                CommonCode.BakAppData();
            });
            t.SetApartmentState(System.Threading.ApartmentState.STA);
            t.Start();
        }

        private void CloseBarIcon_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.appData.AppConfig.ShowBarIcon = false;
        }

        private void Backlog_Click(object sender, RoutedEventArgs e)
        {
            ToDoWindow.Show();
        }

        private void OpenThisDir_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "Explorer.exe";
            p.StartInfo.Arguments = "/e,/select," + Constants.APP_DIR + Constants.MY_NAME + ".exe";
            p.Start();
        }

        private void ConfigApp_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow.Show(MainWindow.appData.AppConfig, MainWindow.mainWindow);
        }

        private void ReStartApp_Click(object sender, RoutedEventArgs e)
        {
            ProcessUtil.ReStartApp();
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.appData.AppConfig.SecondsWindow == true)
            {
                MouseHookThread.Dispose();
            }
            Application.Current.Shutdown();
        }
    }
}
