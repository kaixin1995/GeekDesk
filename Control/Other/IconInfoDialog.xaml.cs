using GeekDesk.Constant;
using GeekDesk.Util;
using GeekDesk.ViewModel;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GeekDesk.Control.Other
{
    /// <summary>
    /// IconInfoDialog.xaml 的交互逻辑 (独立屏幕居中模态窗口)
    /// </summary>
    public partial class IconInfoDialog : Window
    {
        public IconInfoDialog()
        {
            InitializeComponent();
        }

        public IconInfoDialog(IconInfo info)
        {
            this.DataContext = info;
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();
                }
                catch { }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 保存修改属性
        /// </summary>
        private void SaveProperty(object sender, RoutedEventArgs e)
        {
            IconInfo info = this.DataContext as IconInfo;
            if (info != null)
            {
                info.BitmapImage = IconImg.Source as BitmapImage;
                info.Name = IconName.Text;
                if (!string.IsNullOrWhiteSpace(IconPath.Text))
                {
                    info.Path = IconPath.Text.Trim();
                }
                info.AdminStartUp = IconIsAdmin.IsChecked.Value;
                info.StartArg = StartArg.Text;
                CommonCode.SaveAppData(MainWindow.appData, Constants.DATA_FILE_PATH);
            }
            this.Close();
        }

        /// <summary>
        /// 修改图标为默认
        /// </summary>
        private void ReStoreImage(object sender, RoutedEventArgs e)
        {
            IconInfo info = this.DataContext as IconInfo;
            if (info != null)
            {
                info.BitmapImage = ImageUtil.ByteArrToImage(info.DefaultImage);
                CommonCode.SaveAppData(MainWindow.appData, Constants.DATA_FILE_PATH);
            }
        }

        /// <summary>
        /// 修改图标
        /// </summary>
        private void EditImage(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog
                {
                    Multiselect = false,
                    Filter = "所有文件(*.*)|*.*"
                };
                if (ofd.ShowDialog() == true)
                {
                    IconInfo info = this.DataContext as IconInfo;
                    if (info != null)
                    {
                        info.BitmapImage = ImageUtil.GetBitmapIconByPath(ofd.FileName);
                        CommonCode.SaveAppData(MainWindow.appData, Constants.DATA_FILE_PATH);
                    }
                }
            }
            catch (Exception e1)
            {
                LogUtil.WriteErrorLog(e1, "修改图标失败!");
                HandyControl.Controls.Growl.WarningGlobal("修改图标失败,已重置为默认图标!");
                ReStoreImage(null, null);
            }
        }
    }
}
