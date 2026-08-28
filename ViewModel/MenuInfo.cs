using GeekDesk.Constant;
using GeekDesk.Util;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace GeekDesk.ViewModel
{

    [Serializable]
    public class MenuInfo : INotifyPropertyChanged
    {


        private string menuName;
        private string menuId;
        private Visibility menuEdit = Visibility.Collapsed;
        private Visibility notMenuEdit = Visibility.Visible;
        private string menuGeometry;  //菜单几何图标
        private string geometryColor; //几何图标颜色
        private ObservableCollection<IconInfo> iconList = new ObservableCollection<IconInfo>();
        private bool isEncrypt;  //是否加密
        private MenuType menuType; //菜单类型 普通,  关联
        private string linkPath; //关联路径
        private string parentId = null; //父分类ID（支持多级分类）
        private ObservableCollection<MenuInfo> children = new ObservableCollection<MenuInfo>();
        private bool isExpanded = true;
        private bool isSelected = false;

        public string ParentId
        {
            get => parentId;
            set
            {
                parentId = value;
                OnPropertyChanged("ParentId");
            }
        }

        public ObservableCollection<MenuInfo> Children
        {
            get
            {
                if (children == null) children = new ObservableCollection<MenuInfo>();
                return children;
            }
            set
            {
                children = value;
                OnPropertyChanged("Children");
            }
        }

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                    if (isExpanded && MainWindow.appData != null)
                    {
                        foreach (var m in MainWindow.appData.MenuList)
                        {
                            if (m != this && m.IsExpanded)
                            {
                                m.IsExpanded = false;
                            }
                        }
                    }
                }
            }
        }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }


        public string LinkPath
        {
            get
            {
                return linkPath;
            }
            set
            {
                linkPath = value;
                OnPropertyChanged("LinkPath");
            }
        }

        public MenuType MenuType
        {
            get
            {
                return menuType;
            }
            set
            {
                menuType = value;
                OnPropertyChanged("MenuType");
            }
        }

        public bool IsEncrypt
        {
            get
            {
                return isEncrypt;
            }
            set
            {
                isEncrypt = value;
                OnPropertyChanged("IsEncrypt");
            }
        }

        public string MenuGeometry
        {
            get
            {
                if (menuGeometry == null)
                {
                    return Constants.DEFAULT_MENU_GEOMETRY;
                }
                return menuGeometry;
            }
            set
            {
                menuGeometry = value;
                OnPropertyChanged("MenuGeometry");
            }
        }

        public string GeometryColor
        {
            get
            {
                if (geometryColor == null)
                {
                    return Constants.DEFAULT_MENU_GEOMETRY_COLOR;
                }
                return geometryColor;
            }
            set
            {
                geometryColor = value;
                OnPropertyChanged("GeometryColor");
            }
        }

        public string MenuName
        {
            get
            {
                return menuName;
            }
            set
            {
                menuName = value;
                OnPropertyChanged("MenuName");
            }
        }

        public string MenuId
        {
            get
            {
                return menuId;
            }
            set
            {
                menuId = value;
                OnPropertyChanged("MenuId");
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public Visibility MenuEdit
        {
            get
            {
                return menuEdit;
            }
            set
            {
                menuEdit = value;
                if (menuEdit == Visibility.Visible)
                {
                    NotMenuEdit = Visibility.Collapsed;
                }
                else
                {
                    NotMenuEdit = Visibility.Visible;
                }
                OnPropertyChanged("MenuEdit");
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public Visibility NotMenuEdit
        {
            get
            {
                return notMenuEdit;
            }
            set
            {
                notMenuEdit = value;
                OnPropertyChanged("NotMenuEdit");
            }
        }

        public ObservableCollection<IconInfo> IconList
        {
            get
            {
                return iconList;
            }
            set
            {
                iconList = value;
                OnPropertyChanged("IconList");
            }
        }

        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            CommonCode.SaveAppData(MainWindow.appData, Constants.DATA_FILE_PATH);
        }
    }
}
