using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Types.Core;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Windows.ApplicationModel.Resources;

namespace Types.WinUI
{
    public sealed partial class MainWindow : Window
    {
        private List<TypeViewModel> _allTypes = new();
        private ResourceLoader _resourceLoader;

        public MainWindow()
        {
            this.InitializeComponent();
            SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            _resourceLoader = new ResourceLoader();
            this.Activated += MainWindow_Activated;

            // Set AppWindow icon
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            
            var iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
            if (System.IO.File.Exists(iconPath))
            {
                appWindow.SetIcon(iconPath);
            }
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            this.Activated -= MainWindow_Activated;
            LoadTypes();
        }

        private async void LoadTypes()
        {
            StatusText.Text = _resourceLoader.GetString("StatusText_Loading");
            
            var list = await Task.Run(async () =>
            {
                var tempList = new List<TypeViewModel>();
                
                // Load Extensions
                foreach (var ext in Types.Core.Type.Extensions)
                {
                    try 
                    {
                        var t = new Types.Core.Type(ext);
                        string iconPath = t.Icon;
                        
                        tempList.Add(new TypeViewModel { 
                            Id = t.ID, 
                            Title = t.Title ?? "", 
                            IconPath = iconPath 
                        });
                    }
                    catch { }
                }
                
                return tempList;
            });

            _allTypes = list;
            
            // Load icons asynchronously
            _ = LoadIconsAsync(_allTypes);

            FilterList(SearchBox.Text);
            StatusText.Text = $"{_allTypes.Count}{_resourceLoader.GetString("StatusText_Loaded")}";
        }

        private async Task LoadIconsAsync(List<TypeViewModel> types)
        {
            foreach (var item in types)
            {
                if (!string.IsNullOrEmpty(item.IconPath))
                {
                    // Check if it's a resource path (contains comma) or just a file
                    if (item.IconPath.Contains(","))
                    {
                        var img = await IconExtractor.GetIconFromResource(item.IconPath);
                        if (img != null)
                        {
                            item.IconImage = img;
                        }
                    }
                    else
                    {
                        // Direct file path, handled by XAML binding usually, but let's be consistent
                        try
                        {
                            if (System.IO.File.Exists(item.IconPath))
                            {
                                var bitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(item.IconPath));
                                item.IconImage = bitmap;
                            }
                        }
                        catch {}
                    }
                }
            }
        }

        private void FilterList(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                TypesList.ItemsSource = _allTypes;
            }
            else
            {
                query = query.ToLower();
                TypesList.ItemsSource = _allTypes.Where(t => 
                    t.Id.ToLower().Contains(query) || 
                    t.Title.ToLower().Contains(query)).ToList();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterList(SearchBox.Text);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTypes();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Settings
        }

        private void TypesList_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (TypesList.SelectedItem is TypeViewModel selected)
            {
                var typeWindow = new TypeWindow(selected.Id);
                typeWindow.Activate();
            }
        }

        private void TypesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EditIconButton.IsEnabled = TypesList.SelectedItem != null;
        }

        private async void EditIcon_Click(object sender, RoutedEventArgs e)
        {
            if (TypesList.SelectedItem is TypeViewModel selected)
            {
                var type = new Types.Core.Type(selected.Id);
                var currentIcon = type.Icon ?? "";

                var dialog = new ContentDialog
                {
                    Title = _resourceLoader.GetString("ChangeIconTitle"),
                    PrimaryButtonText = _resourceLoader.GetString("SaveButton/Content"),
                    SecondaryButtonText = _resourceLoader.GetString("ApplyButton/Content"),
                    CloseButtonText = _resourceLoader.GetString("CancelButton/Content"),
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.Content.XamlRoot
                };

                var stackPanel = new StackPanel { Spacing = 10 };
                var pathBox = new TextBox 
                { 
                    Header = _resourceLoader.GetString("IconPathHeader"), 
                    Text = currentIcon, 
                    PlaceholderText = _resourceLoader.GetString("IconPathPlaceholder") 
                };

                // Handle Apply button click
                dialog.SecondaryButtonClick += (s, args) =>
                {
                    args.Cancel = true; // Keep dialog open
                    try
                    {
                        type.Icon = pathBox.Text;
                        OS.FlushIcons();
                        StatusText.Text = string.Format(_resourceLoader.GetString("IconApplied"), selected.Id);
                    }
                    catch (Exception ex)
                    {
                        StatusText.Text = $"{_resourceLoader.GetString("ErrorTitle")}: {ex.Message}";
                    }
                };
                
                var browseBtn = new Button { Content = _resourceLoader.GetString("BrowseButton/Content") };
                browseBtn.Click += async (s, args) => 
                {
                    var picker = new Windows.Storage.Pickers.FileOpenPicker();
                    picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                    picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
                    picker.FileTypeFilter.Add(".dll");
                    picker.FileTypeFilter.Add(".exe");
                    picker.FileTypeFilter.Add(".ico");

                    // Initialize the picker with the window handle
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                    WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                    var file = await picker.PickSingleFileAsync();
                    if (file != null)
                    {
                        pathBox.Text = file.Path;
                    }
                };

                stackPanel.Children.Add(pathBox);
                stackPanel.Children.Add(browseBtn);
                dialog.Content = stackPanel;

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    try
                    {
                        type.Icon = pathBox.Text;
                        OS.FlushIcons();
                        StatusText.Text = string.Format(_resourceLoader.GetString("IconUpdated"), selected.Id);
                    }
                    catch (Exception ex)
                    {
                        var errDialog = new ContentDialog
                        {
                            Title = _resourceLoader.GetString("ErrorTitle"),
                            Content = ex.Message,
                            CloseButtonText = "OK",
                            XamlRoot = this.Content.XamlRoot
                        };
                        await errDialog.ShowAsync();
                    }
                }
            }
        }

        // Must be outside the MainWindow class but inside the namespace
    }

    public class TypeViewModel : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string IconPath { get; set; }
        
        private Microsoft.UI.Xaml.Media.ImageSource _iconImage;
        public Microsoft.UI.Xaml.Media.ImageSource IconImage
        {
            get => _iconImage;
            set
            {
                _iconImage = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
