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
        //属性方式读取, 保证备份恢复等替换 appData 实例后仍取到最新配置
        private static AppConfig appConfig => MainWindow.appData.AppConfig;
        public static void BGSetting()
        {
            if (MainWindow.mainWindow == null || MainWindow.mainWindow.BGBorder == null) return;

            if (appConfig.BitmapImage == null && appConfig.GradientBGParam == null)
            {
                ApplyThemeBackground();
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
                    ApplyThemeBackground();
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

        public static void ApplyThemeBackground()
        {
            if (MainWindow.mainWindow?.BGBorder == null) return;

            switch (appConfig.VisualTheme)
            {
                case VisualTheme.MacAcrylic:
                    // 1. macOS 磨砂黑质感 (深色深空灰专业风)
                    LinearGradientBrush darkBrush = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(0, 1)
                    };
                    darkBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#232832"), 0.0));
                    darkBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#181B21"), 1.0));
                    MainWindow.mainWindow.BGBorder.Background = darkBrush;
                    MainWindow.mainWindow.BGBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E4756"));
                    break;

                case VisualTheme.PureMinimal:
                    // 2. 纯净极简纯白现代风
                    MainWindow.mainWindow.BGBorder.Background = Brushes.White;
                    MainWindow.mainWindow.BGBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CBD5E1"));
                    break;

                case VisualTheme.CrystalSkeuomorphic:
                default:
                    // 3. 经典音速金属微拟物风 (经典银灰拉丝高光质感)
                    LinearGradientBrush crystalBrush = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(0, 1)
                    };
                    crystalBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F5F7FA"), 0.0));
                    crystalBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#D5DEE7"), 0.08));
                    crystalBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#E2E8EE"), 0.081));
                    crystalBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#CAD4DF"), 1.0));
                    MainWindow.mainWindow.BGBorder.Background = crystalBrush;
                    MainWindow.mainWindow.BGBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8495A5"));
                    break;
            }
        }
    }
}
