using GeekDesk.Util;
using GeekDesk.ViewModel;
using GeekDesk.ViewModel.Temp;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GeekDesk.Control.Other
{
    /// <summary>
    /// TextDialog.xaml 的交互逻辑
    /// </summary>
    public partial class GradientBGDialog
    {
        public HandyControl.Controls.Dialog dialog;

        /// <summary>与 DataContext 中深拷贝副本一一对应的原实例列表</summary>
        private readonly ObservableCollection<GradientBGParam> originalArr = new ObservableCollection<GradientBGParam>();

        public GradientBGDialog()
        {
            foreach(var bg in GradientBGParamList.GradientBGParams)
            {
                originalArr.Add(bg);
            }
            foreach(var bg in MainWindow.appData.AppConfig.CustomBGParams)
            {
                originalArr.Add(bg);
            }

            this.DataContext = DeepCopyUtil.DeepCopy(originalArr);
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            dialog.Close();
        }

        private void BGBorder_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GradientBGParam bgParam = (sender as Border).Tag as GradientBGParam;
            //解析为原实例 (DataContext 中是深拷贝副本), 保证后续改色作用于真实预设
            ObservableCollection<GradientBGParam> copies = (ObservableCollection<GradientBGParam>)this.DataContext;
            int idx = copies.IndexOf(bgParam);
            GradientBGParam original = (idx >= 0 && idx < originalArr.Count) ? originalArr[idx] : bgParam;
            MainWindow.appData.AppConfig.GradientBGParam = original;
            BGSettingUtil.BGSetting();
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

        private void Delete(object sender, RoutedEventArgs e)
        {
            HandyControl.Controls.Growl.Ask("确认删除吗?", isConfirmed =>
            {
                if (isConfirmed)
                {
                    GradientBGParam bg = (GradientBGParam)(((MenuItem)sender).Tag);
                    ObservableCollection<GradientBGParam> bgArr = (ObservableCollection<GradientBGParam>)this.DataContext;
                    bgArr.Remove(bg);
                    MainWindow.appData.AppConfig.CustomBGParams.Remove(bg);
                    for (int i = MainWindow.appData.AppConfig.CustomBGParams.Count - 1; i >= 0; i--)
                    {
                        var cbg = MainWindow.appData.AppConfig.CustomBGParams[i];
                        if (cbg.Id == null)
                        {
                            if (cbg.Color1.Equals(bg.Color1) && cbg.Color2.Equals(bg.Color2))
                            {
                                MainWindow.appData.AppConfig.CustomBGParams.RemoveAt(i);
                            }
                        } else
                        {
                            if (cbg.Id.Equals(bg.Id))
                            {
                                MainWindow.appData.AppConfig.CustomBGParams.RemoveAt(i);
                            }
                        }
                        
                    }
                    MainWindow.appData.AppConfig.CustomBGParams = DeepCopyUtil.DeepCopy(MainWindow.appData.AppConfig.CustomBGParams);
                }
                return true;
            }, "ConfigWindowAskGrowl");
        }
    }
}
