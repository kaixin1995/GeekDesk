using GeekDesk.Constant;
using GeekDesk.MyThread;
using GeekDesk.Util;
using GeekDesk.ViewModel;
using ShowSeconds;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GeekDesk.Control.UserControls.Config
{
    /// <summary>
    /// OtherControl.xaml 的交互逻辑
    /// </summary>
    public partial class OtherControl : UserControl
    {
        public OtherControl()
        {
            InitializeComponent();
            this.Loaded += OtherControl_Loaded;

        }

        private void OtherControl_Loaded(object sender, RoutedEventArgs e)
        {
            Sort_Check();
        }

        private void SelfStartUpBox_Click(object sender, RoutedEventArgs e)
        {
            AppConfig appConfig = MainWindow.appData.AppConfig;
            RegisterUtil.SetSelfStarting(appConfig.SelfStartUp, Constants.MY_NAME);
        }

        /// <summary>
        /// 移动窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DragMove(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Window.GetWindow(this).DragMove();
            }
        }

        private void SortType_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            SortType type = (SortType)int.Parse(rb.Tag.ToString());

            SortType resType = type;
            switch (type)
            {
                case SortType.CUSTOM:
                    break;
                case SortType.COUNT_UP:
                    if (rb.IsChecked == true)
                    {
                        CountLowSort.IsChecked = true;
                        CountUpSort.Visibility = Visibility.Collapsed;
                        CountLowSort.Visibility = Visibility.Visible;
                        resType = SortType.COUNT_LOW;
                    }
                    break;
                case SortType.COUNT_LOW:
                    if (rb.IsChecked == true)
                    {
                        CountUpSort.IsChecked = true;
                        CountLowSort.Visibility = Visibility.Collapsed;
                        CountUpSort.Visibility = Visibility.Visible;
                        resType = SortType.COUNT_UP;
                    }
                    break;
                case SortType.NAME_UP:
                    if (rb.IsChecked == true)
                    {
                        NameLowSort.IsChecked = true;
                        NameUpSort.Visibility = Visibility.Collapsed;
                        NameLowSort.Visibility = Visibility.Visible;
                        resType = SortType.NAME_LOW;
                    }
                    break;
                case SortType.NAME_LOW:
                    if (rb.IsChecked == true)
                    {
                        NameUpSort.IsChecked = true;
                        NameLowSort.Visibility = Visibility.Collapsed;
                        NameUpSort.Visibility = Visibility.Visible;
                        resType = SortType.NAME_UP;
                    }
                    break;
            }
            MainWindow.appData.AppConfig.IconSortType = resType;
            CommonCode.SortIconList();
        }

        private void Sort_Check()
        {
            if (NameLowSort.IsChecked == true)
            {
                NameUpSort.Visibility = Visibility.Collapsed;
                NameLowSort.Visibility = Visibility.Visible;
            }
            if (CountLowSort.IsChecked == true)
            {
                CountUpSort.Visibility = Visibility.Collapsed;
                CountLowSort.Visibility = Visibility.Visible;
            }
        }

        private void BakDataFile(object sender, RoutedEventArgs e)
        {
            CommonCode.BakAppData();
        }

        /// <summary>
        /// 从备份文件恢复数据
        /// </summary>
        private void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "选择备份文件",
                    Filter = "bak文件(*.bak)|*.bak",
                };
                if (Directory.Exists(Constants.DATA_FILE_BAK_DIR_PATH))
                {
                    ofd.InitialDirectory = Constants.DATA_FILE_BAK_DIR_PATH;
                }
                if (ofd.ShowDialog() != true) return;

                AppData restored = CommonCode.DeserializeAppData(File.ReadAllText(ofd.FileName, System.Text.Encoding.UTF8));
                if (restored == null)
                {
                    HandyControl.Controls.Growl.WarningGlobal("备份文件内容无效, 恢复失败!");
                    return;
                }
                CommonCode.FixMenuHierarchy(restored.MenuList);

                //恢复前先把当前数据做一次安全备份
                Directory.CreateDirectory(Constants.DATA_FILE_BAK_DIR_PATH);
                string safetyPath = Path.Combine(Constants.DATA_FILE_BAK_DIR_PATH,
                    "pre-restore-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bak");
                CommonCode.SaveAppData(MainWindow.appData, safetyPath);

                //替换运行时数据并刷新界面
                MainWindow.appData = restored;
                MainWindow.mainWindow.DataContext = restored;
                MainWindow.mainWindow.AccordionCard.ReInit();
                MainWindow.mainWindow.XiaoQCard.ReloadFromData();
                MainWindow.mainWindow.ApplyLauncherStyle();
                MainWindow.mainWindow.ApplyThemeStyle();
                BGSettingUtil.BGSetting();

                HandyControl.Controls.Growl.SuccessGlobal("数据已从备份恢复! (恢复前的数据已另存为 " + Path.GetFileName(safetyPath) + ")");

                //设置窗口持有的旧配置已失效, 关闭待用户重新打开
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                LogUtil.WriteErrorLog(ex, "从备份恢复失败!");
                HandyControl.Controls.Growl.WarningGlobal("恢复失败, 备份文件无效或已损坏!");
            }
        }

        private void ShowSeconds_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.appData.AppConfig.SecondsWindow == true)
            {
                //StartSecondsWindow();
                SecondsWindow.ShowWindow();
            }
            else
            {
                SecondsWindow.CloseWindow();
                //StopSecondsWindow();
            }
        }

        public static void StopSecondsWindow()
        {
            if (MessageUtil.CheckWindowIsRuning("ShowSeconds_Main_" + Constants.MY_UUID))
            {
                MessageUtil.SendMsgByWName(
                    "ShowSeconds_Main_" + Constants.MY_UUID,
                    "Shutdown"
                    );
            }
        }

        public static void StartSecondsWindow()
        {
            try
            {
                using (var objOS = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject objMgmt in objOS.Get())
                    {
                        if (objMgmt.Properties["Caption"].Value != null)
                        {
                            string caption = objMgmt.Properties["Caption"].Value.ToString(); ;
                            LogUtil.WriteLog("获取的系统版本号为:" + caption);
                            if (caption.Contains("Windows 11"))
                            {
                                //找到ShowSeconds插件
                                FileInfo fi = FileUtil.GetFileByNameWithDir("ShowSeconds.exe", Constants.PLUGINS_PATH);
                                if (fi == null)
                                {
                                    HandyControl.Controls.MessageBox.Show("未安装程序插件:ShowSeconds");
                                }
                                else
                                {
                                    //检查是否在运行
                                    if (!MessageUtil.CheckWindowIsRuning("ShowSeconds_Main_" + Constants.MY_UUID))
                                    {
                                        using (Process p = new Process())
                                        {
                                            p.StartInfo.FileName = fi.FullName;
                                            p.StartInfo.WorkingDirectory = fi.FullName.Substring(0, fi.FullName.LastIndexOf("\\"));
                                            p.Start();
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }
    }
}
