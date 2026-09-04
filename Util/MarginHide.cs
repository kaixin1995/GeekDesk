using GeekDesk.Constant;
using GeekDesk.MyThread;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace GeekDesk.Util
{

    public enum HideType
    {
        TOP_SHOW = 1,
        LEFT_SHOW = 2,
        RIGHT_SHOW = 3,
        TOP_HIDE = 4,
        LEFT_HIDE = 5,
        RIGHT_HIDE = 6
    }


    public class MarginHide
    {
        private static Window window;//定义使用该方法的窗体

        private static readonly int hideTime = 65;
        private static readonly int showTime = 15;

        private static int animalTime;

        private static readonly int fadeHideTime = 50;
        private static readonly int fadeShowTime = 50;
        private static readonly int taskTime = 200;

        public static readonly int shadowWidth = 20;

        public static bool ON_HIDE = false;


        private static double showMarginWidth = 2;

        public static bool IS_HIDE = false;

        public static HideType CURRENT_DOCK = HideType.RIGHT_HIDE;

        private static System.Windows.Forms.Timer timer = null;

        public static void ReadyHide(Window window)
        {
            MarginHide.window = window;
        }


        /// <summary>
        /// 窗体是否贴边
        /// </summary>
        /// <returns></returns>
        public static bool IsMargin()
        {
            double screenLeft = SystemParameters.VirtualScreenLeft;
            double screenTop = SystemParameters.VirtualScreenTop;
            double screenWidth = SystemParameters.VirtualScreenWidth;

            double windowWidth = window.Width;

            double windowTop = window.Top;
            double windowLeft = window.Left;

            //窗体是否贴边
            return (windowLeft <= screenLeft
                || windowTop <= screenTop
                || windowLeft + windowWidth + Math.Abs(screenLeft) >= screenWidth);
        }



        #region 窗体贴边隐藏功能
        private static void HideWindow(object o, EventArgs e)
        {
            if (RunTimeStatus.MARGIN_HIDE_AND_OTHER_SHOW 
                || RunTimeStatus.LOCK_APP_PANEL) return;

            double screenLeft = SystemParameters.VirtualScreenLeft;
            double screenTop = SystemParameters.VirtualScreenTop;
            double screenWidth = SystemParameters.VirtualScreenWidth;

            double windowHeight = window.Height;
            double windowWidth = window.Width;

            double windowTop = window.Top;
            double windowLeft = window.Left;

            //获取鼠标位置
            System.Windows.Point p = MouseUtil.GetMousePosition();
            double mouseX = p.X;
            double mouseY = p.Y;

            //鼠标不在窗口上 (离开窗口且未隐藏，则缩回边缘)
            if ((mouseX < windowLeft || mouseX > windowLeft + windowWidth
                || mouseY < windowTop || mouseY > windowTop + windowHeight) && !IS_HIDE && window.Visibility == Visibility.Visible)
            {
                //上方隐藏条件
                if (windowTop <= screenTop + 5)
                {
                    IS_HIDE = true;
                    CURRENT_DOCK = HideType.TOP_HIDE;
                    HideAnimation(windowTop, screenTop - windowHeight + showMarginWidth, Window.TopProperty, HideType.TOP_HIDE);
                    return;
                }
                //左侧隐藏条件
                if (windowLeft <= screenLeft + 5)
                {
                    IS_HIDE = true;
                    CURRENT_DOCK = HideType.LEFT_HIDE;
                    HideAnimation(windowLeft, screenLeft - windowWidth + showMarginWidth, Window.LeftProperty, HideType.LEFT_HIDE);
                    return;
                }
                //右侧隐藏条件
                if (windowLeft + windowWidth + Math.Abs(screenLeft) >= screenWidth - 5)
                {
                    IS_HIDE = true;
                    CURRENT_DOCK = HideType.RIGHT_HIDE;
                    HideAnimation(windowLeft, screenWidth - Math.Abs(screenLeft) - showMarginWidth, Window.LeftProperty, HideType.RIGHT_HIDE);
                    return;
                }
            }
            // 鼠标触碰停靠边缘或在隐藏细条上时，自动滑出
            else if (IS_HIDE && window.Visibility != Visibility.Visible)
            {
                bool hitDock = false;
                if (CURRENT_DOCK == HideType.LEFT_HIDE && mouseX <= screenLeft + 5 && mouseY >= windowTop && mouseY <= windowTop + windowHeight)
                {
                    hitDock = true;
                }
                else if (CURRENT_DOCK == HideType.RIGHT_HIDE && mouseX >= screenWidth - Math.Abs(screenLeft) - 5 && mouseY >= windowTop && mouseY <= windowTop + windowHeight)
                {
                    hitDock = true;
                }
                else if (CURRENT_DOCK == HideType.TOP_HIDE && mouseY <= screenTop + 5 && mouseX >= windowLeft && mouseX <= windowLeft + windowWidth)
                {
                    hitDock = true;
                }
                else if (mouseX >= windowLeft && mouseX <= windowLeft + windowWidth && mouseY >= windowTop && mouseY <= windowTop + windowHeight)
                {
                    hitDock = true;
                }

                if (hitDock)
                {
                    window.Show();
                    //上方显示
                    if (CURRENT_DOCK == HideType.TOP_HIDE || windowTop <= screenTop - showMarginWidth + 5)
                    {
                        IS_HIDE = false;
                        HideAnimation(windowTop, screenTop, Window.TopProperty, HideType.TOP_SHOW);
                        return;
                    }
                    //左侧显示
                    if (CURRENT_DOCK == HideType.LEFT_HIDE || windowLeft <= screenLeft - showMarginWidth + 5)
                    {
                        IS_HIDE = false;
                        HideAnimation(windowLeft, screenLeft, Window.LeftProperty, HideType.LEFT_SHOW);
                        return;
                    }
                    //右侧显示
                    if (CURRENT_DOCK == HideType.RIGHT_HIDE || windowLeft + Math.Abs(screenLeft) >= screenWidth - showMarginWidth - 5)
                    {
                        IS_HIDE = false;
                        HideAnimation(windowLeft, screenWidth - Math.Abs(screenLeft) - windowWidth, Window.LeftProperty, HideType.RIGHT_SHOW);
                        return;
                    }
                }
            }

        }
        #endregion 


        public static void StartHide()
        {
            ON_HIDE = true;
            if (timer != null) return;
            timer = new System.Windows.Forms.Timer
            {
                Interval = taskTime
            };//添加timer计时器，隐藏功能
            timer.Tick += HideWindow;
            timer.Start();
        }

        public static void StopHide()
        {
            ON_HIDE = false;
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
            if (IS_HIDE)
            {
                ForceShowFromMargin();
            }
        }

        public static void ForceShowFromMargin()
        {
            if (window == null) return;
            double screenLeft = SystemParameters.VirtualScreenLeft;
            double screenTop = SystemParameters.VirtualScreenTop;
            double screenWidth = SystemParameters.VirtualScreenWidth;
            double windowWidth = window.Width;
            double windowTop = window.Top;
            double windowLeft = window.Left;

            window.Visibility = Visibility.Visible;
            window.Show();

            if (CURRENT_DOCK == HideType.TOP_HIDE || windowTop <= screenTop - showMarginWidth + 10)
            {
                IS_HIDE = false;
                HideAnimation(windowTop, screenTop, Window.TopProperty, HideType.TOP_SHOW);
            }
            else if (CURRENT_DOCK == HideType.LEFT_HIDE || windowLeft <= screenLeft - showMarginWidth + 10)
            {
                IS_HIDE = false;
                HideAnimation(windowLeft, screenLeft, Window.LeftProperty, HideType.LEFT_SHOW);
            }
            else if (CURRENT_DOCK == HideType.RIGHT_HIDE || windowLeft + Math.Abs(screenLeft) >= screenWidth - showMarginWidth - 10)
            {
                IS_HIDE = false;
                HideAnimation(windowLeft, screenWidth - Math.Abs(screenLeft) - windowWidth, Window.LeftProperty, HideType.RIGHT_SHOW);
            }
            else
            {
                IS_HIDE = false;
            }
        }


        private static void HideAnimation(double from, double to, DependencyProperty property, HideType hideType)
        {
            if (window == null) return;
            window.Dispatcher.Invoke(() =>
            {
                double targetPos = to;
                switch (hideType)
                {
                    case HideType.LEFT_SHOW:
                        targetPos = to - 20;
                        break;
                    case HideType.RIGHT_SHOW:
                        targetPos = to + 20;
                        break;
                    case HideType.TOP_SHOW:
                        targetPos = to - 20;
                        break;
                }

                DependencyProperty animProperty = (hideType == HideType.TOP_HIDE || hideType == HideType.TOP_SHOW)
                    ? Window.TopProperty
                    : Window.LeftProperty;

                double animDurationMs = (hideType <= HideType.RIGHT_SHOW) ? 120 : 180;

                DoubleAnimation anim = new DoubleAnimation
                {
                    From = from,
                    To = targetPos,
                    Duration = TimeSpan.FromMilliseconds(animDurationMs),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                anim.Completed += (s, e) =>
                {
                    window.BeginAnimation(animProperty, null);
                    if (animProperty == Window.TopProperty)
                    {
                        window.Top = targetPos;
                    }
                    else
                    {
                        window.Left = targetPos;
                    }

                    if (hideType > HideType.RIGHT_SHOW)
                    {
                        window.Visibility = Visibility.Collapsed;
                    }
                };

                window.BeginAnimation(animProperty, anim);
            });
        }

        private static void FadeAnimation(double from, double to)
        {
            new Thread(() =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    double time;
                    if (to == 0D)
                    {
                        time = fadeHideTime;
                    }
                    else
                    {
                        time = fadeShowTime;
                    }
                    DoubleAnimation opacityAnimation = new DoubleAnimation
                    {
                        From = from,
                        To = to,
                        Duration = new Duration(TimeSpan.FromMilliseconds(time))
                    };
                    opacityAnimation.Completed += (s, e) =>
                    {
                        //window.Opacity = to;
                        window.BeginAnimation(Window.OpacityProperty, null);
                    };
                    Timeline.SetDesiredFrameRate(opacityAnimation, 60);
                    window.BeginAnimation(Window.OpacityProperty, opacityAnimation);
                });
            }).Start();
        }


        public static void WaitHide(int waitTime)
        {
            System.Threading.Thread t = new System.Threading.Thread(() =>
            {
                System.Threading.Thread.Sleep(waitTime);
                //修改状态为false 继续执行贴边隐藏
                RunTimeStatus.MARGIN_HIDE_AND_OTHER_SHOW = false;
            });
            t.IsBackground = true;
            t.Start();
        }





    }
}

