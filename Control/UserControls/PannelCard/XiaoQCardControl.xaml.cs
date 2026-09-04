using GeekDesk.Constant;
using GeekDesk.Control.Other;
using GeekDesk.Control.Windows;
using GeekDesk.Util;
using GeekDesk.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GeekDesk.Control.UserControls.PannelCard
{
    public partial class XiaoQCardControl : UserControl
    {
        private AppData appData => MainWindow.appData;

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
                    CategoryListBox.SelectedIndex = index;
                    SwitchCategory(appData.MenuList[index]);
                }
            };
        }

        public void SwitchCategory(MenuInfo menu)
        {
            if (menu == null) return;
            IconGridListBox.ItemsSource = menu.IconList;
            appData.AppConfig.SelectedMenuIcons = menu.IconList;
            appData.AppConfig.SelectedMenuIndex = appData.MenuList.IndexOf(menu);
        }

        private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryListBox.SelectedItem is MenuInfo menu)
            {
                SwitchCategory(menu);
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
                menu.IsEncrypt = !menu.IsEncrypt;
                CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
            }
        }

        private void DeleteMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is MenuInfo menu)
            {
                if (appData.MenuList.Count <= 1)
                {
                    HandyControl.Controls.Growl.Warning("至少保留一个分类!", "MainWindowGrowl");
                    return;
                }
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
                icon.Count++;
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
                HandyControl.Controls.Dialog.Show(dialog, "MainWindowDialog");
            }
        }

        private void RemoveIcon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is IconInfo icon)
            {
                if (CategoryListBox.SelectedItem is MenuInfo currentMenu)
                {
                    currentMenu.IconList.Remove(icon);
                    CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
                }
            }
        }
        #endregion

        #region 外部文件拖入
        private void IconsArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
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
