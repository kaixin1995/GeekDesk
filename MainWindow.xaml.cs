using GeekDesk.Constant;
using GeekDesk.Control.Other;
using GeekDesk.Control.UserControls.Config;
using GeekDesk.Control.UserControls.PannelCard;
using GeekDesk.Control.Windows;
using GeekDesk.Interface;
using GeekDesk.MyThread;
using GeekDesk.Task;
using GeekDesk.Util;
using GeekDesk.ViewModel;
using GeekDesk.ViewModel.Temp;
using Microsoft.Win32;
using NPinyin;
using ShowSeconds;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shell;
using System.Windows.Threading;

namespace GeekDesk
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class MainWindow : Window, IWindowCommon
    {

        public static AppData appData;
        public static ToDoInfoWindow toDoInfoWindow;
        public static int hotKeyId = -1;
        public static int toDoHotKeyId = -1;
        public static int colorPickerHotKeyId = -1;
        public static MainWindow mainWindow;

       

        private static bool dataFileExist = true;
        public MainWindow()
        {
            
            //加载数据
            LoadData();
            InitializeComponent();

            //用于其他类访问
            mainWindow = this;

            //执行待办提醒
            ToDoTask.BackLogCheck();

            ////实例化隐藏 Hide类，进行时间timer设置
            MarginHide.ReadyHide(this);
            if (appData.AppConfig.MarginHide)
            {
                MarginHide.StartHide();
            }



        }


        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                //禁用窗口最大化
                WindowUtil.DisableMaxWindow(this);
            }
            catch (Exception) { }
        }

       



        /// <summary>
        /// 搜索快捷键按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchHotKeyDown(object sender, CanExecuteRoutedEventArgs e)
        {
            if (appData.AppConfig.SearchType == SearchType.HOT_KEY && !RunTimeStatus.SEARCH_BOX_SHOW)
            {
                ShowSearchBox();
            } else if (RunTimeStatus.SEARCH_BOX_SHOW)
            {
                HidedSearchBox();
            }
        }

        /// <summary>
        /// 显示搜索框
        /// </summary>
        private void ShowSearchBox()
        {
            RunTimeStatus.SEARCH_BOX_SHOW = true;
            SearchBarRow.Visibility = Visibility.Visible;
            //内容区下移, 避免搜索条遮挡第一行分类/图标
            AccordionCard.Margin = new Thickness(0, 42, 0, 0);
            XiaoQCard.Margin = new Thickness(0, 42, 0, 0);
            SearchBox.Focus();
        }
        /// <summary>
        /// 搜索开始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <summary>
        /// 根据当前启动器风格恢复内容区卡片可见性 (搜索框收起/清空后调用)
        /// </summary>
        public void RestoreCardContentVisibility()
        {
            if (appData.AppConfig.LauncherStyle == LauncherStyle.XiaoQDesk)
            {
                AccordionCard.Visibility = Visibility.Collapsed;
                XiaoQCard.Visibility = Visibility.Visible;
            }
            else
            {
                AccordionCard.Visibility = Visibility.Visible;
                XiaoQCard.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RunTimeStatus.SEARCH_BOX_SHOW = true;

            //刷新搜索后 鼠标移动次数置为0
            RunTimeStatus.MOUSE_MOVE_COUNT = 0;
            //隐藏popup
            RightCard.MyPoptip.IsOpen = false;

            string inputText = SearchBox.Text.ToLower();
            if (!string.IsNullOrEmpty(inputText))
            {
                RunTimeStatus.EVERYTHING_SEARCH_DELAY_TIME = 300;
                if (!RunTimeStatus.EVERYTHING_NEW_SEARCH)
                {                    
                    RunTimeStatus.EVERYTHING_NEW_SEARCH = true;
                    //显示搜索结果列表
                    RightCard.VisibilitySearchCard(Visibility.Visible);
                    //显示加载条
                    RightCard.Loading_RightCard.Visibility = Visibility.Visible;
                    object obj = RightCard.VerticalCard.Content;
                    if (obj != null)
                    {
                        SearchResControl control = obj as SearchResControl;
                        control.VerticalUFG.Visibility = Visibility.Collapsed;
                    }
                    SearchDelay();
                }
            } else
            {
                //清空查询结果
                object obj = RightCard.VerticalCard.Content;
                if (obj != null)
                {
                    SearchResControl control = obj as SearchResControl;
                    control.VerticalUFG.Visibility = Visibility.Collapsed;
                }
                RightCard.Loading_RightCard.Visibility = Visibility.Collapsed;
                RightCard.Visibility = Visibility.Collapsed;
                RestoreCardContentVisibility();
            }

        }

        private void SearchDelay()
        {

            new Thread(() =>
            {

                while (RunTimeStatus.EVERYTHING_SEARCH_DELAY_TIME > 0)
                {
                    Thread.Sleep(10);
                    RunTimeStatus.EVERYTHING_SEARCH_DELAY_TIME -= 10;
                }
                RunTimeStatus.EVERYTHING_NEW_SEARCH = false;

                this.Dispatcher.Invoke(() =>
                {
                    string inputText = SearchBox.Text.ToLower().Trim();
                    if (string.IsNullOrEmpty(inputText))
                    {
                        RightCard.Loading_RightCard.Visibility = Visibility.Collapsed;
                        RightCard.Visibility = Visibility.Collapsed;
                        RestoreCardContentVisibility();
                        return;
                    }
                    else
                    {
                        RightCard.Visibility = Visibility.Visible;
                        AccordionCard.Visibility = Visibility.Collapsed;
                        XiaoQCard.Visibility = Visibility.Collapsed;
                    }
                    new Thread(() =>
                    {
                        try
                        {
                            ObservableCollection<IconInfo> resList = new ObservableCollection<IconInfo>();

                            //快照避免搜索期间 UI 修改集合导致异常
                            MenuInfo[] menuSnapshot = appData.MenuList.ToArray();

                            // 纯净快捷方式/图标搜索 (基于名称和拼音首字母)
                            foreach (MenuInfo menu in menuSnapshot)
                            {
                                IconInfo[] iconSnapshot = menu?.IconList?.ToArray();
                                if (iconSnapshot == null) continue;
                                foreach (IconInfo icon in iconSnapshot)
                                {
                                    if (RunTimeStatus.EVERYTHING_NEW_SEARCH) return;
                                    string pyName = Pinyin.GetInitials(icon.Name).ToLower();
                                    if (icon.Name.ToLower().Contains(inputText) || pyName.Contains(inputText))
                                    {
                                        resList.Add(icon);
                                    }
                                }
                            }

                            this.Dispatcher.Invoke(() =>
                            {
                                SearchResControl control = new SearchResControl(resList, inputText);
                                RightCard.VerticalCard.Content = control;
                                //关闭加载效果
                                RightCard.Loading_RightCard.Visibility = Visibility.Collapsed;
                            });
                        }
                        catch (Exception ex)
                        {
                            LogUtil.WriteErrorLog(ex, "本地搜索异常!");
                        }
                    }).Start();

                });

            }).Start();
        }

        /// <summary>
        /// 隐藏搜索框
        /// </summary>
        public void HidedSearchBox()
        {
            RunTimeStatus.EVERYTHING_NEW_SEARCH = true;
            RunTimeStatus.SEARCH_BOX_HIDED_300 = false;
            new Thread(() =>
            {
                Thread.Sleep(300);
                RunTimeStatus.SEARCH_BOX_HIDED_300 = true;
            }).Start();
            new Thread(() =>
            {
                Thread.Sleep(1000);
                RunTimeStatus.EVERYTHING_NEW_SEARCH = false;
            }).Start();
            new Thread(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    Keyboard.Focus(SearchBox);
                    RunTimeStatus.SEARCH_BOX_SHOW = false;
                    SearchBarRow.Visibility = Visibility.Collapsed;
                    //内容区位置还原
                    AccordionCard.Margin = new Thickness(0);
                    XiaoQCard.Margin = new Thickness(0);
                    SearchBox.TextChanged -= SearchBox_TextChanged;
                    SearchBox.Clear();
                    SearchBox.TextChanged += SearchBox_TextChanged;
                    RightCard.VerticalCard.Content = null;
                    RightCard.VisibilitySearchCard(Visibility.Collapsed);
                    RightCard.Visibility = Visibility.Collapsed;
                    RestoreCardContentVisibility();
                });
            }).Start();
        }


        /// <summary>
        /// 加载缓存数据
        /// </summary>
        private void LoadData()
        {
            //判断数据文件是否存在 如果不存在那么是第一次打开程序
            dataFileExist = File.Exists(Constants.DATA_FILE_PATH);

            appData = CommonCode.GetAppDataByFile();

            this.DataContext = appData;
            this.Topmost = true;
            appData.AppConfig.AlwaysTopmost = true;
            if (appData.MenuList.Count == 0)
            {
                appData.MenuList.Add(new MenuInfo() { MenuName = "常用软件", MenuId = System.Guid.NewGuid().ToString(), MenuEdit = Visibility.Collapsed });
            }

            //按启动器风格应用各自记忆的窗口尺寸 (带边界校验)
            if (appData.AppConfig.LauncherStyle == LauncherStyle.XiaoQDesk)
            {
                this.Width = Math.Min(Math.Max(appData.AppConfig.XiaoQWindowWidth, 560), 2000);
                this.Height = Math.Min(Math.Max(appData.AppConfig.XiaoQWindowHeight, 380), 2000);
            }
            else
            {
                this.Width = Math.Min(Math.Max(appData.AppConfig.SonicWindowWidth, 180), 460);
                this.Height = Math.Min(Math.Max(appData.AppConfig.SonicWindowHeight, 260), 2000);
            }
        }

        /// <summary>
        /// 窗口加载完毕 执行方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BGSettingUtil.BGSetting();
            if (!appData.AppConfig.StartedShowPanel)
            {
                this.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowApp();
            }
            //给任务栏图标一个名字
            BarIcon.Text = Constants.MY_NAME;
            ApplyLauncherStyle();
            ApplyThemeStyle();

            //注册热键
            if (true == appData.AppConfig.EnableAppHotKey)
            {
                RegisterHotKey(true);
            }
            if (true == appData.AppConfig.EnableTodoHotKey)
            {
                RegisterCreateToDoHotKey(true);
            }

            if (true == appData.AppConfig.EnableColorPickerHotKey)
            {
                RegisterColorPickerHotKey(true);
            }

            //注册自启动
            if (!appData.AppConfig.SelfStartUped && !Constants.DEV)
            {
                RegisterUtil.SetSelfStarting(appData.AppConfig.SelfStartUp, Constants.MY_NAME);
            }

            //注册鼠标监听事件
            // 仅根据秒表窗口等需求开启全局钩子，废弃中键呼出
            if (appData.AppConfig.SecondsWindow == true)
            {
                MouseHookThread.Hook();
            }

            //显秒插件
            if (appData.AppConfig.SecondsWindow == true)
            {
                SecondsWindow.ShowWindow();
            }

            //监听实时文件夹菜单
            FileWatcher.EnableLinkMenuWatcher(appData);

            //建立相对路径
            RelativePathThread.MakeRelativePath();

            //设置归属桌面  解决桌面覆盖程序界面的bug
            WindowUtil.SetOwner(this, WindowUtil.GetDesktopHandle(this, DesktopLayer.Progman));

            //启动文件备份任务
            BakTask.Start();

            Keyboard.Focus(SearchBox);

            MessageUtil.ChangeWindowMessageFilter(MessageUtil.WM_COPYDATA, 1);
        }



        /// <summary>
        /// 注册当前窗口的热键
        /// </summary>
        public static void RegisterHotKey(bool first)
        {
            try
            {
                if (appData.AppConfig.HotkeyModifiers != GlobalHotKey.HotkeyModifiers.None)
                {
                    hotKeyId = GlobalHotKey.RegisterHotKey(appData.AppConfig.HotkeyModifiers, appData.AppConfig.Hotkey, () =>
                    {
                        if (RunTimeStatus.MAIN_HOT_KEY_DOWN) return;
                        RunTimeStatus.MAIN_HOT_KEY_DOWN = true;
                        new Thread(() =>
                        {
                            Thread.Sleep(RunTimeStatus.MAIN_HOT_KEY_TIME);
                            RunTimeStatus.MAIN_HOT_KEY_DOWN = false;
                        }).Start();

                        if (MotionControl.hotkeyFinished)
                        {
                            if (CheckShouldShowApp())
                            {
                                ShowApp();
                            }
                            else
                            {
                                HideApp();
                            }
                        }
                    });
                    if (!first)
                    {
                        HandyControl.Controls.Growl.Success("GeekDesk快捷键注册成功(" + appData.AppConfig.HotkeyStr + ")!", "HotKeyGrowl");
                    }
                }
                else
                {
                }
            }
            catch (Exception)
            {
                if (first)
                {
                    HandyControl.Controls.Growl.WarningGlobal("GeekDesk启动快捷键已被其它程序占用(" + appData.AppConfig.HotkeyStr + ")!");
                }
                else
                {
                    HandyControl.Controls.Growl.Warning("GeekDesk启动快捷键已被其它程序占用(" + appData.AppConfig.HotkeyStr + ")!", "HotKeyGrowl");

                }
            }
        }



        /// <summary>
        /// 注册新建待办的热键
        /// </summary>
        public static void RegisterCreateToDoHotKey(bool first)
        {
            try
            {

                if (appData.AppConfig.HotkeyModifiers != GlobalHotKey.HotkeyModifiers.None)
                {
                    //加载完毕注册热键
                    toDoHotKeyId = GlobalHotKey.RegisterHotKey(appData.AppConfig.ToDoHotkeyModifiers, appData.AppConfig.ToDoHotkey, () =>
                    {
                        if (MotionControl.hotkeyFinished)
                        {
                            ToDoWindow.ShowOrHide();
                        }
                    });
                    if (!first)
                    {
                        HandyControl.Controls.Growl.Success("新建待办任务快捷键注册成功(" + appData.AppConfig.ToDoHotkeyStr + ")!", "HotKeyGrowl");
                    }
                }

            }
            catch (Exception)
            {
                if (first)
                {
                    HandyControl.Controls.Growl.WarningGlobal("新建待办任务快捷键已被其它程序占用(" + appData.AppConfig.ToDoHotkeyStr + ")!");
                }
                else
                {
                    HandyControl.Controls.Growl.Warning("新建待办任务快捷键已被其它程序占用(" + appData.AppConfig.ToDoHotkeyStr + ")!", "HotKeyGrowl");
                }
            }
        }

        /// <summary>
        /// 注册新建待办的热键
        /// </summary>
        public static void RegisterColorPickerHotKey(bool first)
        {
            try
            {
                if (appData.AppConfig.HotkeyModifiers != GlobalHotKey.HotkeyModifiers.None)
                {
                    //加载完毕注册热键
                    colorPickerHotKeyId = GlobalHotKey.RegisterHotKey(appData.AppConfig.ColorPickerHotkeyModifiers, appData.AppConfig.ColorPickerHotkey, () =>
                    {
                        if (MotionControl.hotkeyFinished)
                        {
                            GlobalColorPickerWindow.CreateNoShow();
                        }
                    });
                    if (!first)
                    {
                        HandyControl.Controls.Growl.Success("拾色器快捷键注册成功(" + appData.AppConfig.ColorPickerHotkeyStr + ")!", "HotKeyGrowl");
                    }
                }
            }
            catch (Exception)
            {
                if (first)
                {
                    HandyControl.Controls.Growl.WarningGlobal("拾色器快捷键已被其它程序占用(" + appData.AppConfig.ColorPickerHotkeyStr + ")!");
                }
                else
                {
                    HandyControl.Controls.Growl.Warning("拾色器快捷键已被其它程序占用(" + appData.AppConfig.ColorPickerHotkeyStr + ")!", "HotKeyGrowl");
                }
            }
        }




        /// <summary>
        /// 程序窗体拖动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DragMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }




        /// <summary>
        /// 关闭按钮单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            HideApp();
        }



        ///// <summary>
        ///// 左侧栏宽度改变 持久化
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void LeftCardResize(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        //{
        //    appData.AppConfig.MenuCardWidth = LeftColumn.Width.Value;
        //}



        /// <summary>
        /// 右键任务栏图标 显示主面板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ShowApp(object sender, RoutedEventArgs e)
        {
            ShowApp();
        }
        public static void ShowApp()
        {
            //有全屏化应用则不显示
            //if (CommonCode.IsPrimaryFullScreen())
            //{
            //    return;
            //}

            if (MarginHide.ON_HIDE)
            {
                //修改贴边隐藏状态为未隐藏并平滑滑出
                if (MarginHide.IS_HIDE)
                {
                    MarginHide.ForceShowFromMargin();
                }
                MarginHide.IS_HIDE = false;
                if (!CommonCode.MouseInWindow(mainWindow))
                {
                    RunTimeStatus.MARGIN_HIDE_AND_OTHER_SHOW = true;
                    MarginHide.WaitHide(3000);
                }
            }

            MainWindow.mainWindow.Activate();
            mainWindow.Show();
            if (appData.AppConfig.LauncherStyle == LauncherStyle.XiaoQDesk)
            {
                mainWindow.PositionOverTaskbar();
            }
            //mainWindow.Visibility = Visibility.Visible;
            if (appData.AppConfig.AppAnimation)
            {
                appData.AppConfig.IsShow = true;
            }
            else
            {
                appData.AppConfig.IsShow = null;
                //防止永远不显示界面
                if (mainWindow.Opacity < 1)
                {
                    mainWindow.Opacity = 1;
                }
            }


            //FadeStoryBoard(1, (int)CommonEnum.WINDOW_ANIMATION_TIME, Visibility.Visible);

            Keyboard.Focus(mainWindow);
            if (RunTimeStatus.SHOW_MENU_PASSWORDBOX)
            {
                mainWindow.PDDialog.SetFocus();
            }
            else
            {
                Keyboard.Focus(mainWindow.SearchBox);
            }
        }

        public static void HideApp()
        {
            if (appData.AppConfig.AppAnimation)
            {
                appData.AppConfig.IsShow = false;
            }
            else
            {
                appData.AppConfig.IsShow = null;
                HideAppVis();
            }

        }

        private static void HideAppVis()
        {
            //关闭锁定
            RunTimeStatus.LOCK_APP_PANEL = false;
            if (RunTimeStatus.SEARCH_BOX_SHOW)
            {
                mainWindow.HidedSearchBox();
            }
            mainWindow.Visibility = Visibility.Collapsed;
            //if (!MarginHide.IS_HIDE)
            //{

            //}
            //else
            //{
            //    ShowApp();
            //}
        }

        /// <summary>
        /// 密码验证弹框显隐变更：置焦点并重置输入状态
        /// </summary>
        private void PDDialog_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (PDDialog.Visibility == Visibility.Visible)
            {
                RunTimeStatus.SHOW_MENU_PASSWORDBOX = true;
                PDDialog.ClearVal();
                PDDialog.HideErrorMsg();
                PDDialog.count = 0;
                PDDialog.SetFocus();
            }
            else
            {
                RunTimeStatus.SHOW_MENU_PASSWORDBOX = false;
                PDDialog.ClearVal();
            }
        }

        /// <summary>
        /// 带密码验证的分类打开动作：加密且本次运行未验证过的分类需先输入密码
        /// </summary>
        /// <param name="menu">目标分类</param>
        /// <param name="onOpen">验证通过(或无需验证)后执行</param>
        /// <param name="onRefuse">用户放弃验证时执行(如回退到原分类)</param>
        public static void OpenMenuWithVerify(MenuInfo menu, Action onOpen, Action onRefuse = null)
        {
            if (menu != null && menu.IsEncrypt
                && !string.IsNullOrEmpty(appData.AppConfig.MenuPassword)
                && !RunTimeStatus.UNLOCKED_MENU_IDS.Contains(menu.MenuId))
            {
                PasswordDialog pd = mainWindow.PDDialog;
                pd.type = PasswordType.VERIFY;
                pd.menuInfo = menu;
                pd.VerifySuccess = () =>
                {
                    RunTimeStatus.UNLOCKED_MENU_IDS.Add(menu.MenuId);
                    onOpen?.Invoke();
                };
                pd.VerifyRefuse = onRefuse;
                pd.Visibility = Visibility.Visible;
                return;
            }
            onOpen?.Invoke();
        }

        /// <summary>
        /// 加密/解除加密分类：未设置过密码时先走创建密码流程，已加密的分类解除加密需先验证
        /// </summary>
        public static void ToggleMenuEncrypt(MenuInfo menu)
        {
            if (menu == null) return;
            if (!menu.IsEncrypt)
            {
                if (string.IsNullOrEmpty(appData.AppConfig.MenuPassword))
                {
                    //首次加密：先创建密码 (DonePassword 完成后自动加密该分类)
                    PasswordDialog pd = mainWindow.PDDialog;
                    pd.type = PasswordType.CREATE;
                    pd.menuInfo = menu;
                    pd.count = 0;
                    pd.ClearVal();
                    pd.Visibility = Visibility.Visible;
                }
                else
                {
                    menu.IsEncrypt = true;
                    RunTimeStatus.UNLOCKED_MENU_IDS.Add(menu.MenuId);
                    HandyControl.Controls.Growl.SuccessGlobal(menu.MenuName + " 已加密!");
                }
            }
            else
            {
                OpenMenuWithVerify(menu, () =>
                {
                    menu.IsEncrypt = false;
                    RunTimeStatus.UNLOCKED_MENU_IDS.Remove(menu.MenuId);
                    HandyControl.Controls.Growl.SuccessGlobal(menu.MenuName + " 已解除加密!");
                });
            }
        }

        /// <summary>
        /// 淡入淡出效果
        /// </summary>
        /// <param name="opacity"></param>
        /// <param name="milliseconds"></param>
        /// <param name="visibility"></param>
        public static void FadeStoryBoard(int opacity, int milliseconds, Visibility visibility)
        {
            if (appData.AppConfig.AppAnimation)
            {
                DoubleAnimation opacityAnimation = new DoubleAnimation
                {
                    From = mainWindow.Opacity,
                    To = opacity,
                    Duration = new Duration(TimeSpan.FromMilliseconds(milliseconds))
                };
                opacityAnimation.Completed += (s, e) =>
                {
                    mainWindow.BeginAnimation(OpacityProperty, null);
                    if (visibility == Visibility.Visible)
                    {
                        mainWindow.Opacity = 1;
                    }
                    else
                    {
                        mainWindow.Opacity = 0;
                        CommonCode.SortIconList();
                    }
                };
                Timeline.SetDesiredFrameRate(opacityAnimation, 60);
                mainWindow.BeginAnimation(OpacityProperty, opacityAnimation);
            }
            else
            {
                //防止关闭动画后 窗体仍是0透明度
                mainWindow.Opacity = 1;
                mainWindow.Visibility = visibility;
                if (visibility == Visibility.Collapsed)
                {
                    CommonCode.SortIconList();
                }
            }
        }




        private void ToggleSearchBoxClick(object sender, RoutedEventArgs e)
        {
            if (SearchBarRow.Visibility == Visibility.Visible)
            {
                HidedSearchBox();
            }
            else
            {
                ShowSearchBox();
            }
        }

        private void CloseSearchClick(object sender, RoutedEventArgs e)
        {
            HidedSearchBox();
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainSettingBtn.ContextMenu != null)
            {
                MainSettingBtn.ContextMenu.PlacementTarget = MainSettingBtn;
                MainSettingBtn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Custom;
                MainSettingBtn.ContextMenu.CustomPopupPlacementCallback = (popupSize, targetSize, offset) =>
                {
                    Point p = new Point(targetSize.Width - popupSize.Width, targetSize.Height + 3);
                    return new System.Windows.Controls.Primitives.CustomPopupPlacement[]
                    {
                        new System.Windows.Controls.Primitives.CustomPopupPlacement(p, System.Windows.Controls.Primitives.PopupPrimaryAxis.Horizontal)
                    };
                };
                MainSettingBtn.ContextMenu.IsOpen = true;
            }
        }

        private void MenuSearch_Click(object sender, RoutedEventArgs e)
        {
            ShowSearchBox();
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow.Show(appData.AppConfig, this);
        }

        public void ApplyThemeStyle()
        {
            BGSettingUtil.BGSetting();
            AccordionCard.InitDrawerLayout();

            VisualTheme theme = appData?.AppConfig?.VisualTheme ?? VisualTheme.MacAcrylic;
            switch (theme)
            {
                case VisualTheme.MacAcrylic:
                    // 1. macOS 磨砂黑质感 (深色深空灰专业风)
                    MainContentContainer.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#181B21"));
                    TitleBarBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#21252E"));
                    TitleBarBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#323946"));
                    BottomToolbarBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#21252E"));
                    BottomToolbarBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#323946"));
                    ApplySearchBarTheme("#1F242E", "#323946", "#D1D5DB");

                    Brush darkIconBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1D5DB"));
                    if (StyleSwitchPath != null) StyleSwitchPath.Fill = darkIconBrush;
                    if (SearchBtnPath != null) SearchBtnPath.Fill = darkIconBrush;
                    if (SettingBtnPath != null) SettingBtnPath.Fill = darkIconBrush;
                    if (QuickToolsPanel != null) System.Windows.Documents.TextElement.SetForeground(QuickToolsPanel, darkIconBrush);
                    if (appData?.AppConfig != null) appData.AppConfig.TextColor = "#E5E7EB";
                    break;

                case VisualTheme.PureMinimal:
                    // 2. 纯净极简纯白现代风 (极致纯白平铺，极细线条)
                    MainContentContainer.Background = Brushes.White;
                    TitleBarBorder.Background = Brushes.White;
                    TitleBarBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
                    BottomToolbarBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC"));
                    BottomToolbarBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
                    ApplySearchBarTheme("#F8FAFC", "#E2E8F0", "#334155");

                    Brush lightIconBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
                    if (StyleSwitchPath != null) StyleSwitchPath.Fill = lightIconBrush;
                    if (SearchBtnPath != null) SearchBtnPath.Fill = lightIconBrush;
                    if (SettingBtnPath != null) SettingBtnPath.Fill = lightIconBrush;
                    if (QuickToolsPanel != null) System.Windows.Documents.TextElement.SetForeground(QuickToolsPanel, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B")));
                    if (appData?.AppConfig != null) appData.AppConfig.TextColor = "#1E293B";
                    break;

                case VisualTheme.CrystalSkeuomorphic:
                default:
                    // 3. 经典音速金属微拟物风 (经典电镀银/拉丝双色渐变高光与复古象牙暖米色)
                    MainContentContainer.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F3EC"));
                    LinearGradientBrush titleBrush = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(0, 1)
                    };
                    titleBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F2F5F8"), 0.0));
                    titleBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#DEE5EC"), 0.5));
                    titleBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#BFCBD7"), 0.51));
                    titleBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#ABBAC9"), 1.0));
                    TitleBarBorder.Background = titleBrush;
                    TitleBarBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7E8F9F"));
                    BottomToolbarBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCE3EA"));
                    BottomToolbarBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ADBAC6"));
                    ApplySearchBarTheme("#EDF1F5", "#ADBAC6", "#3B4654");

                    Brush metalIconBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B4654"));
                    if (StyleSwitchPath != null) StyleSwitchPath.Fill = metalIconBrush;
                    if (SearchBtnPath != null) SearchBtnPath.Fill = metalIconBrush;
                    if (SettingBtnPath != null) SettingBtnPath.Fill = metalIconBrush;
                    if (QuickToolsPanel != null) System.Windows.Documents.TextElement.SetForeground(QuickToolsPanel, metalIconBrush);
                    if (appData?.AppConfig != null) appData.AppConfig.TextColor = "#1F2937";
                    break;
            }

            if (XiaoQCard != null)
            {
                XiaoQCard.ApplyTheme(theme);
            }
            if (RightCard != null)
            {
                RightCard.ApplyTheme(theme);
            }
        }

        /// <summary>
        /// 悬浮搜索条按主题着色 (背景/边框/文字/图标)
        /// </summary>
        private void ApplySearchBarTheme(string bg, string border, string foreground)
        {
            if (SearchBarRow == null) return;
            Brush fgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(foreground));
            SearchBarRow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bg));
            SearchBarRow.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(border));
            SearchBox.Foreground = fgBrush;
            if (SearchMagPath != null) SearchMagPath.Fill = fgBrush;
            if (SearchClosePath != null) SearchClosePath.Fill = fgBrush;
        }

        private void SwitchTheme_Mac(object sender, RoutedEventArgs e)
        {
            appData.AppConfig.VisualTheme = VisualTheme.MacAcrylic;
            ApplyThemeStyle();
            CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
            HandyControl.Controls.Growl.SuccessGlobal("已切换至：macOS 磨砂质感风");
        }

        private void SwitchTheme_Minimal(object sender, RoutedEventArgs e)
        {
            appData.AppConfig.VisualTheme = VisualTheme.PureMinimal;
            ApplyThemeStyle();
            CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
            HandyControl.Controls.Growl.SuccessGlobal("已切换至：纯净极简现代风");
        }

        private void SwitchTheme_Crystal(object sender, RoutedEventArgs e)
        {
            appData.AppConfig.VisualTheme = VisualTheme.CrystalSkeuomorphic;
            ApplyThemeStyle();
            CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
            HandyControl.Controls.Growl.SuccessGlobal("已切换至：经典金属微拟物风");
        }

        private void ToggleLauncherStyle_Click(object sender, RoutedEventArgs e)
        {
            if (appData.AppConfig.LauncherStyle == LauncherStyle.SonicDrawer)
            {
                appData.AppConfig.LauncherStyle = LauncherStyle.XiaoQDesk;
            }
            else
            {
                appData.AppConfig.LauncherStyle = LauncherStyle.SonicDrawer;
            }
            ApplyLauncherStyle();
            CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
        }

        public void ApplyLauncherStyle()
        {
            if (appData.AppConfig.LauncherStyle == LauncherStyle.XiaoQDesk)
            {
                AccordionCard.Visibility = Visibility.Collapsed;
                XiaoQCard.Visibility = Visibility.Visible;
                //小Q书桌模式: 宽卡片尺寸 + 更高的最小宽度 (保证左侧分类栏与图标网格可用)
                this.MinWidth = 560;
                this.MinHeight = 380;
                this.Width = Math.Min(Math.Max(appData.AppConfig.XiaoQWindowWidth, 560), 2000);
                this.Height = Math.Min(Math.Max(appData.AppConfig.XiaoQWindowHeight, 380), 2000);
                PositionOverTaskbar();
            }
            else
            {
                XiaoQCard.Visibility = Visibility.Collapsed;
                AccordionCard.Visibility = Visibility.Visible;
                //音速启动模式: 窄条尺寸
                this.MinWidth = 180;
                this.MinHeight = 260;
                this.Width = Math.Min(Math.Max(appData.AppConfig.SonicWindowWidth, 180), 460);
                this.Height = Math.Min(Math.Max(appData.AppConfig.SonicWindowHeight, 260), 2000);
            }
        }

        /// <summary>
        /// 贴靠任务栏正上方并播放自下而上展开动画 (自动感知多屏与 DPI 缩放)
        /// </summary>
        public void PositionOverTaskbar()
        {
            try
            {
                System.Drawing.Point mousePt = System.Windows.Forms.Cursor.Position;
                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromPoint(mousePt);
                if (screen == null) screen = System.Windows.Forms.Screen.PrimaryScreen;

                double dpiScale = ScreenUtil.GetScreenScalingFactor();
                if (dpiScale <= 0) dpiScale = 1.0;

                double workLeft = screen.WorkingArea.Left / dpiScale;
                double workTop = screen.WorkingArea.Top / dpiScale;
                double workWidth = screen.WorkingArea.Width / dpiScale;
                double workHeight = screen.WorkingArea.Height / dpiScale;
                double workBottom = workTop + workHeight;

                double targetLeft = workLeft + (workWidth - this.Width) / 2;
                double targetTop = workBottom - this.Height;

                this.Left = targetLeft;
                this.Top = targetTop;

                DoubleAnimation slideAnim = new DoubleAnimation
                {
                    From = workBottom,
                    To = targetTop,
                    Duration = TimeSpan.FromMilliseconds(180),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                this.BeginAnimation(TopProperty, slideAnim);
            }
            catch { }
        }

        private static bool isPinned = true;
        private void ToggleTopmostClick(object sender, RoutedEventArgs e)
        {
            isPinned = !isPinned;
            this.Topmost = isPinned;
            appData.AppConfig.AlwaysTopmost = isPinned;
            if (isPinned)
            {
                MarginHide.StopHide();
                HandyControl.Controls.Growl.SuccessGlobal("已开启置顶并固定窗口");
            }
            else
            {
                if (appData.AppConfig.MarginHide)
                {
                    MarginHide.StartHide();
                }
                HandyControl.Controls.Growl.InfoGlobal("已解除置顶与固定");
            }
        }

        private void QuickTool_TaskManager(object sender, RoutedEventArgs e)
        {
            try { Process.Start("taskmgr.exe"); } catch { }
        }

        private void QuickTool_ControlPanel(object sender, RoutedEventArgs e)
        {
            try { Process.Start("control.exe"); } catch { }
        }

        private void QuickTool_Calc(object sender, RoutedEventArgs e)
        {
            try { Process.Start("calc.exe"); } catch { }
        }

        private void QuickTool_LockWorkstation(object sender, RoutedEventArgs e)
        {
            try { Process.Start("rundll32.exe", "user32.dll,LockWorkStation"); } catch { }
        }

        /// <summary>
        /// 图片图标单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyIcon_Click(object sender, RoutedEventArgs e)
        {
            if (CheckShouldShowApp())
            {
                ShowApp();
            }
            else
            {
                HideApp();
            }
        }

        private static bool CheckShouldShowApp()
        {
            return mainWindow.Visibility == Visibility.Collapsed
                || mainWindow.Opacity == 0
                || MarginHide.IS_HIDE
                || !WindowUtil.WindowIsTop(mainWindow);
        }

        /// <summary>
        /// 右键任务栏图标 设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigApp(object sender, RoutedEventArgs e)
        {
            ConfigWindow.Show(appData.AppConfig, this);
        }


        /// <summary>
        /// 右键任务栏图标打开程序目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenThisDir(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "Explorer.exe";
            p.StartInfo.Arguments = "/e,/select," + Constants.APP_DIR + Constants.MY_NAME + ".exe";
            p.Start();
        }




        /// <summary>
        /// 设置按钮左键直接打开设置页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigButtonClick(object sender, RoutedEventArgs e)
        {
            ConfigWindow.Show(appData.AppConfig, this);
        }

        /// <summary>
        /// 设置按钮右键打开快捷选项菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ConfigWindow.Show(appData.AppConfig, this);
        }

        /// <summary>
        /// 设置菜单点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigMenuClick(object sender, RoutedEventArgs e)
        {
            ConfigWindow.Show(appData.AppConfig, this);
        }
        /// <summary>
        /// 待办任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BacklogMenuClick(object sender, RoutedEventArgs e)
        {
            ToDoWindow.Show();
        }


        private void AppWindowLostFocus()
        {
            if (appData.AppConfig.AppHideType == AppHideType.LOST_FOCUS
                && this.Opacity == 1 && !RunTimeStatus.LOCK_APP_PANEL)
            {
                //如果开启了贴边隐藏 则窗体不贴边才隐藏窗口
                if (!appData.AppConfig.MarginHide || (appData.AppConfig.MarginHide && !MarginHide.IS_HIDE))
                {
                    HideApp();
                }
            }
        }


        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.DataContext != null)
            {
                AppData appData = this.DataContext as AppData;
                appData.AppConfig.WindowWidth = this.Width;
                appData.AppConfig.WindowHeight = this.Height;
                //窗口尺寸按当前启动器风格分别持久化
                if (appData.AppConfig.LauncherStyle == LauncherStyle.XiaoQDesk)
                {
                    appData.AppConfig.XiaoQWindowWidth = this.Width;
                    appData.AppConfig.XiaoQWindowHeight = this.Height;
                }
                else
                {
                    appData.AppConfig.SonicWindowWidth = this.Width;
                    appData.AppConfig.SonicWindowHeight = this.Height;
                }
            }
        }



        /// <summary>
        /// 右键任务栏图标退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitApp(object sender, RoutedEventArgs e)
        {
            if (appData.AppConfig.SecondsWindow == true)
            {
                MouseHookThread.Dispose();
            }
            Application.Current.Shutdown();
        }
        /// <summary>
        /// 重启
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReStartApp(object sender, RoutedEventArgs e)
        {
            ProcessUtil.ReStartApp();
        }

        /// <summary>
        /// 关闭托盘图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseBarIcon(object sender, RoutedEventArgs e)
        {
            appData.AppConfig.ShowBarIcon = false;
        }


        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                //密码验证弹框显示时 Esc 优先关闭弹框
                if (RunTimeStatus.SHOW_MENU_PASSWORDBOX)
                {
                    PDDialog.Visibility = Visibility.Collapsed;
                    Action refuse = PDDialog.VerifyRefuse;
                    PDDialog.VerifyRefuse = null;
                    refuse?.Invoke();
                    return;
                }
                HideApp();
            }

            if (RunTimeStatus.SEARCH_BOX_SHOW && (e.Key == Key.Up
                || e.Key == Key.Down
                || e.Key == Key.Tab
                || e.Key == Key.Enter
                ))
            {
                SearchResControl res = RightCard.VerticalCard.Content as SearchResControl;
                if (res == null) return;
                if (e.Key == Key.Down || e.Key == Key.Tab)
                {
                    res.SearchListBoxIndexAdd();
                }
                else if (e.Key == Key.Up)
                {
                    res.SearchListBoxIndexSub();
                }
                else if (e.Key == Key.Enter)
                {
                    res.StartupSelectionItem();
                }
            }
        }



        /// <summary>
        /// 为了让修改菜单的textBox失去焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 点击空白区域时不再强制抢焦点，允许搜索框和分类编辑框自由输入
        }

        /// <summary>
        /// 鼠标进入后 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            //防止延迟贴边隐藏
            RunTimeStatus.MARGIN_HIDE_AND_OTHER_SHOW = false;
        }

        /// <summary>
        /// 打开屏幕拾色器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColorPicker(object sender, RoutedEventArgs e)
        {
            TaskbarContextMenu.IsOpen = false;
            GlobalColorPickerWindow.CreateNoShow();
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            // 如果没有在修改菜单 并且不是右键点击了面板
            if (!RunTimeStatus.IS_MENU_EDIT
                && !RunTimeStatus.SHOW_RIGHT_BTN_MENU
                && !RunTimeStatus.APP_BTN_IS_DOWN)
            {
                if (RunTimeStatus.SHOW_MENU_PASSWORDBOX)
                {
                    //必须在其它文本框没有工作的时候才给密码框焦点
                    PDDialog.SetFocus();
                }
                else
                {
                    //必须在其它文本框没有工作的时候才给搜索框焦点
                    Keyboard.Focus(SearchBox);
                }

            }

        }

        private void AppWindow_Deactivated(object sender, EventArgs e)
        {
            AppWindowLostFocus();
        }

        /// <summary>
        /// 备份数据文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [Obsolete]
        private void BakDataFile(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(() =>
            {
                CommonCode.BakAppData();
            });
            t.ApartmentState = ApartmentState.STA;
            t.Start();
        }

        private void AppButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //点击了面板
            RunTimeStatus.APP_BTN_IS_DOWN = true;
            new Thread(() =>
            {
                Thread.Sleep(50);
                RunTimeStatus.APP_BTN_IS_DOWN = false;
            }).Start();
        }


        private ICommand _hideCommand;
        public ICommand HideCommand
        {
            get
            {
                if (_hideCommand == null)
                {
                    _hideCommand = new RelayCommand(
                        p =>
                        {
                            return true;
                        },
                        p =>
                        {
                            HideAppVis();
                        });
                }
                return _hideCommand;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                IntPtr handle = hwndSource.Handle;
                hwndSource.AddHook(new HwndSourceHook(WndProc));
            }
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == MessageUtil.WM_COPYDATA)
            {
                MessageUtil.CopyDataStruct cds = (MessageUtil.CopyDataStruct)System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(MessageUtil.CopyDataStruct));
                if ("ShowApp".Equals(cds.msg))
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        ShowApp();
                        this.Activate();
                        this.Topmost = true;
                        if (appData.AppConfig.AlwaysTopmost != true)
                        {
                            this.Topmost = false;
                        }
                    });
                }
            }
            return hwnd;
        }
    }
}
