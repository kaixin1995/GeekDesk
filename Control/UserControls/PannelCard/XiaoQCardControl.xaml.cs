using GeekDesk.Constant;
using GeekDesk.Control.Other;
using GeekDesk.Control.Windows;
using GeekDesk.Util;
using GeekDesk.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GeekDesk.Control.UserControls.PannelCard
{
    public partial class XiaoQCardControl : UserControl
    {
        private AppData appData => MainWindow.appData;

        /// <summary>当前已切换到的分类 (用于加密验证被拒时回退选中项)</summary>
        private MenuInfo currentMenu;

        public XiaoQCardControl()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                if (appData != null && appData.MenuList != null && appData.MenuList.Count > 0)
                {
                    int index = appData.AppConfig.SelectedMenuIndex;
                    if (index < 0 || index >= appData.MenuList.Count)
                    {
                        index = 0;
                    }
                    //选中后由 SelectionChanged 触发切换 (加密分类会先走密码验证)
                    CategoryListBox.SelectedIndex = index;
                }
            };
        }

        public void SwitchCategory(MenuInfo menu)
        {
            if (menu == null) return;
            currentMenu = menu;
            IconGridListBox.ItemsSource = menu.IconList;
            appData.AppConfig.SelectedMenuIcons = menu.IconList;
            appData.AppConfig.SelectedMenuIndex = appData.MenuList.IndexOf(menu);
        }

        /// <summary>
        /// 数据被整体替换 (如备份恢复) 后重新绑定并切到首个分类
        /// </summary>
        public void ReloadFromData()
        {
            currentMenu = null;
            CategoryListBox.ItemsSource = null;
            CategoryListBox.ItemsSource = appData.MenuList;
            if (appData.MenuList.Count > 0)
            {
                CategoryListBox.SelectedIndex = 0;
                SwitchCategory(appData.MenuList[0]);
            }
        }

        public void ApplyTheme(VisualTheme theme)
        {
            switch (theme)
            {
                case VisualTheme.MacAcrylic:
                    // 1. macOS 磨砂黑 (深色专业深空灰)
                    NavBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E222A"));
                    NavBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3440"));
                    RightIconsBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#181B21"));
                    BottomAddBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252B35"));
                    BottomAddBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3440"));

                    SetThemeBrushes("#0A84FF", "#2E3644", "#D1D5DB", "#FFFFFF", "#9CA3AF");
                    break;

                case VisualTheme.PureMinimal:
                    // 2. 纯净极简纯白现代风
                    NavBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC"));
                    NavBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
                    RightIconsBorder.Background = Brushes.White;
                    BottomAddBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"));
                    BottomAddBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));

                    SetThemeBrushes("#0F172A", "#E2E8F0", "#334155", "#FFFFFF", "#64748B");
                    break;

                case VisualTheme.CrystalSkeuomorphic:
                default:
                    // 3. 经典音速金属微拟物风 (经典象牙米白与商务海军蓝)
                    NavBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E4EAEF"));
                    NavBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B8C5D0"));
                    RightIconsBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAF9F5"));
                    BottomAddBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6DFE7"));
                    BottomAddBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B8C5D0"));

                    SetThemeBrushes("#2B579A", "#CBD8E2", "#2D3748", "#FFFFFF", "#475569");
                    break;
            }
        }

        private void SetThemeBrushes(string selectedNav, string hoverNav, string navText, string navSelectedText, string addText)
        {
            this.Resources["NavItemSelectedBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(selectedNav));
            this.Resources["NavItemHoverBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hoverNav));
            this.Resources["NavTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(navText));
            this.Resources["NavSelectedTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(navSelectedText));
            this.Resources["AddMenuTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(addText));
        }

        private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryListBox.SelectedItem is MenuInfo menu)
            {
                MainWindow.OpenMenuWithVerify(menu,
                    () => SwitchCategory(menu),
                    () =>
                    {
                        //放弃验证：回退到之前选中的分类 (无历史选中时回退到首个未加密分类)
                        MenuInfo fallback = currentMenu;
                        if (fallback == null)
                        {
                            fallback = appData.MenuList.FirstOrDefault(m => !m.IsEncrypt)
                                ?? appData.MenuList.FirstOrDefault();
                        }
                        if (fallback != null)
                        {
                            CategoryListBox.SelectedItem = fallback;
                        }
                    });
            }
        }

        #region 分类管理
        private void CreateMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuInfo info = new MenuInfo()
            {
                MenuEdit = Visibility.Collapsed,
                MenuId = Guid.NewGuid().ToString(),
                MenuName = "新分类",
                IsExpanded = true
            };
            appData.MenuList.Add(info);
            CategoryListBox.SelectedItem = info;
            CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
        }

        private void RenameMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is MenuInfo menu)
            {
                menu.MenuEdit = Visibility.Visible;
            }
        }

        private void MenuRename_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.Tag is MenuInfo menu)
            {
                menu.MenuName = tb.Text.Trim();
                menu.MenuEdit = Visibility.Collapsed;
                CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
            }
        }

        private void MenuRename_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox tb && tb.Tag is MenuInfo menu)
            {
                menu.MenuName = tb.Text.Trim();
                menu.MenuEdit = Visibility.Collapsed;
                CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
            }
        }

        private void EncryptMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is MenuInfo menu)
            {
                MainWindow.ToggleMenuEncrypt(menu);
            }
        }

        private void DeleteMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is MenuInfo menu)
            {
                if (appData.MenuList.Count <= 1)
                {
                    HandyControl.Controls.Growl.WarningGlobal("至少保留一个分类!");
                    return;
                }
                //关联分类先释放文件监听
                FileWatcher.RemoveLinkMenuWatcher(menu);
                int oldIndex = appData.MenuList.IndexOf(menu);
                appData.MenuList.Remove(menu);
                int newIndex = Math.Max(0, oldIndex - 1);
                if (appData.MenuList.Count > 0)
                {
                    CategoryListBox.SelectedIndex = newIndex;
                    SwitchCategory(appData.MenuList[newIndex]);
                }
                CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
            }
        }

        private void MoveMenuUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is MenuInfo menu)
            {
                int idx = appData.MenuList.IndexOf(menu);
                if (idx > 0)
                {
                    appData.MenuList.Move(idx, idx - 1);
                    CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
                }
            }
        }

        private void MoveMenuDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is MenuInfo menu)
            {
                int idx = appData.MenuList.IndexOf(menu);
                if (idx >= 0 && idx < appData.MenuList.Count - 1)
                {
                    appData.MenuList.Move(idx, idx + 1);
                    CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
                }
            }
        }
        #endregion

        #region 图标交互
        private void Icon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (appData.AppConfig.DoubleOpen && e.ClickCount == 2)
            {
                LaunchIcon(sender);
            }
            else if (!appData.AppConfig.DoubleOpen)
            {
                LaunchIcon(sender);
            }
        }

        private void LaunchIcon(object sender)
        {
            if (sender is FrameworkElement element && element.Tag is IconInfo icon)
            {
                //启动次数在 ProcessUtil.StartIconApp 内统一累加
                ProcessUtil.StartIconApp(icon, IconStartType.DEFAULT_STARTUP);
                if (appData.AppConfig.AppHideType == AppHideType.START_EXE)
                {
                    MainWindow.HideApp();
                }
            }
        }

        private void IconAdminStart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is IconInfo icon)
            {
                ProcessUtil.StartIconApp(icon, IconStartType.ADMIN_STARTUP);
            }
        }

        private void ShowInExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is IconInfo icon)
            {
                ProcessUtil.StartIconApp(icon, IconStartType.SHOW_IN_EXPLORE);
            }
        }

        private void AddUrlIcon_Click(object sender, RoutedEventArgs e)
        {
            IconInfoUrlDialog urlDialog = new IconInfoUrlDialog();
            HandyControl.Controls.Dialog.Show(urlDialog, "MainWindowDialog");
        }

        private void AddSystemIcon_Click(object sender, RoutedEventArgs e)
        {
            SystemItemWindow.Show();
        }

        private void SystemContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is IconInfo icon)
            {
                ShellContextMenu scm = new ShellContextMenu();
                FileInfo[] files = new FileInfo[1];
                files[0] = new FileInfo(icon.Path);
                System.Drawing.Point p = System.Windows.Forms.Cursor.Position;
                scm.ShowContextMenu(files, p);
            }
        }

        private void IconProperty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is IconInfo icon)
            {
                IconInfoDialog dialog = new IconInfoDialog(icon);
                dialog.Owner = MainWindow.mainWindow;
                dialog.ShowDialog();
            }
        }

        private void RemoveIcon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is IconInfo icon && CategoryListBox.SelectedItem is MenuInfo currentMenu)
            {
                int index = currentMenu.IconList.IndexOf(icon);
                currentMenu.IconList.Remove(icon);
                CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
                HandyControl.Controls.Growl.Ask("已从「" + currentMenu.MenuName + "」移除 " + icon.Name + ", 撤销本次移除?",
                    isConfirmed =>
                    {
                        if (isConfirmed)
                        {
                            int insertIdx = Math.Min(index, currentMenu.IconList.Count);
                            currentMenu.IconList.Insert(insertIdx, icon);
                            CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
                        }
                        return true;
                    }, "MainWindowAskGrowl");
            }
        }
        #endregion

        #region 外部文件拖入
        private void IconsArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Handled) return;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Handled = true;
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                MenuInfo targetMenu = CategoryListBox.SelectedItem as MenuInfo;
                if (targetMenu != null && files != null)
                {
                    foreach (var file in files)
                    {
                        IconInfo newIcon = CommonCode.GetIconInfoByPath(file);
                        if (newIcon != null)
                        {
                            targetMenu.IconList.Add(newIcon);
                        }
                    }
                    CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
                }
            }
        }
        #endregion
    }
}
