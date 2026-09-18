using GeekDesk.Constant;
using GeekDesk.Control.Other;
using GeekDesk.ViewModel;
using HandyControl.Data;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using static GeekDesk.Control.Other.GlobalMsgNotification;

namespace GeekDesk.Util
{
    class CommonCode
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        /// <summary>
        /// 获取 app 数据 (采用 JSON 存储与读取，同时兼容尝试读取旧版数据)
        /// </summary>
        /// <returns></returns>
        public static AppData GetAppDataByFile()
        {
            AppData appData = null;
            if (!File.Exists(Constants.DATA_FILE_PATH))
            {
                appData = new AppData();
                SaveAppData(appData, Constants.DATA_FILE_PATH);
                return appData;
            }

            try
            {
                string json = File.ReadAllText(Constants.DATA_FILE_PATH, Encoding.UTF8);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    appData = JsonConvert.DeserializeObject<AppData>(json, JsonSettings);
                }

                if (appData != null)
                {
                    // 将菜单密码写入文件
                    if (!string.IsNullOrEmpty(appData.AppConfig?.MenuPassword))
                    {
                        SavePassword(appData.AppConfig.MenuPassword);
                    }

                    // 确保每个 MenuInfo 的 children 和属性不为空
                    FixMenuHierarchy(appData.MenuList);
                    return appData;
                }
            }
            catch (Exception ex)
            {
                LogUtil.WriteErrorLog(ex, "读取主数据文件异常，尝试从备份恢复...");
            }

            // 若主文件读取失败，则从备份目录读取
            DirectoryInfo dirInfo = new DirectoryInfo(Constants.DATA_FILE_BAK_DIR_PATH);
            if (dirInfo.Exists)
            {
                FileInfo[] files = dirInfo.GetFiles()
                    .Where(f => f.Extension.Equals(".bak", StringComparison.OrdinalIgnoreCase)).ToArray();
                if (files.Length > 0)
                {
                    FileInfo[] sortedFiles = files.OrderByDescending(file => file.CreationTime).ToArray();
                    foreach (FileInfo bakFile in sortedFiles)
                    {
                        try
                        {
                            string json = File.ReadAllText(bakFile.FullName, Encoding.UTF8);
                            appData = JsonConvert.DeserializeObject<AppData>(json, JsonSettings);
                            if (appData != null)
                            {
                                DialogMsg msg = new DialogMsg
                                {
                                    msg = "GeekDesk 当前主数据文件已自动从备份恢复：\n" + bakFile.FullName
                                };
                                GlobalMsgNotification gm = new GlobalMsgNotification(msg);
                                HandyControl.Controls.Notification ntf = HandyControl.Controls.Notification.Show(gm, ShowAnimation.Fade, true);
                                gm.ntf = ntf;

                                FixMenuHierarchy(appData.MenuList);
                                SaveAppData(appData, Constants.DATA_FILE_PATH);
                                return appData;
                            }
                        }
                        catch { }
                    }
                }
            }

            appData = new AppData();
            SaveAppData(appData, Constants.DATA_FILE_PATH);
            return appData;
        }

        private readonly static object _MyLock = new object();
        private static readonly object _SaveDebounceLock = new object();
        private static System.Threading.Timer _saveDebounceTimer;

        /// <summary>
        /// 从 JSON 反序列化 AppData (供备份恢复等场景使用)
        /// </summary>
        public static AppData DeserializeAppData(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonConvert.DeserializeObject<AppData>(json, JsonSettings);
        }

        /// <summary>
        /// 异步防抖保存（针对高频属性变更，如动画、窗口坐标拖动、快速点击）
        /// </summary>
        public static void RequestSaveAppData()
        {
            lock (_SaveDebounceLock)
            {
                if (_saveDebounceTimer == null)
                {
                    _saveDebounceTimer = new System.Threading.Timer(state =>
                    {
                        if (MainWindow.appData != null)
                        {
                            SaveAppData(MainWindow.appData, Constants.DATA_FILE_PATH);
                        }
                    }, null, 500, System.Threading.Timeout.Infinite);
                }
                else
                {
                    _saveDebounceTimer.Change(500, System.Threading.Timeout.Infinite);
                }
            }
        }

        private static bool isSaving = false;

        /// <summary>
        /// 保存 app 数据为 JSON 格式
        /// </summary>
        /// <param name="appData"></param>
        /// <param name="filePath"></param>
        public static void SaveAppData(AppData appData, string filePath)
        {
            if (appData == null) return;
            lock (_MyLock)
            {
                if (isSaving) return; // 彻底阻断任何嵌套/递归重入序列化
                try
                {
                    isSaving = true;
                    string dir = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    string json = JsonConvert.SerializeObject(appData, JsonSettings);
                    File.WriteAllText(filePath, json, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    LogUtil.WriteErrorLog(ex, "保存数据文件失败: " + filePath);
                }
                finally
                {
                    isSaving = false;
                }
            }
        }

        public static void SavePassword(string password)
        {
            using (StreamWriter sw = new StreamWriter(Constants.PW_FILE_BAK_PATH))
            {
                sw.Write(password);
            }
        }

        private static string GeneraterUUID()
        {
            try
            {
                if (!File.Exists(Constants.UUID_FILE_BAK_PATH) || string.IsNullOrEmpty(GetUniqueUUID()))
                {
                    using (StreamWriter sw = new StreamWriter(Constants.UUID_FILE_BAK_PATH))
                    {
                        string uuid = Guid.NewGuid().ToString() + "-" + Constants.MY_UUID;
                        sw.Write(uuid);
                        return uuid;
                    }
                }
            }
            catch (Exception) { }
            return "ERROR_UUID_GeneraterUUID_" + Constants.MY_UUID;
        }

        public static string GetUniqueUUID()
        {
            try
            {
                if (File.Exists(Constants.UUID_FILE_BAK_PATH))
                {
                    using (StreamReader reader = new StreamReader(Constants.UUID_FILE_BAK_PATH))
                    {
                        return reader.ReadToEnd().Trim();
                    }
                }
                else
                {
                    return GeneraterUUID();
                }
            }
            catch (Exception) { }
            return "ERROR_UUID_GetUniqueUUID_" + Constants.MY_UUID;
        }

        public static void BakAppData()
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Title = "备份文件",
                Filter = "bak文件(*.bak)|*.bak",
                FileName = "Data-GD-" + DateTime.Now.ToString("yyMMdd") + ".bak",
            };
            if (sfd.ShowDialog() == true)
            {
                SaveAppData(MainWindow.appData, sfd.FileName);
            }
        }

        /// <summary>
        /// 根据路径获取文件图标等信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IconInfo GetIconInfoByPath(string path)
        {
            string tempPath = path;
            string iconPath;
            string targetPath = FileUtil.GetTargetPathByLnk(path);
            iconPath = FileUtil.GetIconPathByLnk(path);
            if (targetPath != null)
            {
                path = targetPath;
            }
            if (StringUtil.IsEmpty(iconPath))
            {
                iconPath = path;
            }

            BitmapImage bi = ImageUtil.GetBitmapIconByPath(iconPath);
            IconInfo iconInfo = new IconInfo
            {
                Path_NoWrite = path,
                LnkPath_NoWrite = tempPath,
                BitmapImage_NoWrite = bi,
                StartArg_NoWrite = FileUtil.GetArgByLnk(tempPath)
            };
            iconInfo.DefaultImage_NoWrite = iconInfo.ImageByteArr;
            string rawName = System.IO.Path.GetFileNameWithoutExtension(tempPath);
            iconInfo.Name_NoWrite = CleanIconName(rawName);
            if (StringUtil.IsEmpty(iconInfo.Name))
            {
                iconInfo.Name_NoWrite = path;
            }
            string relativePath = FileUtil.MakeRelativePath(Constants.APP_DIR + "GeekDesk.exe", iconInfo.Path);
            if (!string.IsNullOrEmpty(relativePath) && !relativePath.Equals(iconInfo.Path))
            {
                iconInfo.RelativePath_NoWrite = relativePath;
            }
            return iconInfo;
        }

        public static IconInfo GetIconInfoByPath_NoWrite(string path)
        {
            string tempPath = path;
            string iconPath = null;
            if (path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                string targetPath = FileUtil.GetTargetPathByLnk(path);
                iconPath = FileUtil.GetIconPathByLnk(path);
                if (targetPath != null)
                {
                    path = targetPath;
                }
            }
            if (StringUtil.IsEmpty(iconPath))
            {
                iconPath = path;
            }

            BitmapImage bi = ImageUtil.GetBitmapIconByPath(iconPath);
            IconInfo iconInfo = new IconInfo
            {
                Path_NoWrite = path,
                LnkPath_NoWrite = tempPath,
                BitmapImage_NoWrite = bi,
                StartArg_NoWrite = FileUtil.GetArgByLnk(tempPath)
            };
            iconInfo.DefaultImage_NoWrite = iconInfo.ImageByteArr;
            string rawName = System.IO.Path.GetFileNameWithoutExtension(tempPath);
            iconInfo.Name = CleanIconName(rawName);
            if (StringUtil.IsEmpty(iconInfo.Name))
            {
                iconInfo.Name_NoWrite = path;
            }
            return iconInfo;
        }

        /// <summary>
        /// 清理快捷方式名称后缀及残留扩展名 (如去除 ' - 快捷方式'、'.exe' 等)
        /// </summary>
        public static string CleanIconName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName)) return rawName;

            string cleanName = rawName.Trim();

            string[] suffixes = new[]
            {
                " - 快捷方式",
                "- 快捷方式",
                " -快捷方式",
                " 快捷方式",
                " - Shortcut",
                "- Shortcut",
                " -Shortcut",
                " Shortcut"
            };

            foreach (var suffix in suffixes)
            {
                if (cleanName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    cleanName = cleanName.Substring(0, cleanName.Length - suffix.Length).Trim();
                    break;
                }
            }

            if (cleanName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                cleanName.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) ||
                cleanName.EndsWith(".url", StringComparison.OrdinalIgnoreCase) ||
                cleanName.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) ||
                cleanName.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase))
            {
                cleanName = System.IO.Path.GetFileNameWithoutExtension(cleanName);
            }

            return string.IsNullOrWhiteSpace(cleanName) ? rawName : cleanName;
        }

        /// <summary>
        /// 排序图标
        /// </summary>
        public static void SortIconList(bool sort = true)
        {
            try
            {
                if (MainWindow.appData.AppConfig.IconSortType != SortType.CUSTOM && sort)
                {
                    ObservableCollection<MenuInfo> menuList = MainWindow.appData.MenuList;
                    List<IconInfo> list;
                    foreach (MenuInfo menuInfo in menuList)
                    {
                        list = new List<IconInfo>(menuInfo.IconList);
                        switch (MainWindow.appData.AppConfig.IconSortType)
                        {
                            case SortType.COUNT_UP:
                                list.Sort((x, y) => x.Count.CompareTo(y.Count));
                                break;
                            case SortType.COUNT_LOW:
                                list.Sort((x, y) => y.Count.CompareTo(x.Count));
                                break;
                            case SortType.NAME_UP:
                                list.Sort((x, y) => x.Name.CompareTo(y.Name));
                                break;
                            case SortType.NAME_LOW:
                                list.Sort((x, y) => y.Name.CompareTo(x.Name));
                                break;
                        }
                        menuInfo.IconList = new ObservableCollection<IconInfo>(list);
                    }
                    MainWindow.appData.AppConfig.SelectedMenuIcons = MainWindow.appData.MenuList[MainWindow.appData.AppConfig.SelectedMenuIndex].IconList;
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 判断鼠标是否在窗口内
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static bool MouseInWindow(Window window)
        {
            double windowHeight = window.Height;
            double windowWidth = window.Width;
            double windowTop = window.Top;
            double windowLeft = window.Left;

            System.Windows.Point p = MouseUtil.GetMousePosition();
            double mouseX = p.X;
            double mouseY = p.Y;

            if (mouseX < windowLeft || mouseX > windowLeft + windowWidth
                || mouseY < windowTop || mouseY > windowTop + windowHeight)
            {
                return false;
            }
            return true;
        }

        public static void FixMenuHierarchy(ObservableCollection<MenuInfo> list)
        {
            if (list == null) return;
            foreach (var menu in list)
            {
                if (menu.Children == null)
                {
                    menu.Children = new ObservableCollection<MenuInfo>();
                }
                if (menu.IconList == null)
                {
                    menu.IconList = new ObservableCollection<IconInfo>();
                }
                else
                {
                    foreach (var icon in menu.IconList)
                    {
                        if (!string.IsNullOrEmpty(icon.Name))
                        {
                            string cleaned = CleanIconName(icon.Name);
                            if (!cleaned.Equals(icon.Name))
                            {
                                icon.Name = cleaned;
                            }
                        }
                    }
                }
                FixMenuHierarchy(menu.Children);
            }
        }
    }
}
