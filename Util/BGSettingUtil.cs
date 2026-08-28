using GeekDesk.Constant;
using GeekDesk.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace GeekDesk.Util
{
    public class BGSettingUtil
    {
        private static readonly AppConfig appConfig = MainWindow.appData.AppConfig;

        public static void BGSetting()
        {
            if (appConfig.BitmapImage == null && appConfig.GradientBGParam == null)
            {
                // 音速启动 (Sonic Launcher) 经典金属银灰质感渐变背景
                LinearGradientBrush sonicBrush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1)
                };
                sonicBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#DDE2E8"), 0.0));
                sonicBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#BAC3CE"), 0.08));
                sonicBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFFF"), 0.081));
                sonicBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F8F9FA"), 1.0));
                MainWindow.mainWindow.BGBorder.Background = sonicBrush;
                return;
            }

            if (appConfig.BGStyle == BGStyle.ImgBac || appConfig.BGStyle == 0)
            {
                if (appConfig.BitmapImage != null)
                {
                    Image image = new Image
                    {
                        Effect = new BlurEffect()
                        {
                            Radius = appConfig.BlurValue
                        },
                        Margin = new Thickness(-30),
                        Source = appConfig.BitmapImage,
                        Opacity = (double)(Math.Round((decimal)(appConfig.BgOpacity / 100.00), 2))
                    };

                    Grid grid = new Grid
                    {
                        ClipToBounds = true
                    };
                    grid.Children.Add(image);

                    VisualBrush vb = new VisualBrush
                    {
                        Visual = grid
                    };
                    MainWindow.mainWindow.BGBorder.Background = vb;
                }
                else
                {
                    LinearGradientBrush sonicBrush = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(0, 1)
                    };
                    sonicBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#DDE2E8"), 0.0));
                    sonicBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#BAC3CE"), 0.08));
                    sonicBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFFF"), 0.081));
                    sonicBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F8F9FA"), 1.0));
                    MainWindow.mainWindow.BGBorder.Background = sonicBrush;
                }
            }
            else
            {
                LinearGradientBrush lgb = new LinearGradientBrush();
                GradientStop gs = new GradientStop
                {
                    Color = (Color)ColorConverter.ConvertFromString(appConfig.GradientBGParam.Color1),
                    Offset = 0
                };

                lgb.GradientStops.Add(gs);

                GradientStop gs2 = new GradientStop
                {
                    Color = (Color)ColorConverter.ConvertFromString(appConfig.GradientBGParam.Color2),
                    Offset = 1
                };
                lgb.GradientStops.Add(gs2);
                lgb.Opacity = (double)(Math.Round((decimal)(appConfig.BgOpacity / 100.00), 2));
                MainWindow.mainWindow.BGBorder.Background = lgb;
            }
        }
    }
}
