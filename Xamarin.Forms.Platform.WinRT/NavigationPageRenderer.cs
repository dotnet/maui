using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
using Windows.UI.Core;

#endif

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class NavigationPageRenderer : IVisualElementRenderer, ITitleProvider, IToolbarProvider
#if WINDOWS_UWP
										  , IToolBarForegroundBinder
#endif
	{
		PageControl _container;
		Page _currentPage;
		Page _previousPage;

		bool _disposed;
#if WINDOWS_UWP
		SystemNavigationManager _navManager;
#endif
		MasterDetailPage _parentMasterDetailPage;
		TabbedPage _parentTabbedPage;
		bool _showTitle = true;
		VisualElementTracker<Page, PageControl> _tracker;
		ContentThemeTransition _transition;

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

		IPageController PageController => Element as IPageController;

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

			set { }
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

		public void SetElement(VisualElement element)
		{
			if (element != null && !(element is NavigationPage))
				throw new ArgumentException("Element must be a Page", "element");

			NavigationPage oldElement = Element;
			Element = (NavigationPage)element;

			if (oldElement != null)
			{
				((INavigationPageController)oldElement).PushRequested -= OnPushRequested;
				((INavigationPageController)oldElement).PopRequested -= OnPopRequested;
				((INavigationPageController)oldElement).PopToRootRequested -= OnPopToRootRequested;
				((IPageController)oldElement).InternalChildren.CollectionChanged -= OnChildrenChanged;
				oldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (element != null)
			{
				if (_container == null)
				{
					_container = new PageControl();
					_container.PointerPressed += OnPointerPressed;
					_container.SizeChanged += OnNativeSizeChanged;
					_container.BackClicked += OnBackClicked;

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
				Element.PropertyChanged += OnElementPropertyChanged;
				((INavigationPageController)Element).PushRequested += OnPushRequested;
				((INavigationPageController)Element).PopRequested += OnPopRequested;
				((INavigationPageController)Element).PopToRootRequested += OnPopToRootRequested;
				PageController.InternalChildren.CollectionChanged += OnChildrenChanged;

				if (!string.IsNullOrEmpty(Element.AutomationId))
					_container.SetValue(AutomationProperties.AutomationIdProperty, Element.AutomationId);

				PushExistingNavigationStack();
			}

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));
		}

		protected void Dispose(bool disposing)
		{
			if (!disposing || _disposed)
				return;
			PageController?.SendDisappearing();
			_disposed = true;

			_container.PointerPressed -= OnPointerPressed;
			_container.SizeChanged -= OnNativeSizeChanged;
			_container.BackClicked -= OnBackClicked;

			SetElement(null);
			SetPage(null, false, true);
			_previousPage = null;

			if (_parentTabbedPage != null)
				_parentTabbedPage.PropertyChanged -= MultiPagePropertyChanged;

			if (_parentMasterDetailPage != null)
				_parentMasterDetailPage.PropertyChanged -= MultiPagePropertyChanged;

#if WINDOWS_UWP
			if (_navManager != null)
			{
				_navManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
			}
#endif
		}

		protected void OnElementChanged(VisualElementChangedEventArgs e)
		{
			EventHandler<VisualElementChangedEventArgs> changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		Brush GetBarBackgroundBrush()
		{
#if WINDOWS_UWP
			object defaultColor = Windows.UI.Xaml.Application.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"];
#else
			object defaultColor = Windows.UI.Xaml.Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
#endif
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

        // TODO EZH Why don't this and GetToolBarProvider ever get called on either platform?
		Task<CommandBar> GetCommandBarAsync()
		{
			var platform = (Platform)Element.Platform;
			IToolbarProvider toolbarProvider = platform.GetToolbarProvider();
			if (toolbarProvider == null)
				return Task.FromResult<CommandBar>(null);

			return toolbarProvider.GetCommandBarAsync();
		}

		bool GetIsNavBarPossible()
		{
			return _showTitle;
		}

		IToolbarProvider GetToolbarProvider()
		{
			var platform = (Platform)Element.Platform;
			return platform.GetToolbarProvider();
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
#if WINDOWS_UWP
			((ITitleProvider)this).ShowTitle = _parentTabbedPage == null && _parentMasterDetailPage == null;
#else
			if (Device.Idiom == TargetIdiom.Phone && _parentTabbedPage != null)
				((ITitleProvider)this).ShowTitle = false;
			else
				((ITitleProvider)this).ShowTitle = true;
#endif
			UpdateTitleOnParents();
		}

		void MultiPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentPage" || e.PropertyName == "Detail")
				UpdateTitleOnParents();
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
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == NavigationPage.BarTextColorProperty.PropertyName)
				UpdateTitleColor();
			else if (e.PropertyName == NavigationPage.BarBackgroundColorProperty.PropertyName)
				UpdateNavigationBarBackground();
			else if (e.PropertyName == Page.PaddingProperty.PropertyName)
				UpdatePadding();
            else if (e.PropertyName == PlatformConfiguration.WindowsSpecific.Page.ToolbarPlacementProperty.PropertyName)
				UpdateToolbarPlacement();
		}

		void OnLoaded(object sender, RoutedEventArgs args)
		{
			if (Element == null)
				return;

#if WINDOWS_UWP
			_navManager = SystemNavigationManager.GetForCurrentView();
#endif
			PageController.SendAppearing();
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
			var newCurrent = (Page)PageController.InternalChildren[PageController.InternalChildren.Count - 2];
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
			PageController?.SendDisappearing();
		}

		void PushExistingNavigationStack()
		{
			for (int i = ((INavigationPageController)Element).StackCopy.Count - 1; i >= 0; i--)
				SetPage(((INavigationPageController)Element).StackCopy.ElementAt(i), false, false);
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

			if (isAnimated && _transition == null)
			{
				_transition = new ContentThemeTransition();
				_container.ContentTransitions = new TransitionCollection();
			}

			if (!isAnimated && _transition != null)
				_container.ContentTransitions.Remove(_transition);
			else if (isAnimated && _container.ContentTransitions.Count == 0)
				_container.ContentTransitions.Add(_transition);

			_container.Content = renderer.ContainerElement;
			_container.DataContext = page;
		}

		void UpdateBackButton()
		{
			bool showBackButton = PageController.InternalChildren.Count > 1 && NavigationPage.GetHasBackButton(_currentPage);
			_container.ShowBackButton = showBackButton;

#if WINDOWS_UWP
			if (_navManager != null)
			{
				_navManager.AppViewBackButtonVisibility = showBackButton ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
			}
#endif
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
			PageController.ContainerArea = new Rectangle(0, 0, _container.ContentWidth, _container.ContentHeight);
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

        void UpdateToolbarPlacement()
		{
#if WINDOWS_UWP
            if (_container == null)
            {
                return;
            }

            _container.ToolbarPlacement = Element.OnThisPlatform().GetToolbarPlacement();
#endif
		}

#pragma warning disable 1998 // considered for removal
		async void UpdateTitleOnParents()
#pragma warning restore 1998
		{
			if (Element == null)
				return;

			ITitleProvider render = null;
			if (_parentTabbedPage != null)
			{
				render = Platform.GetRenderer(_parentTabbedPage) as ITitleProvider;
				if (render != null)
					render.ShowTitle = (_parentTabbedPage.CurrentPage == Element) && NavigationPage.GetHasNavigationBar(_currentPage);
			}

			if (_parentMasterDetailPage != null)
			{
				render = Platform.GetRenderer(_parentMasterDetailPage) as ITitleProvider;
				if (render != null)
					render.ShowTitle = (_parentMasterDetailPage.Detail == Element) && NavigationPage.GetHasNavigationBar(_currentPage);
			}

			if (render != null && render.ShowTitle)
			{
				render.Title = _currentPage.Title;
				render.BarBackgroundBrush = GetBarBackgroundBrush();
				render.BarForegroundBrush = GetBarForegroundBrush();
#if WINDOWS_UWP
				await (Element.Platform as Platform).UpdateToolbarItems();
#endif
			}
			else if (_showTitle)
			{
#if WINDOWS_UWP
				await (Element.Platform as Platform).UpdateToolbarItems();
#endif
			}
		}

#if WINDOWS_UWP
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
				new Windows.UI.Xaml.Data.Binding { Path = new PropertyPath("TitleBrush"), Source = _container, RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent } });
		}
#endif
	}
}