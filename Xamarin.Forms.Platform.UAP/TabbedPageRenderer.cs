using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using WGrid = Windows.UI.Xaml.Controls.Grid;

namespace Xamarin.Forms.Platform.UWP
{
    public class TabbedPageRenderer : IVisualElementRenderer, ITitleProvider, IToolbarProvider
    {
        const string TabBarHeaderTextBlockName = "TabbedPageHeaderTextBlock";
        const string TabBarHeaderGridName = "TabbedPageHeaderGrid";

        Color _barBackgroundColor;
        Color _barTextColor;
        bool _disposed;
        bool _showTitle;
        VisualElementTracker<Page, Pivot> _tracker;

        ITitleProvider TitleProvider => this;

        public FormsPivot Control { get; private set; }

        public TabbedPage Element { get; private set; }

        protected VisualElementTracker<Page, Pivot> Tracker
        {
            get { return _tracker; }
            set
            {
                if (_tracker == value)
                    return;

                if (_tracker != null)
                    _tracker.Dispose();

                _tracker = value;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        Brush ITitleProvider.BarBackgroundBrush
        {
            set { Control.ToolbarBackground = value; }
        }

        Brush ITitleProvider.BarForegroundBrush
        {
            set { Control.ToolbarForeground = value; }
        }

        bool ITitleProvider.ShowTitle
        {
            get { return _showTitle; }

            set
            {
                if (_showTitle == value)
                    return;
                _showTitle = value;

                UpdateTitleVisibility();
            }
        }

        string ITitleProvider.Title
        {
            get { return (string)Control?.Title; }

            set
            {
                if (Control != null && _showTitle)
                    Control.Title = value;
            }
        }

        public Task<CommandBar> GetCommandBarAsync()
        {
            return (Control as IToolbarProvider)?.GetCommandBarAsync();
        }

        public FrameworkElement ContainerElement
        {
            get { return Control; }
        }

        VisualElement IVisualElementRenderer.Element
        {
            get { return Element; }
        }

        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

        public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            var constraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);

            double oldWidth = Control.Width;
            double oldHeight = Control.Height;

            Control.Height = double.NaN;
            Control.Width = double.NaN;

            Control.Measure(constraint);
            var result = new Size(Math.Ceiling(Control.DesiredSize.Width), Math.Ceiling(Control.DesiredSize.Height));

            Control.Width = oldWidth;
            Control.Height = oldHeight;

            return new SizeRequest(result);
        }

		UIElement IVisualElementRenderer.GetNativeElement()
		{
			return Control;
		}

		public void SetElement(VisualElement element)
        {
            if (element != null && !(element is TabbedPage))
                throw new ArgumentException("Element must be a TabbedPage", "element");

            TabbedPage oldElement = Element;
            Element = (TabbedPage)element;

            if (oldElement != null)
            {
                oldElement.PropertyChanged -= OnElementPropertyChanged;
                ((INotifyCollectionChanged)oldElement.Children).CollectionChanged -= OnPagesChanged;
            }

            if (element != null)
            {
                if (Control == null)
                {
                    Control = new FormsPivot
                    {
                        Style = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources["TabbedPageStyle"],
                    };

                    Control.SelectionChanged += OnSelectionChanged;

                    Tracker = new BackgroundTracker<Pivot>(Windows.UI.Xaml.Controls.Control.BackgroundProperty)
                    {
                        Element = (Page)element,
                        Control = Control,
                        Container = Control
                    };

                    Control.Loaded += OnLoaded;
                    Control.Unloaded += OnUnloaded;
                }

                Control.DataContext = Element;
                OnPagesChanged(Element.Children,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                UpdateCurrentPage();
                UpdateToolbarPlacement();

                ((INotifyCollectionChanged)Element.Children).CollectionChanged += OnPagesChanged;
                element.PropertyChanged += OnElementPropertyChanged;

                if (!string.IsNullOrEmpty(element.AutomationId))
					Control.SetValue(Windows.UI.Xaml.Automation.AutomationProperties.AutomationIdProperty, element.AutomationId);
            }

            OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
                return;

            _disposed = true;
            Element?.SendDisappearing();
            SetElement(null);
            Tracker = null;
        }

        protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
        {
            EventHandler<VisualElementChangedEventArgs> changed = ElementChanged;
            if (changed != null)
                changed(this, e);
        }

        void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TabbedPage.CurrentPage))
            {
                UpdateCurrentPage();
                UpdateBarTextColor();
                UpdateBarBackgroundColor();
            }
            else if (e.PropertyName == TabbedPage.BarTextColorProperty.PropertyName)
                UpdateBarTextColor();
            else if (e.PropertyName == TabbedPage.BarBackgroundColorProperty.PropertyName)
                UpdateBarBackgroundColor();
            else if (e.PropertyName == PlatformConfiguration.WindowsSpecific.Page.ToolbarPlacementProperty.PropertyName)
                UpdateToolbarPlacement();
        }

        void OnLoaded(object sender, RoutedEventArgs args)
        {
            Element?.SendAppearing();
            UpdateBarTextColor();
            UpdateBarBackgroundColor();
        }

        void OnPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            e.Apply(Element.Children, Control.Items);

            // Potential performance issue, UpdateLayout () is called for every page change
            Control.UpdateLayout();
        }

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Element == null)
                return;

            Page page = e.AddedItems.Count > 0 ? (Page)e.AddedItems[0] : null;
            Page currentPage = Element.CurrentPage;
            if (currentPage == page)
                return;
            currentPage?.SendDisappearing();
            Element.CurrentPage = page;
            page?.SendAppearing();
        }

        void OnUnloaded(object sender, RoutedEventArgs args)
        {
            Element?.SendDisappearing();
        }

        Brush GetBarBackgroundBrush()
        {
            object defaultColor = new SolidColorBrush(Windows.UI.Colors.Transparent);

            if (Element.BarBackgroundColor.IsDefault && defaultColor != null)
                return (Brush)defaultColor;
            return Element.BarBackgroundColor.ToBrush();
        }

        Brush GetBarForegroundBrush()
        {
            object defaultColor = Windows.UI.Xaml.Application.Current.Resources["ApplicationForegroundThemeBrush"];
            if (Element.BarTextColor.IsDefault && defaultColor != null)
                return (Brush)defaultColor;
            return Element.BarTextColor.ToBrush();
        }

        void UpdateBarBackgroundColor()
        {
            if (Element == null) return;
            var barBackgroundColor = Element.BarBackgroundColor;

            if (barBackgroundColor == _barBackgroundColor) return;
            _barBackgroundColor = barBackgroundColor;

            var controlToolbarBackground = Control.ToolbarBackground;
            if (controlToolbarBackground == null && barBackgroundColor.IsDefault) return;

            var brush = GetBarBackgroundBrush();
            if (brush == controlToolbarBackground) return;

            TitleProvider.BarBackgroundBrush = brush;

            foreach (WGrid tabBarGrid in Control.GetDescendantsByName<WGrid>(TabBarHeaderGridName))
            {
                tabBarGrid.Background = brush;
            }
        }

        void UpdateBarTextColor()
        {
            if (Element == null) return;
            var barTextColor = Element.BarTextColor;

            if (barTextColor == _barTextColor) return;
            _barTextColor = barTextColor;

            var controlToolbarForeground = Control.ToolbarForeground;
            if (controlToolbarForeground == null && barTextColor.IsDefault) return;

            var brush = GetBarForegroundBrush();
            if (brush == controlToolbarForeground)
                return;

            TitleProvider.BarForegroundBrush = brush;

            foreach (TextBlock tabBarTextBlock in Control.GetDescendantsByName<TextBlock>(TabBarHeaderTextBlockName))
            {
                tabBarTextBlock.Foreground = brush;
            }
        }

        void UpdateTitleVisibility()
        {
            Control.TitleVisibility = _showTitle ? Visibility.Visible : Visibility.Collapsed;
        }

        void UpdateCurrentPage()
        {
            Page page = Element.CurrentPage;

            var nav = page as NavigationPage;
            TitleProvider.ShowTitle = nav != null;

			// Enforce consistency rules on toolbar (show toolbar if visible Tab is Navigation Page)
			Control.ShouldShowToolbar = nav != null;

            if (page == null)
                return;

            Control.SelectedItem = page;
        }

        void UpdateToolbarPlacement()
        {
            Control.ToolbarPlacement = Element.OnThisPlatform().GetToolbarPlacement();
        }
    }
}