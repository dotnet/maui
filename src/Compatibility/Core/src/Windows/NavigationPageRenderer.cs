using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI.Core;
using Microsoft.UI.Xaml.Data;
using Microsoft.Maui.Controls.Internals;
using static Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Page;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class NavigationPageRenderer : IVisualElementRenderer, ITitleProvider, ITitleIconProvider, 
		ITitleViewProvider, IToolbarProvider, IToolBarForegroundBinder
	{
		PageControl _container;
		Page _currentPage;
		Page _previousPage;

		bool _disposed;

		FlyoutPage _parentFlyoutPage;
		TabbedPage _parentTabbedPage;
		bool _showTitle = true;
		WImageSource _titleIcon;
		VisualElementTracker<Page, PageControl> _tracker;
		EntranceThemeTransition _transition;
		Platform _platform;
		bool _parentsLookedUp = false;

		Platform Platform => _platform ?? (_platform = Platform.Current);

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

		WBrush ITitleProvider.BarBackgroundBrush
		{
			set
			{
				_container.ToolbarBackground = value;
				UpdateTitleOnParents();
			}
		}

		WBrush ITitleProvider.BarForegroundBrush
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

			if (Element != null && Element.CurrentPage is null)
				throw new InvalidOperationException(
					"NavigationPage must have a root Page before being used. Either call PushAsync with a valid Page, or pass a Page to the constructor before usage.");

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

				if (Brush.IsNullOrEmpty(Element.BarBackground))
					UpdateNavigationBarBackgroundColor();
				else
					UpdateNavigationBarBackground();

				UpdateToolbarPlacement();
				UpdateToolbarDynamicOverflowEnabled();
				UpdateTitleIcon();
				UpdateTitleView();

				// Enforce consistency rules on toolbar (show toolbar if top-level page is Navigation Page)
				_container.ShouldShowToolbar = _parentFlyoutPage == null && _parentTabbedPage == null;
				if (_parentTabbedPage != null)
					Element.Appearing += OnElementAppearing;

				Element.PropertyChanged += OnElementPropertyChanged;
				Element.PushRequested += OnPushRequested;
				Element.PopRequested += OnPopRequested;
				Element.PopToRootRequested += OnPopToRootRequested;
				Element.InternalChildren.CollectionChanged += OnChildrenChanged;

				if (!string.IsNullOrEmpty(Element.AutomationId))
					_container.SetValue(Microsoft.UI.Xaml.Automation.AutomationProperties.AutomationIdProperty, Element.AutomationId);

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

			if (_parentFlyoutPage != null)
				_parentFlyoutPage.PropertyChanged -= MultiPagePropertyChanged;

			if (_navManager != null)
			{
				_navManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
			}
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			EventHandler<VisualElementChangedEventArgs> changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		WBrush GetBarBackgroundColorBrush()
		{
			object defaultColor = GetDefaultColor();

			if (Element.BarBackgroundColor.IsDefault && defaultColor != null)
				return (WBrush)defaultColor;
			return Element.BarBackgroundColor.ToBrush();
		}

		WBrush GetBarBackgroundBrush()
		{
			var barBackground = Element.BarBackground;
			object defaultColor = GetDefaultColor();

			if (!Brush.IsNullOrEmpty(barBackground))
				return barBackground.ToBrush();

			if (defaultColor != null)
				return (WBrush)defaultColor;

			return null;
		}

		WBrush GetBarForegroundBrush()
		{
			object defaultColor = Microsoft.UI.Xaml.Application.Current.Resources["ApplicationForegroundThemeBrush"];
			if (Element.BarTextColor.IsDefault)
				return (WBrush)defaultColor;
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
			if (_parentFlyoutPage != null)
				_parentFlyoutPage.PropertyChanged -= MultiPagePropertyChanged;

			foreach (Page parentPage in parentPages)
			{
				_parentTabbedPage = parentPage as TabbedPage;
				_parentFlyoutPage = parentPage as FlyoutPage;
			}

			if (_parentTabbedPage != null)
				_parentTabbedPage.PropertyChanged += MultiPagePropertyChanged;
			if (_parentFlyoutPage != null)
				_parentFlyoutPage.PropertyChanged += MultiPagePropertyChanged;

			UpdateShowTitle();
			UpdateTitleOnParents();
			_parentsLookedUp = true;
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

		void OnBackClicked(object sender, RoutedEventArgs e)
		{
			Element?.SendBackButtonPressed();
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
			else if (e.PropertyName == NavigationPage.TitleIconImageSourceProperty.PropertyName)
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
				UpdateNavigationBarBackgroundColor();
			else if (e.PropertyName == NavigationPage.BarBackgroundProperty.PropertyName)
				UpdateNavigationBarBackground();
			else if (e.PropertyName == Page.PaddingProperty.PropertyName)
				UpdatePadding();
			else if (e.PropertyName == ToolbarPlacementProperty.PropertyName)
				UpdateToolbarPlacement();
			else if (e.PropertyName == ToolbarDynamicOverflowEnabledProperty.PropertyName)
				UpdateToolbarDynamicOverflowEnabled();
			else if (e.PropertyName == NavigationPage.TitleIconImageSourceProperty.PropertyName)
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

			if (_parentFlyoutPage != null)
			{
				UpdateTitleView();
				UpdateTitleIcon();
			}
		}

		void OnNativeSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateContainerArea();
		}

		void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			if (e.Handled)
				return;

			var point = e.GetCurrentPoint(_container);
			if (point == null)
				return;

			if (point.PointerDeviceType != PointerDeviceType.Mouse)
				return;

			if (point.Properties.IsXButton1Pressed)
			{
				e.Handled = true;
				OnBackClicked(_container, e);
			}
		}

		protected virtual void OnPopRequested(object sender, NavigationRequestedEventArgs e)
		{
			var newCurrent = Element.Peek(1);
			SetPage(newCurrent, e.Animated, true);
		}

		protected virtual void OnPopToRootRequested(object sender, NavigationRequestedEventArgs e)
		{
			SetPage(e.Page, e.Animated, true);
		}

		protected virtual void OnPushRequested(object sender, NavigationRequestedEventArgs e)
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
				{
					_currentPage.Cleanup();
					_container.TitleView?.Cleanup();
				}

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
			UpdateTitleView();

			SetupPageTransition(_transition, isAnimated, isPopping);

			_container.Content = renderer.ContainerElement;
			_container.DataContext = page;
		}

		protected virtual void SetupPageTransition(Transition transition, bool isAnimated, bool isPopping)
		{
			if (isAnimated && transition == null)
			{
				transition  = new EntranceThemeTransition();
				_transition = (EntranceThemeTransition)transition;
				_container.ContentTransitions = new TransitionCollection();
			}

			if (!isAnimated && _container.ContentTransitions?.Count > 0)
			{
				_container.ContentTransitions.Clear();
			}
			else if (isAnimated && _container.ContentTransitions.Contains(transition) == false)
			{
				_container.ContentTransitions.Clear();
				_container.ContentTransitions.Add(transition);
			}
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

		void UpdateNavigationBarBackgroundColor()
		{
			(this as ITitleProvider).BarBackgroundBrush = GetBarBackgroundColorBrush();
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

			ImageSource source = NavigationPage.GetTitleIconImageSource(_currentPage);

			_titleIcon = await source.ToWindowsImageSourceAsync();

			_container.TitleIcon = _titleIcon;

			if (_parentFlyoutPage != null && Platform.GetRenderer(_parentFlyoutPage) is ITitleIconProvider parent)
				parent.TitleIcon = _titleIcon;

			_container.UpdateLayout();
			UpdateContainerArea();
		}

		void UpdateTitleView()
		{
			// if the life cycle hasn't reached the point where _parentFlyoutPage gets wired up then 
			// don't update the title view
			if (_currentPage == null || !_parentsLookedUp)
				return;

			// If the container TitleView gets initialized before the FP TitleView it causes the 
			// FP TitleView to not render correctly
			if (_parentFlyoutPage != null)
			{
				if (Platform.GetRenderer(_parentFlyoutPage) is ITitleViewProvider parent)
					parent.TitleView = TitleView;
			}
			else if (_parentFlyoutPage == null)
				_container.TitleView = TitleView;

		}

		SystemNavigationManager _navManager;

		public void BindForegroundColor(AppBar appBar)
		{
			SetAppBarForegroundBinding(appBar);
		}

		public void BindForegroundColor(AppBarButton button)
		{
			SetAppBarForegroundBinding(button);
		}

		void SetAppBarForegroundBinding(FrameworkElement element)
		{
			element.SetBinding(Control.ForegroundProperty,
				new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath("TitleBrush"), Source = _container, RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent } });
		}

		void UpdateToolbarPlacement()
		{
			if (_container == null)
			{
				return;
			}

			_container.ToolbarPlacement = Element.OnThisPlatform().GetToolbarPlacement();
		}

		void UpdateToolbarDynamicOverflowEnabled()
		{
			if (_container == null)
			{
				return;
			}

			_container.ToolbarDynamicOverflowEnabled = Element.OnThisPlatform().GetToolbarDynamicOverflowEnabled();
		}
		

		void UpdateShowTitle()
		{
			((ITitleProvider)this).ShowTitle = _parentTabbedPage == null && _parentFlyoutPage == null;
		}

		static object GetDefaultColor()
		{
			return Microsoft.UI.Xaml.Application.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"];
		}

		void UpdateBackButton()
		{
			if (_navManager == null || _currentPage == null)
			{
				return;
			}

			bool showBackButton = Element.InternalChildren.Count > 1 && NavigationPage.GetHasBackButton(_currentPage);
			_navManager.AppViewBackButtonVisibility = showBackButton ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
			_container.SetBackButtonTitle(Element);
		}

		async void UpdateTitleOnParents()
		{
			if (Element == null || _currentPage == null)
				return;

			ITitleProvider render = null;
			if (_parentTabbedPage != null)
			{
				render = Platform.GetRenderer(_parentTabbedPage) as ITitleProvider;
				if (render != null)
					render.ShowTitle = (_parentTabbedPage.CurrentPage == Element) && NavigationPage.GetHasNavigationBar(_currentPage);
			}

			if (_parentFlyoutPage != null)
			{
				render = Platform.GetRenderer(_parentFlyoutPage) as ITitleProvider;
				if (render != null)
					render.ShowTitle = (_parentFlyoutPage.Detail == Element) && NavigationPage.GetHasNavigationBar(_currentPage);
			}

			if (render != null && render.ShowTitle)
			{
				render.Title = _currentPage.Title;

				if (!Brush.IsNullOrEmpty(Element.BarBackground))
					render.BarBackgroundBrush = GetBarBackgroundBrush();
				else
					render.BarBackgroundBrush = GetBarBackgroundColorBrush();

				render.BarForegroundBrush = GetBarForegroundBrush();
			}

			if (_showTitle || (render != null && render.ShowTitle))
			{
				if (Platform != null)
				{
					await Platform.UpdateToolbarItems();
				}
			}
		}
	}
}
