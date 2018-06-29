using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Xamarin.Forms.Internals;
using static Xamarin.Forms.PlatformConfiguration.WindowsSpecific.Page;
using WImageSource = Windows.UI.Xaml.Media.ImageSource;


using Windows.UI.Core;
namespace Xamarin.Forms.Platform.UWP
{
	public partial class NavigationPageRenderer : IVisualElementRenderer, ITitleProvider, ITitleIconProvider, ITitleViewProvider, IToolbarProvider
	{
		PageControl _container;
		Page _currentPage;
		Page _previousPage;

		bool _disposed;

		MasterDetailPage _parentMasterDetailPage;
		TabbedPage _parentTabbedPage;
		bool _showTitle = true;
		WImageSource _titleIcon;
		VisualElementTracker<Page, PageControl> _tracker;
		EntranceThemeTransition _transition;

		public NavigationPage Element { get; private set; }

		protected VisualElementTracker<Page, PageControl> Tracker
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
			set
			{
				_container.ToolbarBackground = value;
				UpdateTitleOnParents();
			}
		}

		Brush ITitleProvider.BarForegroundBrush
		{
			set
			{
				_container.TitleBrush = value;
				UpdateTitleOnParents();
			}
		}

		bool ITitleProvider.ShowTitle
		{
			get { return _showTitle; }
			set
			{
				if (_showTitle == value)
					return;

				_showTitle = value;
				UpdateTitleVisible();
				UpdateTitleOnParents();
			}
		}

		public string Title
		{
			get { return _currentPage?.Title; }

			set { /*Not implemented but required by interface*/ }
		}

		public WImageSource TitleIcon
		{
			get => _titleIcon;
			set => _titleIcon = value;
		}

		public View TitleView
		{
			get
			{
				if (_currentPage == null)
					return null;

				return NavigationPage.GetTitleView(_currentPage) as View;
			}
			set { /*Not implemented but required by interface*/ }
		}

		Task<CommandBar> IToolbarProvider.GetCommandBarAsync()
		{
			return ((IToolbarProvider)_container)?.GetCommandBarAsync();
		}

		public FrameworkElement ContainerElement
		{
			get { return _container; }
		}

		VisualElement IVisualElementRenderer.Element
		{
			get { return Element; }
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var constraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);
			IVisualElementRenderer childRenderer = Platform.GetRenderer(Element.CurrentPage);
			FrameworkElement child = childRenderer.ContainerElement;

			double oldWidth = child.Width;
			double oldHeight = child.Height;

			child.Height = double.NaN;
			child.Width = double.NaN;

			child.Measure(constraint);
			var result = new Size(Math.Ceiling(child.DesiredSize.Width), Math.Ceiling(child.DesiredSize.Height));

			child.Width = oldWidth;
			child.Height = oldHeight;

			return new SizeRequest(result);
		}

		UIElement IVisualElementRenderer.GetNativeElement()
		{
			return null;
		}

		public void SetElement(VisualElement element)
		{
			if (element != null && !(element is NavigationPage))
				throw new ArgumentException("Element must be a Page", nameof(element));

			NavigationPage oldElement = Element;
			Element = (NavigationPage)element;

			if (oldElement != null)
			{
				oldElement.PushRequested -= OnPushRequested;
				oldElement.PopRequested -= OnPopRequested;
				oldElement.PopToRootRequested -= OnPopToRootRequested;
				oldElement.InternalChildren.CollectionChanged -= OnChildrenChanged;
				oldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (element != null)
			{
				if (_container == null)
				{
					_container = new PageControl();
					_container.PointerPressed += OnPointerPressed;
					_container.SizeChanged += OnNativeSizeChanged;

					Tracker = new BackgroundTracker<PageControl>(Control.BackgroundProperty) { Element = (Page)element, Container = _container };

					SetPage(Element.CurrentPage, false, false);

					_container.Loaded += OnLoaded;
					_container.Unloaded += OnUnloaded;
				}

				_container.DataContext = Element.CurrentPage;

				UpdatePadding();
				LookupRelevantParents();
				UpdateTitleColor();
				UpdateNavigationBarBackground();
				UpdateToolbarPlacement();
				UpdateTitleIcon();
				UpdateTitleView();

				// Enforce consistency rules on toolbar (show toolbar if top-level page is Navigation Page)
				_container.ShouldShowToolbar = _parentMasterDetailPage == null && _parentTabbedPage == null;
				if (_parentTabbedPage != null)
					Element.Appearing += OnElementAppearing;

				Element.PropertyChanged += OnElementPropertyChanged;
				Element.PushRequested += OnPushRequested;
				Element.PopRequested += OnPopRequested;
				Element.PopToRootRequested += OnPopToRootRequested;
				Element.InternalChildren.CollectionChanged += OnChildrenChanged;

				if (!string.IsNullOrEmpty(Element.AutomationId))
					_container.SetValue(Windows.UI.Xaml.Automation.AutomationProperties.AutomationIdProperty, Element.AutomationId);

				PushExistingNavigationStack();
			}

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));
		}

		protected void Dispose(bool disposing)
		{
			if (_disposed || !disposing)
			{
				return;
			}

			Element?.SendDisappearing();
			_disposed = true;

			_container.PointerPressed -= OnPointerPressed;
			_container.SizeChanged -= OnNativeSizeChanged;
			_container.Loaded -= OnLoaded;
			_container.Unloaded -= OnUnloaded;

			if (_parentTabbedPage != null)
				Element.Appearing -= OnElementAppearing;

			SetElement(null);
			SetPage(null, false, true);
			_previousPage = null;

			if (_parentTabbedPage != null)
				_parentTabbedPage.PropertyChanged -= MultiPagePropertyChanged;

			if (_parentMasterDetailPage != null)
				_parentMasterDetailPage.PropertyChanged -= MultiPagePropertyChanged;

			if (_navManager != null)
			{
				_navManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
			}
		}

		protected void OnElementChanged(VisualElementChangedEventArgs e)
		{
			EventHandler<VisualElementChangedEventArgs> changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		Brush GetBarBackgroundBrush()
		{
			object defaultColor = GetDefaultColor();

			if (Element.BarBackgroundColor.IsDefault && defaultColor != null)
				return (Brush)defaultColor;
			return Element.BarBackgroundColor.ToBrush();
		}

		Brush GetBarForegroundBrush()
		{
			object defaultColor = Windows.UI.Xaml.Application.Current.Resources["ApplicationForegroundThemeBrush"];
			if (Element.BarTextColor.IsDefault)
				return (Brush)defaultColor;
			return Element.BarTextColor.ToBrush();
		}

		bool GetIsNavBarPossible()
		{
			return _showTitle;
		}

		void LookupRelevantParents()
		{
			IEnumerable<Page> parentPages = Element.GetParentPages();

			if (_parentTabbedPage != null)
				_parentTabbedPage.PropertyChanged -= MultiPagePropertyChanged;
			if (_parentMasterDetailPage != null)
				_parentMasterDetailPage.PropertyChanged -= MultiPagePropertyChanged;

			foreach (Page parentPage in parentPages)
			{
				_parentTabbedPage = parentPage as TabbedPage;
				_parentMasterDetailPage = parentPage as MasterDetailPage;
			}

			if (_parentTabbedPage != null)
				_parentTabbedPage.PropertyChanged += MultiPagePropertyChanged;
			if (_parentMasterDetailPage != null)
				_parentMasterDetailPage.PropertyChanged += MultiPagePropertyChanged;

			UpdateShowTitle();

			UpdateTitleOnParents();

			UpdateTitleIcon();

			UpdateTitleView();
		}

		void MultiPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentPage" || e.PropertyName == "Detail")
			{
				UpdateTitleOnParents();
				UpdateTitleIcon();
				UpdateTitleView();
			}
		}

		async void OnBackClicked(object sender, RoutedEventArgs e)
		{
			await Element.PopAsync();
		}

		void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateBackButton();
		}

		void OnCurrentPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == NavigationPage.HasBackButtonProperty.PropertyName)
				UpdateBackButton();
			else if (e.PropertyName == NavigationPage.BackButtonTitleProperty.PropertyName)
				UpdateBackButtonTitle();
			else if (e.PropertyName == NavigationPage.HasNavigationBarProperty.PropertyName)
				UpdateTitleVisible();
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateTitleOnParents();
			else if (e.PropertyName == NavigationPage.TitleIconProperty.PropertyName)
				UpdateTitleIcon();
			else if (e.PropertyName == NavigationPage.TitleViewProperty.PropertyName)
				UpdateTitleView();
		}

		void OnElementAppearing(object sender, EventArgs e)
		{
			UpdateTitleVisible();
			UpdateBackButton();
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == NavigationPage.BarTextColorProperty.PropertyName)
				UpdateTitleColor();
			else if (e.PropertyName == NavigationPage.BarBackgroundColorProperty.PropertyName)
				UpdateNavigationBarBackground();
			else if (e.PropertyName == Page.PaddingProperty.PropertyName)
				UpdatePadding();
			else if (e.PropertyName == ToolbarPlacementProperty.PropertyName)
				UpdateToolbarPlacement();
			else if (e.PropertyName == NavigationPage.TitleIconProperty.PropertyName)
				UpdateTitleIcon();
			else if (e.PropertyName == NavigationPage.TitleViewProperty.PropertyName)
				UpdateTitleView();
		}

		void OnLoaded(object sender, RoutedEventArgs args)
		{
			if (Element == null)
				return;

			_navManager = SystemNavigationManager.GetForCurrentView();
			Element.SendAppearing();
			UpdateBackButton();
			UpdateTitleOnParents();
		}

		void OnNativeSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateContainerArea();
		}

		void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			if (e.Handled)
				return;

			PointerPoint point = e.GetCurrentPoint(_container);
			if (point == null)
				return;

			if (point.PointerDevice.PointerDeviceType != PointerDeviceType.Mouse)
				return;

			if (point.Properties.IsXButton1Pressed)
			{
				e.Handled = true;
				OnBackClicked(_container, e);
			}
		}

		void OnPopRequested(object sender, NavigationRequestedEventArgs e)
		{
			var newCurrent = Element.Peek(1);
			SetPage(newCurrent, e.Animated, true);
		}

		void OnPopToRootRequested(object sender, NavigationRequestedEventArgs e)
		{
			SetPage(e.Page, e.Animated, true);
		}

		void OnPushRequested(object sender, NavigationRequestedEventArgs e)
		{
			SetPage(e.Page, e.Animated, false);
		}

		void OnUnloaded(object sender, RoutedEventArgs args)
		{
			Element?.SendDisappearing();
		}

		void PushExistingNavigationStack()
		{
			foreach (var page in Element.Pages)
			{
				SetPage(page, false, false);
			}
		}

		void SetPage(Page page, bool isAnimated, bool isPopping)
		{
			if (_currentPage != null)
			{
				if (isPopping)
					_currentPage.Cleanup();

				_container.Content = null;
				_currentPage.PropertyChanged -= OnCurrentPagePropertyChanged;
			}

			if (!isPopping)
				_previousPage = _currentPage;

			_currentPage = page;

			if (page == null)
				return;

			UpdateBackButton();
			UpdateBackButtonTitle();

			page.PropertyChanged += OnCurrentPagePropertyChanged;

			IVisualElementRenderer renderer = page.GetOrCreateRenderer();

			UpdateTitleVisible();
			UpdateTitleOnParents();
			UpdateTitleIcon();
			UpdateTitleView();

			if (isAnimated && _transition == null)
			{
				_transition = new EntranceThemeTransition();
				_container.ContentTransitions = new TransitionCollection();
			}

			if (!isAnimated && _transition != null)
				_container.ContentTransitions.Remove(_transition);
			else if (isAnimated && _container.ContentTransitions.Count == 0)
				_container.ContentTransitions.Add(_transition);

			_container.Content = renderer.ContainerElement;
			_container.DataContext = page;
		}

		void UpdateBackButtonTitle()
		{
			string title = null;
			if (_previousPage != null)
				title = NavigationPage.GetBackButtonTitle(_previousPage);

			_container.BackButtonTitle = title;
		}

		void UpdateContainerArea()
		{
			Element.ContainerArea = new Rectangle(0, 0, _container.ContentWidth, _container.ContentHeight);
		}

		void UpdateNavigationBarBackground()
		{
			(this as ITitleProvider).BarBackgroundBrush = GetBarBackgroundBrush();
		}

		void UpdateTitleVisible()
		{
			UpdateTitleOnParents();

			bool showing = _container.TitleVisibility == Visibility.Visible;
			bool newValue = GetIsNavBarPossible() && NavigationPage.GetHasNavigationBar(_currentPage);
			if (showing == newValue)
				return;

			_container.TitleVisibility = newValue ? Visibility.Visible : Visibility.Collapsed;

			// Force ContentHeight/Width to update, doesn't work from inside PageControl for some reason
			_container.UpdateLayout();
			UpdateContainerArea();
		}

		void UpdatePadding()
		{
			_container.TitleInset = Element.Padding.Left;
		}

		void UpdateTitleColor()
		{
			(this as ITitleProvider).BarForegroundBrush = GetBarForegroundBrush();
		}

		async void UpdateTitleIcon()
		{
			if (_currentPage == null)
				return;

			ImageSource source = NavigationPage.GetTitleIcon(_currentPage);

			_titleIcon = await source.ToWindowsImageSource();

			_container.TitleIcon = _titleIcon;

			if (_parentMasterDetailPage != null && Platform.GetRenderer(_parentMasterDetailPage) is ITitleIconProvider parent)
				parent.TitleIcon = _titleIcon;

			_container.UpdateLayout();
			UpdateContainerArea();
		}

		void UpdateTitleView()
		{
			if (_currentPage == null)
				return;

			_container.TitleView = TitleView;

			if (_parentMasterDetailPage != null && Platform.GetRenderer(_parentMasterDetailPage) is ITitleViewProvider parent)
				parent.TitleView = TitleView;
		}
	}
}
