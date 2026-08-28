using GeekDesk.Constant;
using GeekDesk.Control.Other;
using GeekDesk.Control.Windows;
using GeekDesk.Util;
using GeekDesk.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace GeekDesk.Control.UserControls.PannelCard
{
    public partial class AccordionCardControl : UserControl
    {
        private AppData appData => MainWindow.appData;

        public AccordionCardControl()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                InitDrawerLayout();
                if (appData?.MenuList != null)
                {
                    appData.MenuList.CollectionChanged -= MenuList_CollectionChanged;
                    appData.MenuList.CollectionChanged += MenuList_CollectionChanged;
                }
            };
        }

        private void MenuList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InitDrawerLayout();
        }

        public void InitDrawerLayout()
        {
            RootGrid.Children.Clear();
            RootGrid.RowDefinitions.Clear();

            if (appData == null || appData.MenuList == null || appData.MenuList.Count == 0)
            {
                return;
            }

            // 确保有且仅有一个抽屉展开
            bool hasExpanded = false;
            foreach (var menu in appData.MenuList)
            {
                if (menu.IsExpanded)
                {
                    if (!hasExpanded)
                    {
                        hasExpanded = true;
                    }
                    else
                    {
                        menu.IsExpanded = false;
                    }
                }
            }
            if (!hasExpanded)
            {
                appData.MenuList[0].IsExpanded = true;
            }

            int rowIndex = 0;
            for (int i = 0; i < appData.MenuList.Count; i++)
            {
                var menu = appData.MenuList[i];

                // 1. 抽屉标题栏行 (Auto)
                RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var header = CreateDrawerHeader(menu);
                Grid.SetRow(header, rowIndex++);
                RootGrid.Children.Add(header);

                // 2. 抽屉内容行 (如果是展开项则为 * 占满剩余所有空间，折叠项为 0)
                var contentRowDef = new RowDefinition();
                if (menu.IsExpanded)
                {
                    contentRowDef.Height = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    contentRowDef.Height = new GridLength(0);
                }
                RootGrid.RowDefinitions.Add(contentRowDef);

                var contentArea = CreateDrawerContent(menu, contentRowDef);
                Grid.SetRow(contentArea, rowIndex++);
                RootGrid.Children.Add(contentArea);
            }
        }

        private FrameworkElement CreateDrawerHeader(MenuInfo menu)
        {
            // 音速启动 (Sonic Launcher) 经典风格抽屉栏：浅灰/纯白水晶高光按钮，上下细边框，黑色清晰居中文字
            Border headerBorder = new Border
            {
                Height = 30,
                Margin = new Thickness(0, 1, 0, 1),
                CornerRadius = new CornerRadius(0),
                Cursor = Cursors.Hand,
                Tag = menu,
                ContextMenu = (ContextMenu)this.Resources["CategoryContextMenu"],
                AllowDrop = true
            };

            LinearGradientBrush normalBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            normalBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFFF"), 0.0));
            normalBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F1F3F5"), 0.5));
            normalBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#E4E7EB"), 1.0));

            LinearGradientBrush expandedBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            expandedBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#EBF3FC"), 0.0));
            expandedBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#D0E4F9"), 0.5));
            expandedBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#B9D7F6"), 1.0));

            LinearGradientBrush hoverBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            hoverBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F7FAFE"), 0.0));
            hoverBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#E6F0FC"), 0.5));
            hoverBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#D5E5F9"), 1.0));

            Action updateHeaderStyle = () =>
            {
                if (menu.IsExpanded)
                {
                    headerBorder.Background = expandedBrush;
                    headerBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7DA2CE"));
                    headerBorder.BorderThickness = new Thickness(0, 1, 0, 1);
                }
                else
                {
                    headerBorder.Background = normalBrush;
                    headerBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCD2D9"));
                    headerBorder.BorderThickness = new Thickness(0, 1, 0, 1);
                }
            };
            updateHeaderStyle();

            headerBorder.MouseEnter += (s, e) =>
            {
                if (!menu.IsExpanded)
                {
                    headerBorder.Background = hoverBrush;
                    headerBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#99BBE8"));
                }
            };
            headerBorder.MouseLeave += (s, e) =>
            {
                updateHeaderStyle();
            };

            headerBorder.MouseLeftButtonDown += DrawerHeader_Click;
            headerBorder.DragEnter += DrawerHeader_DragEnter;
            headerBorder.Drop += DrawerHeader_Drop;

            Grid centerGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 重命名输入框
            TextBox editBox = new TextBox
            {
                FontSize = 13,
                Height = 24,
                MinWidth = 100,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = menu,
                Padding = new Thickness(2),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7DA2CE")),
                Background = Brushes.White,
                Foreground = Brushes.Black
            };
            editBox.SetBinding(TextBox.TextProperty, new Binding("MenuName") { Source = menu, Mode = BindingMode.TwoWay });
            editBox.SetBinding(UIElement.VisibilityProperty, new Binding("MenuEdit") { Source = menu });
            editBox.LostFocus += MenuRename_LostFocus;
            editBox.KeyDown += MenuRename_KeyDown;

            // 居中分类文字 (经典黑字清晰显示)
            TextBlock nameBlock = new TextBlock
            {
                FontSize = 13,
                FontWeight = FontWeights.Normal,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222222"))
            };
            nameBlock.SetBinding(TextBlock.TextProperty, new Binding("MenuName") { Source = menu });
            nameBlock.SetBinding(UIElement.VisibilityProperty, new Binding("NotMenuEdit") { Source = menu });

            menu.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "IsExpanded")
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        updateHeaderStyle();
                    });
                }
            };

            centerGrid.Children.Add(editBox);
            centerGrid.Children.Add(nameBlock);
            headerBorder.Child = centerGrid;

            return headerBorder;
        }

        private FrameworkElement CreateDrawerContent(MenuInfo menu, RowDefinition rowDef)
        {
            // 音速启动图标容器面板：纯白背景
            Border contentBorder = new Border
            {
                Padding = new Thickness(2, 4, 2, 4),
                Margin = new Thickness(0),
                Background = Brushes.White,
                BorderThickness = new Thickness(0),
                AllowDrop = true,
                Tag = menu
            };

            // 根据 IsExpanded 动态设置内容显隐和高度
            Action updateVisibility = () =>
            {
                if (menu.IsExpanded)
                {
                    contentBorder.Visibility = Visibility.Visible;
                    rowDef.Height = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    contentBorder.Visibility = Visibility.Collapsed;
                    rowDef.Height = new GridLength(0);
                }
            };
            updateVisibility();

            menu.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "IsExpanded")
                {
                    this.Dispatcher.Invoke(updateVisibility);
                }
            };

            contentBorder.Drop += DrawerIcons_Drop;

            HandyControl.Controls.ScrollViewer scrollViewer = new HandyControl.Controls.ScrollViewer
            {
                IsInertiaEnabled = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            ListBox iconListBox = new ListBox
            {
                ItemsSource = menu.IconList,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                AllowDrop = true,
                Tag = menu
            };
            iconListBox.Drop += DrawerIcons_Drop;

            // WrapPanel 面板：从左到右依次排列，满行自动换行
            FrameworkElementFactory panelFactory = new FrameworkElementFactory(typeof(WrapPanel));
            panelFactory.SetValue(WrapPanel.OrientationProperty, Orientation.Horizontal);
            panelFactory.SetValue(WrapPanel.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            panelFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(4, 2, 4, 2));
            iconListBox.ItemsPanel = new ItemsPanelTemplate(panelFactory);

            // ItemContainer 样式
            Style itemStyle = new Style(typeof(ListBoxItem));
            itemStyle.Setters.Add(new Setter(FrameworkElement.FocusVisualStyleProperty, null));
            itemStyle.Setters.Add(new Setter(FrameworkElement.ContextMenuProperty, (ContextMenu)this.Resources["IconContextMenu"]));
            
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            FrameworkElementFactory presenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            borderFactory.AppendChild(presenterFactory);
            ControlTemplate itemTemplate = new ControlTemplate(typeof(ListBoxItem));
            itemTemplate.VisualTree = borderFactory;
            itemStyle.Setters.Add(new Setter(System.Windows.Controls.Control.TemplateProperty, itemTemplate));
            iconListBox.ItemContainerStyle = itemStyle;

            // 图标模板
            iconListBox.ItemTemplate = CreateIconDataTemplate();

            scrollViewer.Content = iconListBox;
            contentBorder.Child = scrollViewer;

            return contentBorder;
        }

        private DataTemplate CreateIconDataTemplate()
        {
            DataTemplate template = new DataTemplate();

            FrameworkElementFactory stackPanel = new FrameworkElementFactory(typeof(HandyControl.Controls.SimpleStackPanel));
            stackPanel.SetBinding(FrameworkElement.WidthProperty, new Binding("AppConfig.ImgPanelWidth") { Source = appData });
            stackPanel.SetBinding(FrameworkElement.HeightProperty, new Binding("AppConfig.ImgPanelHeight") { Source = appData });
            stackPanel.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            stackPanel.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            stackPanel.SetValue(FrameworkElement.MarginProperty, new Thickness(3));
            stackPanel.SetValue(FrameworkElement.CursorProperty, Cursors.Hand);
            stackPanel.SetBinding(FrameworkElement.ToolTipProperty, new Binding("Content"));
            stackPanel.SetBinding(FrameworkElement.TagProperty, new Binding());

            stackPanel.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(Icon_MouseLeftButtonDown));

            // 图标图片
            FrameworkElementFactory image = new FrameworkElementFactory(typeof(Image));
            image.SetValue(FrameworkElement.StyleProperty, (Style)this.Resources["DrawerIconImageStyle"]);
            stackPanel.AppendChild(image);

            // 图标标题 (单行不折行，超长显示省略号)
            FrameworkElementFactory textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetBinding(TextBlock.TextProperty, new Binding("Name"));
            textBlock.SetValue(FrameworkElement.MaxWidthProperty, 68.0);
            textBlock.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 3, 0, 0));
            textBlock.SetValue(TextBlock.FontSizeProperty, 12.0);
            textBlock.SetValue(TextBlock.TextWrappingProperty, TextWrapping.NoWrap);
            textBlock.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
            textBlock.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
            textBlock.SetBinding(TextBlock.ForegroundProperty, new Binding("AppConfig.TextColor") { Source = appData });
            textBlock.SetBinding(UIElement.VisibilityProperty, new Binding("AppConfig.ShowIconTitle") { Source = appData, Converter = (IValueConverter)this.Resources["BoolToVisibilityConverter"] });
            stackPanel.AppendChild(textBlock);

            template.VisualTree = stackPanel;
            return template;
        }

        #region 抽屉点击与互斥展开
        private void DrawerHeader_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is MenuInfo menu)
            {
                if (!menu.IsExpanded)
                {
                    menu.IsExpanded = true;
                    appData.AppConfig.SelectedMenuIndex = appData.MenuList.IndexOf(menu);
                    appData.AppConfig.SelectedMenuIcons = menu.IconList;
                }
            }
        }

        private void DrawerHeader_DragEnter(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is MenuInfo menu)
            {
                if (!menu.IsExpanded)
                {
                    menu.IsExpanded = true;
                }
            }
        }
        #endregion

        #region 分类管理事件
        private void CreateMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuInfo info = new MenuInfo()
            {
                MenuEdit = Visibility.Collapsed,
                MenuId = Guid.NewGuid().ToString(),
                MenuName = "新分类",
                IsExpanded = true
            };
            appData.MenuList.Add(info);
            info.IsExpanded = true;
            CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
            InitDrawerLayout();
        }

        private void CreateLinkMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFolderDialog dialog = new OpenFolderDialog
                {
                    Title = "选择关联文件夹",
                    Multiselect = false
                };
                if (dialog.ShowDialog() == true)
                {
                    string menuId = Guid.NewGuid().ToString();
                    string path = dialog.FolderName;

                    MenuInfo menuInfo = new MenuInfo
                    {
                        MenuName = Path.GetFileNameWithoutExtension(path),
                        MenuId = menuId,
                        MenuType = MenuType.LINK,
                        LinkPath = path,
                        IsEncrypt = false,
                        IsExpanded = true
                    };

                    appData.MenuList.Add(menuInfo);
                    menuInfo.IsExpanded = true;
                    CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
                    InitDrawerLayout();

                    new Thread(() =>
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(menuInfo.LinkPath);
                        FileSystemInfo[] fileInfos = dirInfo.GetFileSystemInfos();
                        ObservableCollection<IconInfo> iconList = new ObservableCollection<IconInfo>();
                        foreach (FileSystemInfo fileInfo in fileInfos)
                        {
                            IconInfo iconInfo = CommonCode.GetIconInfoByPath_NoWrite(fileInfo.FullName);
                            iconList.Add(iconInfo);
                        }
                        this.Dispatcher.Invoke(() =>
                        {
                            menuInfo.IconList = iconList;
                        });
                    }).Start();
                }
            }
            catch (Exception ex)
            {
                LogUtil.WriteErrorLog(ex, "新建关联菜单失败!");
            }
        }

        private void RenameMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is MenuInfo menu)
            {
                menu.MenuEdit = Visibility.Visible;
            }
        }

        private void MenuRename_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.Tag is MenuInfo menu)
            {
                menu.MenuName = tb.Text.Trim();
                menu.MenuEdit = Visibility.Collapsed;
                CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
            }
        }

        private void MenuRename_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox tb && tb.Tag is MenuInfo menu)
            {
                menu.MenuName = tb.Text.Trim();
                menu.MenuEdit = Visibility.Collapsed;
                CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
            }
        }

        private void EncryptMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is MenuInfo menu)
            {
                menu.IsEncrypt = !menu.IsEncrypt;
                CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
            }
        }

        private void DeleteMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is MenuInfo menu)
            {
                if (appData.MenuList.Count <= 1)
                {
                    HandyControl.Controls.Growl.Warning("至少保留一个分类!", "MainWindowGrowl");
                    return;
                }
                appData.MenuList.Remove(menu);
                if (appData.MenuList.Count > 0)
                {
                    appData.MenuList[0].IsExpanded = true;
                }
                CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
                InitDrawerLayout();
            }
        }
        #endregion

        #region 图标交互与启动
        private void Icon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (appData.AppConfig.DoubleOpen && e.ClickCount == 2)
            {
                LaunchIcon(sender);
            }
            else if (!appData.AppConfig.DoubleOpen)
            {
                LaunchIcon(sender);
            }
        }

        private void LaunchIcon(object sender)
        {
            if (sender is FrameworkElement element && element.Tag is IconInfo icon)
            {
                icon.Count++;
                ProcessUtil.StartIconApp(icon, IconStartType.DEFAULT_STARTUP);
                if (appData.AppConfig.AppHideType == AppHideType.START_EXE)
                {
                    MainWindow.HideApp();
                }
            }
        }

        private void IconAdminStart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is IconInfo icon)
            {
                ProcessUtil.StartIconApp(icon, IconStartType.ADMIN_STARTUP);
            }
        }

        private void ShowInExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is IconInfo icon)
            {
                ProcessUtil.StartIconApp(icon, IconStartType.SHOW_IN_EXPLORE);
            }
        }

        private void AddUrlIcon_Click(object sender, RoutedEventArgs e)
        {
            IconInfoUrlDialog urlDialog = new IconInfoUrlDialog();
            HandyControl.Controls.Dialog.Show(urlDialog, "MainWindowDialog");
        }

        private void AddSystemIcon_Click(object sender, RoutedEventArgs e)
        {
            SystemItemWindow.Show();
        }

        private void SystemContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is IconInfo icon)
            {
                ShellContextMenu scm = new ShellContextMenu();
                FileInfo[] files = new FileInfo[1];
                files[0] = new FileInfo(icon.Path);
                System.Drawing.Point p = System.Windows.Forms.Cursor.Position;
                scm.ShowContextMenu(files, p);
            }
        }

        private void IconProperty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is IconInfo icon)
            {
                IconInfoDialog dialog = new IconInfoDialog(icon);
                HandyControl.Controls.Dialog.Show(dialog, "MainWindowDialog");
            }
        }

        private void RemoveIcon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is IconInfo icon)
            {
                foreach (var menu in appData.MenuList)
                {
                    if (menu.IconList.Contains(icon))
                    {
                        menu.IconList.Remove(icon);
                        CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
                        break;
                    }
                }
            }
        }
        #endregion

        #region 外部文件拖入
        private void DrawerIcons_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                MenuInfo targetMenu = null;

                if (sender is FrameworkElement element && element.Tag is MenuInfo menu)
                {
                    targetMenu = menu;
                }
                else
                {
                    foreach (var m in appData.MenuList)
                    {
                        if (m.IsExpanded)
                        {
                            targetMenu = m;
                            break;
                        }
                    }
                }

                if (targetMenu != null && files != null)
                {
                    foreach (var file in files)
                    {
                        IconInfo newIcon = CommonCode.GetIconInfoByPath(file);
                        if (newIcon != null)
                        {
                            targetMenu.IconList.Add(newIcon);
                        }
                    }
                    CommonCode.SaveAppData(appData, Constants.DATA_FILE_PATH);
                }
            }
        }

        private void DrawerHeader_Drop(object sender, DragEventArgs e)
        {
            DrawerIcons_Drop(sender, e);
        }
        #endregion
    }
}
