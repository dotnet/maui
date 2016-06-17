using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WinRT
{
	internal class TabbedPagePresenter : Windows.UI.Xaml.Controls.ContentPresenter
	{
		public TabbedPagePresenter()
		{
			SizeChanged += (s, e) => {
				if (ActualWidth > 0 && ActualHeight > 0)
				{
					var tab = ((Page)DataContext);
					((IPageController)tab.RealParent).ContainerArea = new Rectangle(0, 0, ActualWidth, ActualHeight);
				}
			};
		}
	}

	public class TabbedPageRenderer
		: IVisualElementRenderer, ITitleProvider
	{
		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public FrameworkElement ContainerElement
		{
			get { return Control; }
		}

		VisualElement IVisualElementRenderer.Element
		{
			get { return Element; }
		}

		public FormsPivot Control
		{
			get;
			private set;
		}

		public TabbedPage Element
		{
			get;
			private set;
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
					Control = new FormsPivot {
						Style = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources["TabbedPageStyle"]
					};
					Control.HeaderTemplate = (Windows.UI.Xaml.DataTemplate)Windows.UI.Xaml.Application.Current.Resources["TabbedPageHeader"];
					Control.ItemTemplate = (Windows.UI.Xaml.DataTemplate)Windows.UI.Xaml.Application.Current.Resources["TabbedPage"];

					Control.SelectionChanged += OnSelectionChanged;

					Tracker = new BackgroundTracker<Pivot>(Windows.UI.Xaml.Controls.Control.BackgroundProperty) {
						Element = (Page)element,
						Control = Control,
						Container = Control
					};

					Control.Loaded += OnLoaded;
					Control.Unloaded += OnUnloaded;
				}

				Control.DataContext = Element;
				OnPagesChanged(Element.Children, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
				UpdateCurrentPage();
				UpdateBarTextColor();
				UpdateBarBackgroundColor();

				((INotifyCollectionChanged)Element.Children).CollectionChanged += OnPagesChanged;
				element.PropertyChanged += OnElementPropertyChanged;

				if (!string.IsNullOrEmpty(element.AutomationId))
					Control.SetValue(AutomationProperties.AutomationIdProperty, element.AutomationId);
			}

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));
		}

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var constraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);

			var oldWidth = Control.Width;
			var oldHeight = Control.Height;

			Control.Height = double.NaN;
			Control.Width = double.NaN;

			Control.Measure(constraint);
			var result = new Size(Math.Ceiling(Control.DesiredSize.Width), Math.Ceiling(Control.DesiredSize.Height));

			Control.Width = oldWidth;
			Control.Height = oldHeight;

			return new SizeRequest(result);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || _disposed)
				return;

			_disposed = true;
			SetElement(null);
			Tracker = null;
		}

		protected VisualElementTracker<Page, Pivot> Tracker
		{
			get
			{
				return _tracker;
			}
			set
			{
				if (_tracker == value)
					return;

				if (_tracker != null)
				{
					_tracker.Dispose();
					/*this.tracker.Updated -= OnTrackerUpdated;*/
				}

				_tracker = value;

				/*if (this.tracker != null)
					this.tracker.Updated += OnTrackerUpdated;*/
			}
		}

		bool ITitleProvider.ShowTitle
		{
			get
			{
				return _showTitle;
			}

			set
			{
				if (_showTitle == value)
					return;
				_showTitle = value;

				(Control as FormsPivot).ToolbarVisibility = _showTitle ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		string ITitleProvider.Title
		{
			get
			{
				return (string)Control?.Title;
			}

			set
			{
				if (Control != null)
					Control.Title = value;
			}
		}

		Brush ITitleProvider.BarBackgroundBrush
		{
			set
			{
				(Control as FormsPivot).ToolbarBackground = value;
			}
		}

		Brush ITitleProvider.BarForegroundBrush
		{
			set
			{
				(Control as FormsPivot).ToolbarForeground = value;
			}
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			var changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		bool _disposed;
		VisualElementTracker<Page, Pivot> _tracker;
		bool _showTitle;

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

			Page page = (e.AddedItems.Count > 0) ? (Page)e.AddedItems[0] : null;
			Element.CurrentPage = page;
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(TabbedPage.CurrentPage))
				UpdateCurrentPage();
			else if (e.PropertyName == TabbedPage.BarTextColorProperty.PropertyName)
				UpdateBarTextColor();
			else if (e.PropertyName == TabbedPage.BarBackgroundColorProperty.PropertyName)
				UpdateBarBackgroundColor();
		}

		void UpdateCurrentPage()
		{
			Page page = Element.CurrentPage;
			UpdateTitle(page);

			if (page == null)
				return;
			Control.SelectedItem = page;
		}

		void OnLoaded(object sender, RoutedEventArgs args)
		{
			if (Element == null)
				return;

			((IPageController)Element).SendAppearing();
		}

		void OnUnloaded(object sender, RoutedEventArgs args)
		{
			if (Element == null)
				return;

			((IPageController)Element).SendDisappearing();
		}

		void OnTrackerUpdated(object sender, EventArgs e)
		{

		}

		Brush GetBarBackgroundBrush()
		{
			object defaultColor = Windows.UI.Xaml.Application.Current.Resources["AppBarBackgroundThemeBrush"];
			if (Element.BarBackgroundColor.IsDefault && defaultColor != null)
				return (Brush)defaultColor;
			return Element.BarBackgroundColor.ToBrush();
		}

		Brush GetBarForegroundBrush()
		{
			object defaultColor = Windows.UI.Xaml.Application.Current.Resources["AppBarItemForegroundThemeBrush"];
			if (Element.BarTextColor.IsDefault)
				return (Brush)defaultColor;
			return Element.BarTextColor.ToBrush();
		}

		void UpdateBarBackgroundColor()
		{
			Control.ToolbarBackground = GetBarBackgroundBrush();
		}

		void UpdateBarTextColor()
		{
			Control.ToolbarForeground = GetBarForegroundBrush();
		}

		void UpdateTitle(Page child)
		{
			Control.ClearValue(Pivot.TitleProperty);

			if (child == null)
				return;
			var renderer = Platform.GetRenderer(child);
			var navigationRenderer = renderer as NavigationPageRenderer;
			if (navigationRenderer != null)
			{
				Control.Title = navigationRenderer.Title;
			}
			else {
				((ITitleProvider)this).ShowTitle = false;
			}

		}

	}
}