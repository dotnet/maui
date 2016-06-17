using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WinRT
{
	public class TabbedPageRenderer
		: IVisualElementRenderer
	{
		Canvas _canvas;

		bool _disposed;
		TabsControl _tabs;
		VisualElementTracker<Page, Canvas> _tracker;

		public TabbedPage Page
		{
			get { return (TabbedPage)Element; }
		}

		IPageController PageController => Element as IPageController;

		protected VisualElementTracker<Page, Canvas> Tracker
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

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public FrameworkElement ContainerElement
		{
			get { return _canvas; }
		}

		public VisualElement Element { get; private set; }

		public void SetElement(VisualElement element)
		{
			if (element != null && !(element is TabbedPage))
				throw new ArgumentException("Element must be a TabbedPage", "element");

			var oldElement = Page;
			Element = element;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
				((INotifyCollectionChanged)oldElement.Children).CollectionChanged -= OnPagesChanged;
			}

			if (element != null)
			{
				if (_tracker == null)
				{
					_tabs = new TabsControl();

					_canvas = new Canvas();

					_canvas.ChildrenTransitions = new TransitionCollection
					{
						new EntranceThemeTransition()
					};

					Tracker = new BackgroundTracker<Canvas>(Panel.BackgroundProperty)
					{
						Element = (Page)element,
						Control = _canvas,
						Container = _canvas
					};

					_canvas.Loaded += OnLoaded;
					_canvas.Unloaded += OnUnloaded;
				}

				_tabs.DataContext = element;

				OnPagesChanged(Page.Children, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
				UpdateCurrentPage();
				UpdateBarTextColor();
				UpdateBarBackgroundColor();

				((INotifyCollectionChanged)Page.Children).CollectionChanged += OnPagesChanged;
				element.PropertyChanged += OnElementPropertyChanged;
			}

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));
		}

		Brush GetBarBackgroundBrush()
		{
			object defaultColor = Windows.UI.Xaml.Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
			if (Page.BarBackgroundColor.IsDefault && defaultColor != null)
				return (Brush)defaultColor;
			return Page.BarBackgroundColor.ToBrush();
		}

		Brush GetBarForegroundBrush()
		{
			object defaultColor = Windows.UI.Xaml.Application.Current.Resources["ApplicationForegroundThemeBrush"];
			if (Page.BarTextColor.IsDefault)
				return (Brush)defaultColor;
			return Page.BarTextColor.ToBrush();
		}

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (_canvas.Children.Count == 0)
				return new SizeRequest();

			var constraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);
			var child = (FrameworkElement)_canvas.Children[0];

			var oldWidth = child.Width;
			var oldHeight = child.Height;

			child.Height = double.NaN;
			child.Width = double.NaN;

			child.Measure(constraint);
			var result = new Size(Math.Ceiling(child.DesiredSize.Width), Math.Ceiling(child.DesiredSize.Height));

			child.Width = oldWidth;
			child.Height = oldHeight;

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

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			var changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		void OnPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			e.Apply(Page.Children, _tabs.Items);
		}

		internal void OnTabSelected(Page tab)
		{
			CloseTabs();
			Page.CurrentPage = tab;
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

		void UpdateBarBackgroundColor()
		{
			_tabs.Background = GetBarBackgroundBrush();
		}

		void UpdateBarTextColor()
		{
			_tabs.Foreground = GetBarForegroundBrush();
		}

		void UpdateCurrentPage()
		{
			var renderer = Page.CurrentPage.GetOrCreateRenderer();
			_canvas.Children.Clear();

			if (renderer != null)
				_canvas.Children.Add(renderer.ContainerElement);
		}

		void OnLoaded(object sender, RoutedEventArgs args)
		{
			if (Page == null)
				return;

			ShowTabs();
			PageController.SendAppearing();
		}

		Windows.UI.Xaml.Controls.Page GetTopPage()
		{
			var frame = Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
			if (frame == null)
				return null;

			return frame.Content as Windows.UI.Xaml.Controls.Page;
		}

		AppBar GetOrCreateAppBar()
		{
			var npage = GetTopPage();
			if (npage == null)
				return null;

			if (npage.TopAppBar == null)
				npage.TopAppBar = new AppBar();

			return npage.TopAppBar;
		}

		void ShowTabs()
		{
			var bar = GetOrCreateAppBar();
			if (bar != null)
				bar.Content = _tabs;
		}

		void CloseTabs()
		{
			var page = GetTopPage();
			if (page == null)
				return;

			if (page.TopAppBar != null)
				page.TopAppBar.IsOpen = false;
		}

		void RemoveTabs()
		{
			var page = GetTopPage();
			if (page == null)
				return;

			// Explicitly unparent this.tabs so we can reuse
			if (page.TopAppBar != null)
				page.TopAppBar.Content = null;

			page.TopAppBar = null;
		}

		void OnUnloaded(object sender, RoutedEventArgs args)
		{
			RemoveTabs();
			PageController?.SendDisappearing();
		}
	}
}