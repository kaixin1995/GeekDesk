using GeekDesk.Interface;
using GeekDesk.Util;
using HandyControl.Controls;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Cursors = System.Windows.Input.Cursors;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace GeekDesk.Control.Windows
{
    /// <summary>
    /// ColorPickerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PixelColorPickerWindow : IWindowCommon
    {
        private static int PIXEL_REC_LENGTH = 10;

        private static readonly int MIN_LENGTH = 10;
        private static readonly int MAX_LENGTH = 50;

        //private static System.Drawing.Bitmap bgBitmap;

        private static RenderTargetBitmap renderTargetBitmap;

        private readonly ColorPicker colorPicker;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        public PixelColorPickerWindow(ColorPicker colorPicker)
        {
            InitializeComponent();
            this.colorPicker = colorPicker;

            try
            {
                SetProcessDPIAware();
            }
            catch (Exception e) { }
            ColorPickerWindow_Init();
        }

        private void ColorPickerWindow_Init()
        {

            this.WindowState = WindowState.Normal;//还原窗口（非最小化和最大化）

            //虚拟屏幕 (含所有显示器的包围区域, 正确处理负坐标多显示器)
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
            this.Left = SystemParameters.VirtualScreenLeft;
            this.Top = SystemParameters.VirtualScreenTop;

            //获取缩放比例
            double scale = ScreenUtil.GetScreenScalingFactor();

            DesktopBG.Width = this.Width;
            DesktopBG.Height = this.Height;
            this.Topmost = true;

            System.Drawing.Bitmap bgBitmap = new System.Drawing.Bitmap(
                    (int)(Width * scale),
                    (int)(Height * scale),
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );

            try
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bgBitmap))
                {
                    g.CopyFromScreen(
                        (int)(this.Left * scale),
                        (int)(this.Top * scale),
                        0,
                        0,
                        bgBitmap.Size
                        );
                }
                IntPtr hBitmap = bgBitmap.GetHbitmap();
                try
                {
                    BitmapSource bs = Imaging.CreateBitmapSourceFromHBitmap(
                                                 hBitmap,
                                                IntPtr.Zero,
                                                Int32Rect.Empty,
                                                BitmapSizeOptions.FromEmptyOptions()
                                            );
                    DesktopBG.Source = bs;
                }
                finally
                {
                    //释放GDI位图句柄 防止泄漏
                    DeleteObject(hBitmap);
                }
            }
            finally
            {
                bgBitmap.Dispose();
            }

            VisualBrush b = (VisualBrush)PixelBG.Fill;
            b.Visual = DesktopBG;
            Mouse.OverrideCursor = Cursors.Cross;


        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 创建一个RenderTargetBitmap，捕获窗口的内容
            renderTargetBitmap = new RenderTargetBitmap((int)this.Width, (int)this.Height, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(this);

            SetPixelAbout(null);
        }




        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                //恢复系统光标 (否则关闭后全局保持十字光标)
                Mouse.OverrideCursor = null;
                this.DataContext = null;
                this.Close();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.OverrideCursor = null;
            Point pos = e.MouseDevice.GetPosition(DesktopBG);
            colorPicker.SelectedBrush = new SolidColorBrush(GetColorAtPosition(Mouse.GetPosition(this)));
            this.Close();
            ClickColorPickerToggleButton(colorPicker);
        }

        public void ClickColorPickerToggleButton(ColorPicker colorPicker)
        {
            const BindingFlags InstanceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = colorPicker.GetType();
            FieldInfo fi = type.GetField("_toggleButtonDropper", InstanceBindFlags);

            ToggleButton tb = (ToggleButton)fi.GetValue(colorPicker);
            if (tb != null && tb.IsChecked == true)
            {
                tb.IsChecked = false;
                MethodInfo mi = type.GetMethod("ToggleButtonDropper_Click", InstanceBindFlags);
                mi.Invoke(colorPicker, new object[] { null, null });
            }
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            SetPixelAbout(e);
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr onj);


        // Constants for DPI
        private const int HORZRES = 8;
        private const int VERTRES = 10;
        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;
        private static float GetDpi(bool isX)
        {
            IntPtr hdc = WindowUtil.GetDC(IntPtr.Zero);
            int dpi = isX ? WindowUtil.GetDeviceCaps(hdc, LOGPIXELSX) : WindowUtil.GetDeviceCaps(hdc, LOGPIXELSY);
            WindowUtil.ReleaseDC(IntPtr.Zero, hdc);
            return dpi / 96f;
        }

        private void SetPixelAbout(MouseEventArgs e)
        {
            VisualBrush b = (VisualBrush)PixelBG.Fill;

            Point pos = Mouse.GetPosition(this);


            Rect viewBox = b.Viewbox;

            viewBox.Width = PIXEL_REC_LENGTH;
            viewBox.Height = PIXEL_REC_LENGTH;

            viewBox.X = pos.X - PIXEL_REC_LENGTH / 2;
            viewBox.Y = pos.Y - PIXEL_REC_LENGTH / 2;
            b.Viewbox = viewBox;

            double x = pos.X + 10;
            double y = pos.Y + 10;



            //获取缩放比例
            double scale = ScreenUtil.GetScreenScalingFactor();
            if (x + ColorCanvas.Width > this.Width / scale)
            {
                x = pos.X - ColorCanvas.Width - 10;
            }
            if (y + ColorCanvas.Height > this.Height / scale)
            {
                y = pos.Y - ColorCanvas.Height - 10;
            }


            Canvas.SetLeft(ColorCanvas, x);
            Canvas.SetTop(ColorCanvas, y);




            Color wColor = GetColorAtPosition(pos);

            System.Drawing.Color dColor = System.Drawing.Color.FromArgb(wColor.A, wColor.R, wColor.G, wColor.B);

            //已知色名 (如 Black) 不是十六进制, 不能截断; 未知颜色 Name 为 AARRGGBB 十六进制, 截掉 AA 两位
            PixelColor_HTML.Text = dColor.IsKnownColor
                ? "#" + dColor.Name.ToUpper()
                : "#" + dColor.Name.ToUpper().Substring(2);
            PixelColor_RGB.Text = dColor.R + "," + dColor.G + "," + dColor.B;
            Pixel_XY.Text = (int)pos.X + "*" + (int)pos.Y;

            SolidColorBrush scb = (SolidColorBrush)PixelColor.Fill;
            scb.Color = wColor;
            //scb.Color = Color.FromArgb(dColor.A, dColor.R, dColor.G, dColor.B);
        }

        private Color GetColorAtPosition(Point position)
        {
            //边缘位置钳制 防止裁剪区域越界抛异常
            int px = Math.Max(0, Math.Min((int)position.X, (int)renderTargetBitmap.Width - 1));
            int py = Math.Max(0, Math.Min((int)position.Y, (int)renderTargetBitmap.Height - 1));
            // 使用CroppedBitmap裁剪出鼠标位置的一个像素
            CroppedBitmap croppedBitmap = new CroppedBitmap(renderTargetBitmap, new Int32Rect(px, py, 1, 1));
            // 将像素数据复制到数组中
            byte[] pixels = new byte[4];
            croppedBitmap.CopyPixels(pixels, 4, 0);
            return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
        }




        /// <summary>
        /// 滚轮控制缩放区域
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (e.Delta < 0 && PIXEL_REC_LENGTH < MAX_LENGTH)
            {
                //缩小
                PIXEL_REC_LENGTH += 5;
            }
            else if (e.Delta > 0 && PIXEL_REC_LENGTH > MIN_LENGTH)
            {
                //放大
                PIXEL_REC_LENGTH -= 5;
            }
            SetPixelAbout(e);
        }

        /// <summary>
        /// 右键按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.OverrideCursor = null;
            GlobalColorPickerWindow.ShowOrHide();
            //关闭
            this.Close();
        }

    }
}
