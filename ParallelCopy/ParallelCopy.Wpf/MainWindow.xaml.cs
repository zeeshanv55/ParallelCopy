namespace ParallelCopy.Wpf
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Interop;
    using Common;
    using Common.Enums;
    using Common.Models;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            Instance = this;
            this.ShowInTaskbar = false;

            this.ClipboardItems = new ObservableCollection<ClipboardItem>();
            ListViewClipboardItems.ItemsSource = this.ClipboardItems;
        }

        #endregion

        #region Properties

        public static MainWindow Instance { get; private set; }

        public ObservableCollection<ClipboardItem> ClipboardItems
        {
            get { return _clipboardItems; }
            set
            {
                _clipboardItems = value;
                OnPropertyChanged("ClipboardItems");
            }
        }

        public ClipboardItem ActiveItem
        {
            get { return _activeItem; }
            set
            {
                _activeItem = value;
                OnPropertyChanged("ActiveItem");
            }
        }

        #endregion

        #region Fields

        private static IntPtr _clipboardViewerNext;

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<ClipboardItem> _clipboardItems;

        private ClipboardItem _activeItem;

        #endregion

        #region Event Handlers

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonRemoveItem_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (ClipboardItem)((ListViewItem)(((StackPanel)((DockPanel)(((Button)sender).Parent)).Parent).TemplatedParent)).DataContext;
            var itemInList = this.ClipboardItems.FirstOrDefault(x => x.Equals(item));
            this.ClipboardItems.Remove(itemInList);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;

            _clipboardViewerNext = User32.SetClipboardViewer(new WindowInteropHelper(this).Handle);
            HwndSource.FromHwnd(new WindowInteropHelper(this).Handle).AddHook(WndProc);

            ShowClipboardContents();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            User32.ChangeClipboardChain(new WindowInteropHelper(this).Handle, _clipboardViewerNext);
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Opacity = 1;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Opacity = 0.25;
        }

        private void ListViewClipboardItems_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ActiveItem = (ClipboardItem)((ListView)sender).SelectedItem;
            this.ActiveItem.Write();
        }

        #endregion

        #region Overrides

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Private Methods

        private static void ShowClipboardContents()
        {
            try
            {
                var item = new ClipboardItem();

                if (item.Exists && !Instance.IsDuplicate(item))
                {
                    Instance.ClipboardItems.Insert(0, item);
                    Instance.ListViewClipboardItems.SelectedIndex = 0;
                }
            }
            catch (Exception excep)
            {

            }
        }

        private bool IsDuplicate(ClipboardItem item)
        {
            return this.ClipboardItems.Any(x => x.Equals(item));
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                //WM_DRAWCLIPBOARD
                case 0x0308:
                    User32.SendMessage(_clipboardViewerNext, msg, wParam, lParam);
                    ShowClipboardContents();
                    break;

                //WM_CHANGECBCHAIN
                case 0x030D:
                    if (wParam == _clipboardViewerNext)
                    {
                        _clipboardViewerNext = lParam;
                    }
                    else
                    {
                        User32.SendMessage(_clipboardViewerNext, msg, wParam, lParam);
                    }

                    break;

                //WM_WINDOWPOSCHANGING
                case 0x0046:
                    Instance.Topmost = true;
                    break;
            }

            return IntPtr.Zero;
        }

        #endregion
    }

    public class ClipboardTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Extensions.GetDescription(typeof(ClipboardItemType), (ClipboardItemType)Enum.Parse(typeof(ClipboardItemType), value.ToString()));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}