using GeekDesk.Constant;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace GeekDesk.Util
{
    public class ShowWindowFollowMouse
    {

        public enum MousePosition
        {
            CENTER = 1,
            LEFT_TOP = 2,
            LEFT_BOTTOM = 3,
            RIGHT_TOP = 4,
            RIGHT_BOTTOM = 5,
            LEFT_CENTER = 6,
            RIGHT_CENTER = 7
        }

        // Constants for DPI
        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;

        private static float GetDpi(bool isX)
        {
            IntPtr hdc = WindowUtil.GetDC(IntPtr.Zero);
            int dpi = isX ? WindowUtil.GetDeviceCaps(hdc, LOGPIXELSX) : WindowUtil.GetDeviceCaps(hdc, LOGPIXELSY);
            WindowUtil.ReleaseDC(IntPtr.Zero, hdc);
            return dpi / 96f;
        }

        public static void Show(Window window, MousePosition position, double widthOffset = 0, double heightOffset = 0)
        {
            //获取鼠标位置
            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;

            // 获取鼠标所在的屏幕
            Screen currentScreen = Screen.FromPoint(p);
            float dpiX = GetDpi(true);
            float dpiY = GetDpi(false);

            p.X = (int)(p.X / dpiX);
            p.Y = (int)(p.Y / dpiY);

            //工作区宽度
            double screenWidth = currentScreen.WorkingArea.Width / dpiX;
            //工作区高度
            double screenHeight = currentScreen.WorkingArea.Height / dpiY;
            double screenX = currentScreen.WorkingArea.X / dpiX;
            double screenY = currentScreen.WorkingArea.Y / dpiY;

            //判断是否超出最左边缘
            if (p.X - window.Width / 2 < screenX)
            {
                //超出最左边缘
                window.Left = screenX - Constants.SHADOW_WIDTH;
            } else
            {
                window.Left = p.X - window.Width / 2 + Constants.SHADOW_WIDTH;
            }

            //判断是否超出最右边缘
            if (p.X + window.Width / 2 > screenWidth + screenX)
            {
                //超出最右边缘
                window.Left = screenWidth + screenX - window.Width + Constants.SHADOW_WIDTH;
            }
            

            //判断是否超出最上边缘
            if (p.Y - window.Height / 2 < screenY)
            {
                //超出最上边缘
                window.Top = screenY - Constants.SHADOW_WIDTH;
            } else
            {
                window.Top = p.Y - window.Height / 2 + Constants.SHADOW_WIDTH;
            }

            //判断是否超出最下边缘
            if (p.Y + window.Height / 2 > screenHeight + screenY)
            {
                //超出最下边缘
                window.Top = screenHeight + screenY - window.Height + Constants.SHADOW_WIDTH;
            }
            





            //// 显示屏幕信息
            //Console.WriteLine("鼠标当前所在屏幕的信息:");
            //Console.WriteLine($"屏幕设备名称: {currentScreen.DeviceName}");
            //Console.WriteLine($"屏幕分辨率: {currentScreen.Bounds.Width}x{currentScreen.Bounds.Height}");
            //Console.WriteLine($"屏幕工作区: {currentScreen.WorkingArea}");
            //Console.WriteLine($"主屏幕: {currentScreen.Primary}");

            //Console.WriteLine(p.X + "=" + p.Y);
            //float dpiX = GetDpi(true);
            //float dpiY = GetDpi(false);
            //p.X = (int)(p.X / dpiX);
            //p.Y = (int)(p.Y / dpiY);
            //Console.WriteLine(p.X + "=" + p.Y);
            //double left = SystemParameters.VirtualScreenLeft;
            //double top = SystemParameters.VirtualScreenTop;
            //double width = SystemParameters.VirtualScreenWidth;
            //double height = SystemParameters.WorkArea.Bottom;

            //Console.WriteLine("VirtualScreenTop:" + SystemParameters.VirtualScreenTop);
            //Console.WriteLine("VirtualScreenLeft:" + SystemParameters.VirtualScreenLeft);
            //Console.WriteLine("VirtualScreenWidth:" + SystemParameters.VirtualScreenWidth);
            //Console.WriteLine("VirtualScreenHeight:" + SystemParameters.VirtualScreenHeight);
            //Console.WriteLine("WorkArea.Bottom:" + SystemParameters.WorkArea.Bottom);
            //Console.WriteLine(" window.Height:" + window.Height);
            //double right = width - Math.Abs(left);
            //double bottom = height - Math.Abs(top);

            //double afterWidth;
            //double afterHeight;

            //switch (position)
            //{

            //    case MousePosition.LEFT_BOTTOM:
            //        afterWidth = 0;
            //        afterHeight = window.Height;
            //        break;
            //    case MousePosition.LEFT_TOP:
            //        afterWidth = 0;
            //        afterHeight = 0;
            //        break;
            //    case MousePosition.LEFT_CENTER:
            //        afterWidth = 0;
            //        afterHeight = window.Height / 2;
            //        break;
            //    case MousePosition.RIGHT_BOTTOM:
            //        afterWidth = window.Width;
            //        afterHeight = window.Height;
            //        break;
            //    case MousePosition.RIGHT_TOP:
            //        afterWidth = window.Width;
            //        afterHeight = 0;
            //        break;
            //    case MousePosition.RIGHT_CENTER:
            //        afterWidth = window.Width;
            //        afterHeight = window.Height / 2;
            //        break;
            //    default:
            //        afterWidth = window.Width / 2;
            //        afterHeight = window.Height / 2;
            //        break;
            //}

            //afterWidth += widthOffset;
            //afterHeight -= heightOffset;

            //if (p.X - afterWidth < left)
            //{
            //    //判断是否在最左边缘
            //    window.Left = left - Constants.SHADOW_WIDTH;
            //}
            //else if (p.X + afterWidth > right)
            //{
            //    //判断是否在最右边缘
            //    window.Left = right - window.Width + Constants.SHADOW_WIDTH;
            //}
            //else
            //{
            //    window.Left = p.X - afterWidth;
            //}


            //if (p.Y - afterHeight < top)
            //{
            //    //判断是否在最上边缘
            //    window.Top = top - Constants.SHADOW_WIDTH;
            //}
            //else if (p.Y + afterHeight > bottom)
            //{
            //    Console.WriteLine("p.Y:" + p.Y);
            //    Console.WriteLine("afterHeight:" + afterHeight);
            //    Console.WriteLine("bottom:" + bottom);
            //    //判断是否在最下边缘
            //    window.Top = bottom - window.Height + Constants.SHADOW_WIDTH;
            //}
            //else
            //{
            //    window.Top = p.Y - afterHeight;
            //}
        }

    }
}
