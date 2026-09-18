using GeekDesk.Constant;
using GeekDesk.Util;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

/// <summary>
/// 图标信息
/// </summary>
namespace GeekDesk.ViewModel
{
    [Serializable]
    public class IconInfo : INotifyPropertyChanged
    {
        private string path; //路径
        private string name; //文件名
        private int count = 0; //打开次数
        [field: NonSerialized]
        [JsonIgnore]
        private BitmapImage bitmapImage; //位图
        private byte[] imageByteArr; //图片 byte数组
        private string content; //显示信息
        private bool adminStartUp = false; //始终管理员方式启动 默认否
        private byte[] defaultImage; //默认图标
        private string startArg; //启动参数
        private string lnkPath;
        private string relativePath; //相对路径
        private IconType iconType = IconType.OTHER;
        private bool isChecked = false; //是否选中

        [JsonIgnore]
        public bool IsChecked_NoWrite
        {
            get => isChecked;
            set
            {
                isChecked = value;
                OnPropertyChanged("IsChecked_NoWrite");
            }
        }

        [JsonIgnore]
        public string RelativePath_NoWrite
        {
            get => relativePath;
            set => relativePath = value;
        }

        public string RelativePath
        {
            get => relativePath;
            set
            {
                relativePath = value;
                OnPropertyChanged("RelativePath");
            }
        }

        [JsonIgnore]
        public string LnkPath_NoWrite
        {
            get => lnkPath;
            set => lnkPath = value;
        }

        public string LnkPath
        {
            get => lnkPath;
            set
            {
                lnkPath = value;
                OnPropertyChanged("LnkPath");
            }
        }

        public string StartArg
        {
            get => startArg;
            set
            {
                startArg = value;
                OnPropertyChanged("StartArg");
            }
        }

        [JsonIgnore]
        public string StartArg_NoWrite
        {
            get => startArg;
            set => startArg = value;
        }

        public IconType IconType
        {
            get
            {
                if (iconType == 0) return IconType.OTHER;
                return iconType;
            }
            set
            {
                iconType = value;
                OnPropertyChanged("IconType");
            }
        }

        public byte[] DefaultImage
        {
            get => defaultImage;
            set
            {
                defaultImage = value;
                OnPropertyChanged("DefaultImage");
            }
        }

        [JsonIgnore]
        public byte[] DefaultImage_NoWrite
        {
            get => defaultImage;
            set => defaultImage = value;
        }

        public bool AdminStartUp
        {
            get => adminStartUp;
            set
            {
                adminStartUp = value;
                OnPropertyChanged("AdminStartUp");
            }
        }

        public int Count
        {
            get => count;
            set
            {
                count = value;
                if (StringUtil.IsEmpty(Path))
                {
                    Content = Name + "\n使用次数: " + Count;
                }
                else
                {
                    Content = Path + "\n" + Name + "\n使用次数: " + Count;
                }
                OnPropertyChanged("Count");
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                if (StringUtil.IsEmpty(Path))
                {
                    Content = Name + "\n使用次数: " + Count;
                }
                else
                {
                    Content = Path + "\n" + Name + "\n使用次数: " + Count;
                }
                OnPropertyChanged("Name");
            }
        }

        [JsonIgnore]
        public string Name_NoWrite
        {
            get => name;
            set
            {
                name = value;
                if (StringUtil.IsEmpty(Path))
                {
                    content = Name + "\n使用次数: " + Count;
                }
                else
                {
                    content = Path + "\n" + Name + "\n使用次数: " + Count;
                }
            }
        }

        public string Path
        {
            get => path;
            set
            {
                path = value;
                if (StringUtil.IsEmpty(Path))
                {
                    Content = Name + "\n使用次数: " + Count;
                }
                else
                {
                    Content = Path + "\n" + Name + "\n使用次数: " + Count;
                }
                OnPropertyChanged("Path");
            }
        }

        [JsonIgnore]
        public string Path_NoWrite
        {
            get => path;
            set
            {
                path = value;
                if (StringUtil.IsEmpty(Path))
                {
                    content = Name + "\n使用次数: " + Count;
                }
                else
                {
                    content = Path + "\n" + Name + "\n使用次数: " + Count;
                }
            }
        }

        [JsonIgnore]
        public BitmapImage BitmapImage
        {
            get
            {
                //缓存解码结果 避免每次绑定求值都重新解码 byte[]
                if (bitmapImage == null && imageByteArr != null && imageByteArr.Length > 0)
                {
                    bitmapImage = ImageUtil.ByteArrToImage(imageByteArr);
                }
                return bitmapImage;
            }
            set
            {
                if (value == null)
                {
                    bitmapImage = null;
                    imageByteArr = null;
                }
                else
                {
                    imageByteArr = ImageUtil.BitmapImageToByte(value);
                    bitmapImage = value;
                }
                OnPropertyChanged("ImageByteArr");
                OnPropertyChanged("BitmapImage");
            }
        }

        [JsonIgnore]
        public BitmapImage BitmapImage_NoWrite
        {
            get
            {
                if (bitmapImage == null && imageByteArr != null && imageByteArr.Length > 0)
                {
                    bitmapImage = ImageUtil.ByteArrToImage(imageByteArr);
                }
                return bitmapImage;
            }
            set
            {
                if (value == null)
                {
                    bitmapImage = null;
                    imageByteArr = null;
                }
                else
                {
                    imageByteArr = ImageUtil.BitmapImageToByte(value);
                    bitmapImage = value;
                }
                OnPropertyChanged("BitmapImage_NoWrite");
            }
        }

        public byte[] ImageByteArr
        {
            get => imageByteArr;
            set
            {
                imageByteArr = value;
                bitmapImage = null; //图像字节变更时清除解码缓存
                OnPropertyChanged("ImageByteArr");
            }
        }

        [JsonIgnore]
        public byte[] ImageByteArr_NoWrite
        {
            get => imageByteArr;
            set
            {
                imageByteArr = value;
                bitmapImage = null; //图像字节变更时清除解码缓存
            }
        }

        public string Content
        {
            get => content;
            set
            {
                content = value;
                OnPropertyChanged("Content");
            }
        }

        [JsonIgnore]
        public string Content_NoWrite
        {
            get => content;
            set => content = value;
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
    }
}
