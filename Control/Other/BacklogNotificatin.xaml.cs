using GeekDesk.Constant;
using GeekDesk.Task;
using GeekDesk.Util;
using GeekDesk.ViewModel;
using HandyControl.Controls;
using Quartz;
using System;
using System.Windows;
using System.Windows.Input;

namespace GeekDesk.Control.Other
{
    /// <summary>
    /// BacklogNotificatin.xaml 的交互逻辑
    /// </summary>
    public partial class BacklogNotificatin
    {

        //属性方式读取, 保证备份恢复等替换 appData 实例后仍使用最新数据
        private AppData appData => MainWindow.appData;
        public BacklogNotificatin(ToDoInfo info)
        {
            InitializeComponent();
            this.DataContext = info;
        }

        private void BacklogDone_Click(object sender, RoutedEventArgs e)
        {
            ToDoInfo info = this.DataContext as ToDoInfo;
            info.DoneTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (info.ExecType == TodoTaskExecType.CRON)
            {
                CronExpression exp = new CronExpression(info.Cron);
                DateTime nowTime = DateTime.Now;

                //计算下次执行时间
                DateTimeOffset? next = exp.GetNextValidTimeAfter(nowTime);
                if (next == null)
                {
                    //无效 CRON 表达式 按普通待办归档
                    appData.ToDoList.Remove(info);
                    appData.HiToDoList.Add(info);
                    CloseActiveNotification(info);
                    CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
                    return;
                }
                DateTime nextTime = DateTime.SpecifyKind(next.Value.LocalDateTime, DateTimeKind.Local);

                string nextTimeStr = nextTime.ToString("yyyy-MM-dd HH:mm:ss");
                info.ExeTime = nextTimeStr;

                TimeSpan ts = nextTime.Subtract(nowTime);
                int minutes = (int)Math.Ceiling(ts.TotalMinutes);
                if (minutes < 0)
                {
                    minutes = 0;
                }
                if (minutes > 60)
                {
                    int m = minutes % 60;
                    int h = minutes / 60;
                    Growl.SuccessGlobal("下次任务将在 " + h + " 小时零 " + m + " 分钟后提醒您!");
                }
                else
                {
                    Growl.SuccessGlobal("下次任务将在 " + minutes + " 分钟后提醒您!");
                }
            }
            else
            {
                appData.ToDoList.Remove(info); //执行任务删除
                appData.HiToDoList.Add(info);  //添加历史任务
            }
            CloseActiveNotification(info);
            CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
        }

        /// <summary>
        /// 关闭并清除该待办的活动提醒 (防重复点击/竞态导致的 KeyNotFound)
        /// </summary>
        private static void CloseActiveNotification(ToDoInfo info)
        {
            if (ToDoTask.activityBacklog.TryGetValue(info, out ToDoTask.BacklogReminder reminder))
            {
                reminder.Notification?.Close();
                ToDoTask.activityBacklog.Remove(info);
            }
        }


        /// <summary>
        /// 只允许输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelayTime_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int textBoxInt;
            //转化按下的键为数字，如果不是数字则会抓取到报错信息，不键入，反之则键入
            try
            {
                textBoxInt = int.Parse($"{e.Text}");
            }
            catch (FormatException)
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// 失去焦点前如果为空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelayTime_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //非法输入重置为默认10分钟
            if (!int.TryParse(DelayTime.Text.Trim(), out int val) || val < 1)
            {
                DelayTime.Text = "10";
            }
        }

        /// <summary>
        /// 推迟提醒
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelayButton_Click(object sender, RoutedEventArgs e)
        {
            ToDoInfo info = this.DataContext as ToDoInfo;
            if (!int.TryParse(DelayTime.Text.Trim(), out int time) || time < 1)
            {
                time = 10;
            }
            if (time > 999999)
            {
                time = 999999;
            }
            string type = DelayType.Text;
            switch (type)
            {
                case "分":
                    info.ExeTime = DateTime.Now.AddMinutes(time).ToString("yyyy-MM-dd HH:mm:ss");
                    Growl.SuccessGlobal("将在 " + time + " 分钟后再次提醒您!");
                    break;
                case "时":
                    info.ExeTime = DateTime.Now.AddHours(time).ToString("yyyy-MM-dd HH:mm:ss");
                    Growl.SuccessGlobal("将在 " + time + " 小时后再次提醒您!");
                    break;
            }
            CloseActiveNotification(info);
            CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
        }
    }
}
