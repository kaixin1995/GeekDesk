using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace GeekDesk.Util
{
    public class ScreenUtil
    {

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        //取得前台窗口句柄函数 
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        //取得桌面窗口句柄函数 
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        //取得Shell窗口句柄函数 
        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();
        //取得窗口大小函数 
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowRect(IntPtr hwnd, out RECT rc);
        //获取窗口标题
        [DllImport("user32", SetLastError = true)]
        private static extern int GetWindowText(
            IntPtr hWnd,//窗口句柄
            StringBuilder lpString,//标题
            int nMaxCount //最大值
            );

        //获取类的名字
        [DllImport("user32.dll")]
        private static extern int GetClassName(
            IntPtr hWnd,//句柄
            StringBuilder lpString, //类名
            int nMaxCount); //最大值

        /// <summary>
        /// 判断当前屏幕(鼠标最后活动屏幕)是否有全屏化应用
        /// </summary>
        /// <returns></returns>
        public static bool IsPrimaryFullScreen()
        {

            //桌面窗口句柄 
            IntPtr desktopHandle; //Window handle for the desktop  
                                  //Shell窗口句柄 
            IntPtr shellHandle; //Window handle for the shell  因为桌面窗口和Shell窗口也是全屏，要排除在其他全屏程序之外。               //取得桌面和Shell窗口句柄 
            desktopHandle = GetDesktopWindow();
            shellHandle = GetShellWindow();    //取得前台窗口句柄并判断是否全屏 
            bool runningFullScreen = false;
            RECT appBounds;
            Rectangle screenBounds;
            IntPtr hWnd;
            //取得前台窗口 
            hWnd = GetForegroundWindow();

            StringBuilder sb = new StringBuilder(256);
            try
            {
                GetClassName(hWnd, sb, sb.Capacity);
            }
            catch { }
            if (sb.ToString().ToLower().Equals("workerw")) return false;
            if (hWnd != IntPtr.Zero)
            {
                //判断是否桌面或shell        
                if (!(hWnd.Equals(desktopHandle) || hWnd.Equals(shellHandle)))
                {
                    //取得窗口大小 
                    GetWindowRect(hWnd, out appBounds);
                    //判断是否全屏 
                    screenBounds = Screen.FromHandle(hWnd).Bounds;
                    if ((appBounds.bottom - appBounds.top) == screenBounds.Height
                        && (appBounds.right - appBounds.left) == screenBounds.Width)
                        runningFullScreen = true;
                }
            }
            return runningFullScreen;
        }


        public static Color GetColorAt(System.Drawing.Point location)
        {
            Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);

            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return screenPixel.GetPixel(0, 0);
        }


        [DllImport("gdi32")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        public const int HORZRES = 8;
        public const int VERTRES = 10;
        public const int DESKTOPVERTRES = 117;
        public const int DESKTOPHORZRES = 118;
        /// <summary>
        /// 获取屏幕缩放比例 (基于系统 DPI，100% = 1.0, 125% = 1.25, 150% = 1.5)
        /// </summary>
        /// <returns></returns>
        public static double GetScreenScalingFactor()
        {
            try
            {
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr desktop = g.GetHdc();
                    try
                    {
                        int dpiY = GetDeviceCaps(desktop, 90); // LOGPIXELSY = 90
                        if (dpiY > 0)
                        {
                            return dpiY / 96.0;
                        }
                    }
                    finally
                    {
                        g.ReleaseHdc(desktop);
                    }
                }
            }
            catch
            {
            }
            return 1.0;
        }

        #region 逐显示器 DPI 与工作区 (分辨率/缩放变更后定位的正确依据)

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT_PT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFO_EX
        {
            public int cbSize;
            public int rcMonitorLeft;
            public int rcMonitorTop;
            public int rcMonitorRight;
            public int rcMonitorBottom;
            public int rcWorkLeft;
            public int rcWorkTop;
            public int rcWorkRight;
            public int rcWorkBottom;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT_PT pt, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO_EX lpmi);

        [DllImport("Shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hMonitor, int dpiType, out uint dpiX, out uint dpiY);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref POINT_PT lpPoint);

        private const uint MONITOR_DEFAULTTONEAREST = 2;
        private const int MDT_EFFECTIVE_DPI = 0;

        /// <summary>
        /// 获取鼠标所在显示器的有效缩放与工作区 (DIP 单位)。
        /// 使用逐显示器 DPI (GetDpiForMonitor), 分辨率/缩放变更后即时有效;
        /// 旧 GetScreenScalingFactor 取的是主屏系统 DPI, 混合 DPI 多屏或变更后会算错坐标。
        /// </summary>
        /// <returns>失败返回 false (调用方回退旧算法)</returns>
        public static bool TryGetWorkAreaAtMouse(out double scale, out double workLeft, out double workTop, out double workWidth, out double workHeight)
        {
            scale = 1.0;
            workLeft = workTop = workWidth = workHeight = 0;
            try
            {
                POINT_PT pt = new POINT_PT();
                if (!GetCursorPos(ref pt)) return false;

                IntPtr hMonitor = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
                if (hMonitor == IntPtr.Zero) return false;

                uint dpiX, dpiY;
                if (GetDpiForMonitor(hMonitor, MDT_EFFECTIVE_DPI, out dpiX, out dpiY) != 0 || dpiY == 0)
                {
                    return false;
                }
                scale = dpiY / 96.0;

                MONITORINFO_EX mi = new MONITORINFO_EX();
                mi.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(MONITORINFO_EX));
                if (!GetMonitorInfo(hMonitor, ref mi)) return false;

                workLeft = mi.rcWorkLeft / scale;
                workTop = mi.rcWorkTop / scale;
                workWidth = (mi.rcWorkRight - mi.rcWorkLeft) / scale;
                workHeight = (mi.rcWorkBottom - mi.rcWorkTop) / scale;
                return workWidth > 0 && workHeight > 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion

    }
}
