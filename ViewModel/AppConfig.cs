
using GeekDesk.Constant;
using GeekDesk.Util;
using GeekDesk.ViewModel.Temp;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static GeekDesk.Util.GlobalHotKey;

/// <summary>
/// 程序设置
/// </summary>
namespace GeekDesk.ViewModel
{

    [Serializable]
    public class AppConfig : INotifyPropertyChanged
    {
        private SortType menuSortType = SortType.CUSTOM; //菜单排序类型
        private SortType iconSortType = SortType.CUSTOM; //图表排序类型
        private double windowWidth = (double)CommonEnum.WINDOW_WIDTH;  //窗口宽度
        private double windowHeight = (double)CommonEnum.WINDOW_HEIGHT; //窗口高度
        private double menuCardWidth = (double)CommonEnum.MENU_CARD_WIDHT;//菜单栏宽度
        private int selectedMenuIndex = 0;  //上次选中菜单索引
        private bool followMouse = true;  //面板跟随鼠标 默认是
        private Visibility configIconVisible = Visibility.Visible; // 设置按钮是否显示
        private Visibility titleLogoVisible = Visibility.Visible; // 标题logo是否显示
        private AppHideType appHideType = AppHideType.START_EXE;  //面板关闭方式 (默认启动程序后)
        private bool startedShowPanel = true;  //启动时是否显示主面板  默认显示
        [field: NonSerialized]
        private BitmapImage bitmapImage; //位图
        private byte[] imageByteArr; //背景图片 byte数组
        private string bacImgName = "系统默认";
        private int cardOpacity = 85;  //默认0.85的不透明度
        private int bgOpacity = 100; // 背景图片不透明度 此处显示数值 * 100
        private int pannelOpacity = 100; //主面板不透明度 此处显示数值 * 100
        private int pannelCornerRadius = 12;  //面板圆角 默认12 (苹果风格)
        [field: NonSerialized]
        private ObservableCollection<IconInfo> selectedMenuIcons;

        private string hotkeyStr = "Ctrl + Q";  //默认启动面板快捷键
        private HotkeyModifiers hotkeyModifiers = HotkeyModifiers.MOD_CONTROL; //默认启动面板快捷键
        private Key hotkey = Key.Q; //默认启动面板快捷键

        private string toDoHotkeyStr = "Ctrl + Shift + Q";  //待办任务快捷键
        private HotkeyModifiers toDoHotkeyModifiers; //待办任务快捷键
        private Key toDoHotkey = Key.Q; //待办任务快捷键


        private string colorPickerHotkeyStr = ""; //拾色器快捷键
        private HotkeyModifiers colorPickerHotkeyModifiers; //拾色器快捷键
        private Key colorPickerHotkey; //拾色器快捷键

        private string customIconUrl; //自定义图标url
        private string customIconJsonUrl;  //自定义图标json信息url

        private bool blurEffect = true; //毛玻璃效果 默认是
        private double blurValue;

        private bool selfStartUp = true; //开机自启动设置
        private bool selfStartUped = false;  //是否已设置
        private bool pmModel = false; //性能模式
        private string textColor = "#000000"; //字体颜色
        private double iconFontSize = 12; // 图标文字大小 (默认12)
        private double imgPanelWidth = 72;
        private double imgPanelHeight = 72;
        private bool marginHide = true; //贴边隐藏 (默认启用音速启动贴边)
        private bool appAnimation = false; //主窗口动画效果
        private int imageWidth = (int)CommonEnum.IMAGE_WIDTH; //图片宽度
        private int imageHeight = (int)CommonEnum.IMAGE_HEIGHT; //图片高度

        private bool mouseMiddleShow = false;  //鼠标中键呼出 (已废弃)

        private bool showBarIcon = true; //显示托盘图标  默认显示

        private bool doubleOpen = false; //双击打开项目  默认关闭

        private bool hoverMenu = true; //悬停切换菜单

        private BGStyle bgStyle = BGStyle.ImgBac; //背景风格

        private GradientBGParam gradientBGParam = null; //渐变背景参数

        private bool? enableAppHotKey = false;  //弱化热键依赖，贴边为主
        private bool? enableTodoHotKey = true; //可能为null 开启待办热键

        private bool enableColorPickerHotKey;  //新增 默认为false 不需要考虑null值

        private SearchType searchType;

        private string sysBakTime;  //系统自动备份时间

        private string menuPassword; //锁菜单密码

        private string passwordHint; //密码提示

        private bool? isShow;

        private bool itemSpradeAnimation; //列表展开动画

        private bool? secondsWindow; //秒数窗口 默认打开

        private bool? alwaysTopmost = true; //默认置顶
        private LauncherStyle launcherStyle = LauncherStyle.XiaoQDesk; //启动器风格 (默认支持小Q书桌模式)
        private VisualTheme visualTheme = VisualTheme.MacAcrylic; //视觉主题 (默认苹果磨砂灰)

        //窗口尺寸按启动器风格分别记忆
        private double xiaoQWindowWidth = 620;
        private double xiaoQWindowHeight = 440;
        private double sonicWindowWidth = 320;
        private double sonicWindowHeight = 560;

        /// <summary>
        /// 小Q书桌模式窗口宽度
        /// </summary>
        public double XiaoQWindowWidth
        {
            get => xiaoQWindowWidth;
            set
            {
                xiaoQWindowWidth = value;
                OnPropertyChanged("XiaoQWindowWidth");
            }
        }

        /// <summary>
        /// 小Q书桌模式窗口高度
        /// </summary>
        public double XiaoQWindowHeight
        {
            get => xiaoQWindowHeight;
            set
            {
                xiaoQWindowHeight = value;
                OnPropertyChanged("XiaoQWindowHeight");
            }
        }

        /// <summary>
        /// 音速抽屉模式窗口宽度
        /// </summary>
        public double SonicWindowWidth
        {
            get => sonicWindowWidth;
            set
            {
                sonicWindowWidth = value;
                OnPropertyChanged("SonicWindowWidth");
            }
        }

        /// <summary>
        /// 音速抽屉模式窗口高度
        /// </summary>
        public double SonicWindowHeight
        {
            get => sonicWindowHeight;
            set
            {
                sonicWindowHeight = value;
                OnPropertyChanged("SonicWindowHeight");
            }
        }

        public VisualTheme VisualTheme
        {
            get => visualTheme;
            set
            {
                visualTheme = value;
                OnPropertyChanged("VisualTheme");
            }
        }

        public LauncherStyle LauncherStyle
        {
            get => launcherStyle;
            set
            {
                launcherStyle = value;
                OnPropertyChanged("LauncherStyle");
            }
        }

        private bool? showIconTitle = true; //是否显示iconTitle

        private bool iconBatch = false; //批量操作图标状态

        private ObservableCollection<GradientBGParam> customBGParams; //自定义纯色背景


        public ObservableCollection<GradientBGParam> CustomBGParams
        {
            get
            {
                if (customBGParams == null)
                {
                    customBGParams = new ObservableCollection<GradientBGParam>();
                }
                return customBGParams;
            }
            set
            {
                customBGParams = value;
                OnPropertyChanged("CustomBGParams");
            }
        }

        [JsonIgnore]
        public bool IconBatch_NoWrite
        {
            get
            {
                return iconBatch;
            }
            set
            {
                iconBatch = value;
                OnPropertyChanged("IconBatch_NoWrite");
            }
        }

        public bool? ShowIconTitle
        {
            get
            {
                if (showIconTitle == null) showIconTitle = true;
                return showIconTitle;
            }
            set
            {
                showIconTitle = value;
                OnPropertyChanged("ShowIconTitle");
            }
        }

        public bool? AlwaysTopmost
        {
            get
            {
                if (alwaysTopmost == null) alwaysTopmost = true;
                return alwaysTopmost;
            }
            set
            {
                alwaysTopmost = value;
                OnPropertyChanged("AlwaysTopmost");
            }
        }

        #region GetSet

        public bool? SecondsWindow
        {
            get
            {
                if (secondsWindow == null) secondsWindow = true;
                return secondsWindow;
            }
            set
            {
                secondsWindow = value;
                OnPropertyChanged("SecondsWindow");
            }
        }

        public bool ItemSpradeAnimation
        {
            get
            {
                return itemSpradeAnimation;
            }
            set
            {
                itemSpradeAnimation = value;
                OnPropertyChanged("ItemSpradeAnimation");
            }
        }

        public bool? IsShow
        {
            get
            {
                return isShow;
            }
            set
            {
                isShow = value;
                OnPropertyChanged("IsShow");
            }
        }

        public string PasswordHint
        {
            get
            {
                return passwordHint;
            }
            set
            {
                passwordHint = value;
                OnPropertyChanged("PasswordHint");
            }
        }
        public string MenuPassword
        {
            get
            {
                return menuPassword;
            }
            set
            {
                menuPassword = value;
                OnPropertyChanged("MenuPassword");
            }
        }

        public string SysBakTime
        {
            get
            {
                return sysBakTime;
            }
            set
            {
                sysBakTime = value;
            }
        }

        public SearchType SearchType
        {
            get
            {
                return searchType;
            }
            set
            {
                searchType = value;
                OnPropertyChanged("SearchType");
            }
        }

        public bool EnableColorPickerHotKey
        {
            get
            {
                return enableColorPickerHotKey;
            }
            set
            {
                enableColorPickerHotKey = value;
                OnPropertyChanged("EnableColorPickerHotKey");
            }
        }

        public bool? EnableAppHotKey
        {
            get
            {
                if (enableAppHotKey == null) enableAppHotKey = true;
                return enableAppHotKey;
            }
            set
            {
                enableAppHotKey = value;
                OnPropertyChanged("EnableAppHotKey");
            }
        }

        public bool? EnableTodoHotKey
        {
            get
            {
                if (enableTodoHotKey == null) enableTodoHotKey = true;
                return enableTodoHotKey;
            }
            set
            {
                enableTodoHotKey = value;
                OnPropertyChanged("EnableTodoHotKey");
            }
        }

        public Visibility TitleLogoVisible
        {
            get
            {
                return titleLogoVisible;
            }
            set
            {
                titleLogoVisible = value;
                OnPropertyChanged("TitleLogoVisible");
            }
        }

        public GradientBGParam GradientBGParam
        {
            get
            {
                if (gradientBGParam == null)
                {
                    gradientBGParam = DeepCopyUtil.DeepCopy(GradientBGParamList.GradientBGParams[0]);
                }
                return gradientBGParam;
            }
            set
            {
                gradientBGParam = value;
                OnPropertyChanged("GradientBGParam");
            }
        }

        public BGStyle BGStyle
        {
            get
            {
                if (bgStyle == 0)
                {
                    bgStyle = (BGStyle)1;
                }
                return bgStyle;
            }
            set
            {
                bgStyle = value;
                OnPropertyChanged("BGStyle");
            }
        }

        public bool HoverMenu
        {
            get
            {
                return hoverMenu;
            }
            set
            {
                hoverMenu = value;
                OnPropertyChanged("HoverMenu");
            }
        }

        public bool DoubleOpen
        {
            get
            {
                return doubleOpen;
            }
            set
            {
                doubleOpen = value;
                OnPropertyChanged("DoubleOpen");
            }
        }

        public bool ShowBarIcon
        {
            get
            {
                return showBarIcon;
            }
            set
            {
                showBarIcon = value;
                OnPropertyChanged("ShowBarIcon");
            }
        }

        public bool MouseMiddleShow
        {
            get
            {
                return mouseMiddleShow;
            }
            set
            {
                mouseMiddleShow = value;
                OnPropertyChanged("MouseMiddleShow");
            }
        }

        public int ImageWidth
        {
            get
            {
                // 为了兼容旧版 暂时使用默认
                if (imageWidth == 0)
                {
                    return (int)CommonEnum.IMAGE_WIDTH;
                }
                else
                {
                    return imageWidth;
                }

            }
            set
            {
                imageWidth = value;
                //同时设置高度
                ImageHeight = value;


                //计算 容器宽度因子
                double i = ((double)imageWidth - (double)CommonEnum.IMAGE_WIDTH) / 5d;
                double s = 2.44;
                i *= 2d;
                while (i > 1)
                {
                    i /= 10d;
                }

                if (i > 0d)
                {
                    s -= i;
                }
                if (s < 2.2)
                {
                    s = 2.2;
                }
                //设置容器宽度
                ImgPanelWidth = (int)(ImageWidth * s);

                OnPropertyChanged("ImageWidth");
            }
        }

        public int ImageHeight
        {
            get
            {
                //都使用宽度来确定大小
                // 为了兼容旧版 暂时使用默认
                if (imageHeight == 0)
                {
                    return (int)CommonEnum.IMAGE_HEIGHT;
                }
                else
                {
                    return imageHeight;
                }
            }
            set
            {
                imageHeight = value;

                //计算容器高度因子
                double i = ((double)imageHeight - (double)CommonEnum.IMAGE_HEIGHT) / 5d;
                while (i > 1)
                {
                    i /= 10d;
                }
                double s = 2.00;
                if (i > 0d)
                {
                    s -= i;
                }
                if (s < 1.5) s = 1.5;

                //设置容器高度
                ImgPanelHeight = ImageHeight * s;
                OnPropertyChanged("ImageHeight");
            }
        }

        public double IconFontSize
        {
            get
            {
                if (iconFontSize < 10) iconFontSize = 12;
                return iconFontSize;
            }
            set
            {
                iconFontSize = value;
                OnPropertyChanged("IconFontSize");
                CommonCode.RequestSaveAppData();
            }
        }

        public bool AppAnimation
        {
            get
            {
                return appAnimation;
            }
            set
            {
                appAnimation = value;
                OnPropertyChanged("AppAnimation");
            }
        }

        public bool MarginHide
        {
            get
            {
                return marginHide;
            }
            set
            {
                marginHide = value;
                OnPropertyChanged("MarginHide");
            }
        }

        public double ImgPanelWidth
        {
            get
            {
                if (imgPanelWidth == 0d || imgPanelWidth > 200) return 72d;
                return imgPanelWidth;
            }
            set
            {
                imgPanelWidth = value;
                OnPropertyChanged("ImgPanelWidth");
            }
        }

        public double ImgPanelHeight
        {
            get
            {
                if (imgPanelHeight == 0d || imgPanelHeight > 200) return 72d;
                return imgPanelHeight;
            }
            set
            {
                imgPanelHeight = value;
                OnPropertyChanged("ImgPanelHeight");
            }
        }

        public string TextColor
        {
            get
            {
                if (textColor == null) return "#000000";
                return textColor;
            }
            set
            {
                textColor = value;
                OnPropertyChanged("TextColor");
            }
        }

        public bool PMModel
        {
            get
            {
                return pmModel;
            }
            set
            {
                pmModel = value;
                OnPropertyChanged("PMModel");
            }
        }

        public bool SelfStartUped
        {
            get
            {
                return selfStartUped;
            }
            set
            {
                selfStartUped = value;
                OnPropertyChanged("SelfStartUped");
            }
        }

        public bool SelfStartUp
        {
            get
            {
                return selfStartUp;
            }
            set
            {
                selfStartUp = value;
                selfStartUped = true;
                OnPropertyChanged("SelfStartUp");
            }
        }

        public Key ColorPickerHotkey
        {
            get
            {
                return colorPickerHotkey;
            }
            set
            {
                colorPickerHotkey = value;
                OnPropertyChanged("ColorPickerHotkey");
            }
        }


        public HotkeyModifiers ColorPickerHotkeyModifiers
        {
            get
            {
                return colorPickerHotkeyModifiers;
            }
            set
            {
                colorPickerHotkeyModifiers = value;
                OnPropertyChanged("ColorPickerHotkeyModifiers");
            }
        }

        public string ColorPickerHotkeyStr
        {
            get
            {
                return colorPickerHotkeyStr;
            }
            set
            {
                colorPickerHotkeyStr = value;
                OnPropertyChanged("ColorPickerHotkeyStr");
            }
        }


        public Key ToDoHotkey
        {
            get
            {
                //兼容老版本
                if (toDoHotkey == Key.None)
                {
                    toDoHotkey = Key.E;
                }
                return toDoHotkey;
            }
            set
            {
                toDoHotkey = value;
                OnPropertyChanged("ToDoHotkey");
            }
        }


        public HotkeyModifiers ToDoHotkeyModifiers
        {
            get
            {
                if (toDoHotkeyModifiers == 0)
                {
                    toDoHotkeyModifiers = HotkeyModifiers.MOD_CONTROL | HotkeyModifiers.MOD_SHIFT;
                }
                return toDoHotkeyModifiers;
            }
            set
            {
                toDoHotkeyModifiers = value;
                OnPropertyChanged("ToDoHotkeyModifiers");
            }
        }

        public string ToDoHotkeyStr
        {
            get
            {
                //兼容老版本
                if (toDoHotkeyStr == null)
                {
                    toDoHotkeyStr = "Ctrl + Shift + Q";
                }
                return toDoHotkeyStr;
            }
            set
            {
                toDoHotkeyStr = value;
                OnPropertyChanged("ToDoHotkeyStr");
            }
        }

        public double BlurValue
        {
            get
            {
                return blurEffect ? 100 : 0;
            }
            set
            {
                blurValue = value;
                OnPropertyChanged("BlurValue");
            }
        }

        public bool BlurEffect
        {
            get
            {
                return blurEffect;
            }
            set
            {
                blurEffect = value;
                blurValue = blurEffect ? 100 : 0;
                OnPropertyChanged("BlurEffect");
                OnPropertyChanged("BlurValue");
            }
        }
        public string CustomIconUrl
        {
            get
            {
                return customIconUrl;
            }
            set
            {
                customIconUrl = value;
                OnPropertyChanged("CustomIconUrl");
            }
        }
        public string CustomIconJsonUrl
        {
            get
            {
                return customIconJsonUrl;
            }
            set
            {
                customIconJsonUrl = value;
                OnPropertyChanged("CustomIconJsonUrl");
            }
        }
        public Key Hotkey
        {
            get
            {
                return hotkey;
            }
            set
            {
                hotkey = value;
                OnPropertyChanged("Hotkey");
            }
        }
        public string HotkeyStr
        {
            get
            {
                return hotkeyStr;
            }
            set
            {
                hotkeyStr = value;
                OnPropertyChanged("HotkeyStr");
            }
        }
        public HotkeyModifiers HotkeyModifiers
        {
            get
            {
                if (hotkeyModifiers == 0)
                {
                    hotkeyModifiers = HotkeyModifiers.MOD_CONTROL;
                }
                return hotkeyModifiers;
            }
            set
            {
                hotkeyModifiers = value;
                OnPropertyChanged("HotkeyModifiers");
            }
        }

        [JsonIgnore]
        public ObservableCollection<IconInfo> SelectedMenuIcons
        {
            get
            {
                return selectedMenuIcons;
            }
            set
            {
                selectedMenuIcons = value;
                OnPropertyChanged("SelectedMenuIcons");
            }
        }

        public int PannelCornerRadius
        {
            get
            {
                return pannelCornerRadius;
            }
            set
            {
                pannelCornerRadius = value;
                OnPropertyChanged("pannelCornerRadius");
            }
        }

        public int PannelOpacity
        {
            get
            {
                return pannelOpacity;
            }
            set
            {
                pannelOpacity = value;
                OnPropertyChanged("PannelOpacity");
            }
        }
        public int BgOpacity
        {
            get
            {
                return bgOpacity;
            }
            set
            {
                bgOpacity = value;
                OnPropertyChanged("BgOpacity");
            }
        }

        public int CardOpacity
        {
            get
            {
                return cardOpacity;
            }
            set
            {
                cardOpacity = value;
                OnPropertyChanged("CardOpacity");
            }
        }

        public string BacImgName
        {
            get
            {
                return bacImgName;
            }
            set
            {
                bacImgName = value;
                OnPropertyChanged("BacImgName");
            }
        }

        public byte[] ImageByteArr
        {
            get
            {
                return imageByteArr;
            }
            set
            {
                imageByteArr = value;
                OnPropertyChanged("ImageByteArr");
            }
        }


        [JsonIgnore]
        public BitmapImage BitmapImage
        {
            get
            {
                if (imageByteArr == null || imageByteArr.Length == 0)
                {
                    bacImgName = "系统默认";
                    //Image image = ImageUtil.ByteArrayToImage(Convert.FromBase64String(Constants.DEFAULT_BAC_IMAGE_BASE64));
                    return ImageUtil.ByteArrToImage(Convert.FromBase64String(Constants.DEFAULT_BAC_IMAGE_BASE64));
                }
                else
                {
                    return ImageUtil.ByteArrToImage(ImageByteArr);
                }
            }
            set
            {
                bitmapImage = value;
                imageByteArr = ImageUtil.BitmapImageToByte(bitmapImage);
                OnPropertyChanged("BitmapImage");
            }
        }


        public bool StartedShowPanel
        {
            get
            {
                return startedShowPanel;
            }
            set
            {
                startedShowPanel = value;
                OnPropertyChanged("StartedShowPanel");
            }
        }

        public AppHideType AppHideType
        {
            get
            {
                return appHideType;
            }
            set
            {
                appHideType = value;
                OnPropertyChanged("AppHideType");
            }
        }

        public Visibility ConfigIconVisible
        {
            get
            {
                return configIconVisible;
            }
            set
            {
                configIconVisible = value;
                OnPropertyChanged("ConfigIconVisible");
            }
        }


        public bool FollowMouse
        {
            get
            {
                return followMouse;
            }
            set
            {
                followMouse = value;
                OnPropertyChanged("FollowMouse");
            }
        }

        public int SelectedMenuIndex
        {
            get
            {
                return selectedMenuIndex;
            }
            set
            {
                selectedMenuIndex = value;
                OnPropertyChanged("SelectedMenuIndex");
            }
        }

        public SortType MenuSortType
        {
            get
            {
                return menuSortType;
            }
            set
            {
                menuSortType = value;
                OnPropertyChanged("MenuSortType");
            }
        }

        public SortType IconSortType
        {
            get
            {
                return iconSortType;
            }
            set
            {
                iconSortType = value;
                OnPropertyChanged("IconSortType");
            }
        }

        public double WindowWidth
        {
            get
            {
                return windowWidth;
            }
            set
            {
                windowWidth = value;
                OnPropertyChanged("WindowWidth");
            }
        }

        public double WindowHeight
        {
            get
            {
                return windowHeight;
            }
            set
            {
                windowHeight = value;
                OnPropertyChanged("WindowHeight");
            }
        }

        public double MenuCardWidth
        {
            get
            {
                return menuCardWidth;
            }
            set
            {
                menuCardWidth = value;
                OnPropertyChanged("MenuCardWidth");
            }
        }

        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName != null && !propertyName.Contains("NoWrite"))
            {
                CommonCode.RequestSaveAppData();
            }
        }

        #endregion

        public override String ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
