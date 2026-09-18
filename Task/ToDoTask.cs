using GeekDesk.Control.Other;
using GeekDesk.ViewModel;
using HandyControl.Controls;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Timers;

namespace GeekDesk.Task
{
    public class ToDoTask
    {

        /// <summary>
        /// 到期待办的活动提醒记录
        /// </summary>
        public class BacklogReminder
        {
            public Notification Notification;
            public DateTime LastShowTime;
            /// <summary>
            /// 通知是否仍在显示 (用户点通知自带 X 关闭后置 false, 超时未处理将重新提醒)
            /// </summary>
            public bool ContentAlive = true;
        }

        /// <summary>
        /// 用户手动关闭通知后, 间隔多久重新提醒 (分钟)
        /// </summary>
        private const int RENOTIFY_INTERVAL_MINUTES = 5;

        public static Dictionary<ToDoInfo, BacklogReminder> activityBacklog = new Dictionary<ToDoInfo, BacklogReminder>();

        public static void BackLogCheck()
        {

            System.Timers.Timer timer = new System.Timers.Timer
            {
                Enabled = true,
                Interval = 5000
            };
            timer.Start();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(Check);
        }


        private static void Check(object source, ElapsedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)(() =>
            {
                try
                {
                    if (MainWindow.appData.ToDoList.Count > 0)
                    {
                        string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        ObservableCollection<ToDoInfo> exeBacklogList = MainWindow.appData.ToDoList;
                        foreach (ToDoInfo info in exeBacklogList)
                        {
                            if (info.ExeTime.CompareTo(nowTime) == -1)
                            {
                                if (activityBacklog.TryGetValue(info, out BacklogReminder reminder))
                                {
                                    //通知被用户手动关闭后仍未处理, 超过间隔则重新提醒
                                    if (!reminder.ContentAlive
                                        && (DateTime.Now - reminder.LastShowTime).TotalMinutes >= RENOTIFY_INTERVAL_MINUTES)
                                    {
                                        ShowNotification(info);
                                    }
                                }
                                else
                                {
                                    ShowNotification(info);
                                }
                            }
                        }
                    }
                }
                catch (Exception e1) { }
                //ClearMemory();
            }));
        }

        /// <summary>
        /// 弹出待办提醒通知并记录活动提醒
        /// </summary>
        private static void ShowNotification(ToDoInfo info)
        {
            BacklogNotificatin content = new BacklogNotificatin(info);
            //用户点通知右上角 X 关闭时, 通知窗口关闭会触发内容控件 Unloaded, 据此标记以便后续重新提醒
            content.Unloaded += (s, e) =>
            {
                if (activityBacklog.TryGetValue(info, out BacklogReminder reminder))
                {
                    reminder.ContentAlive = false;
                }
            };
            Notification ntf = Notification.Show(content, ShowAnimation.Fade, true);
            activityBacklog[info] = new BacklogReminder
            {
                Notification = ntf,
                LastShowTime = DateTime.Now
            };
        }

        /// <summary>
        /// 释放内存
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }

        #region 内存回收
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        #endregion


    }
}
