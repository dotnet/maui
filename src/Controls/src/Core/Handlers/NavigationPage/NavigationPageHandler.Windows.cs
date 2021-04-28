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
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class NavigationPageHandler : 
		ViewHandler<NavigationPage, PageControl>, ITitleProvider, ITitleIconProvider, 
		ITitleViewProvider, IToolbarProvider, IToolBarForegroundBinder, IViewHandler
	{
		Page _currentPage;
		Page _previousPage;

		FlyoutPage _parentFlyoutPage;
		TabbedPage _parentTabbedPage;
		bool _showTitle = true;
		VisualElementTracker<Page, PageControl> _tracker;
		EntranceThemeTransition _transition;
		bool _parentsLookedUp = false;

		public void BindForegroundColor(AppBar appBar)
		{
			SetAppBarForegroundBinding(appBar);
		}

		public void BindForegroundColor(AppBarButton button)
		{
			SetAppBarForegroundBinding(button);
		}

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

		//public void Dispose()
		//{
		//	Dispose(true);
		//}

		void SetAppBarForegroundBinding(FrameworkElement element)
		{
			element.SetBinding(Control.ForegroundProperty,
				new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath("TitleBrush"), Source = NativeView, RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent } });
		}

		WBrush ITitleProvider.BarBackgroundBrush
		{
			set
			{
				NativeView.ToolbarBackground = value;
				UpdateTitleOnParents();
			}
		}

		WBrush ITitleProvider.BarForegroundBrush
		{
			set
			{
				NativeView.TitleBrush = value;
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

		public WImageSource TitleIcon { get; set; }

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
			return ((IToolbarProvider)NativeView)?.GetCommandBarAsync();
		}

		public FrameworkElement ContainerElement
		{
			get { return NativeView; }
		}

		IView IViewHandler.VirtualView
		{
			get { return VirtualView; }
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var constraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);
			IViewHandler childRenderer = VirtualView.CurrentPage.Handler;
			FrameworkElement child = childRenderer.NativeView as FrameworkElement;

			double oldWidth = child.Width;
			double oldHeight = child.Height;

			child.Height = double.NaN;
			child.Width = double.NaN;

			child.Measure(constraint);
			var result = new Size(Math.Ceiling(child.DesiredSize.Width), Math.Ceiling(child.DesiredSize.Height));

			child.Width = oldWidth;
			child.Height = oldHeight;

			return result;
		}

		object IViewHandler.NativeView
		{
			get
			{
				return NativeView;
			}
		}

		protected override PageControl CreateNativeView()
		{
			return new PageControl();
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			if (view != null && !(view is NavigationPage))
				throw new ArgumentException("VirtualView must be a Page", nameof(view));

			if (view is NavigationPage np && np != null && np.CurrentPage is null)
				throw new InvalidOperationException(
					"NavigationPage must have a root Page before being used. Either call PushAsync with a valid Page, or pass a Page to the constructor before usage.");

		}

		protected override void ConnectHandler(PageControl nativeView)
		{
			base.ConnectHandler(nativeView);

			NativeView.PointerPressed += OnPointerPressed;
			NativeView.SizeChanged += OnNativeSizeChanged;
			Tracker = new BackgroundTracker<PageControl>(Control.BackgroundProperty) 
			{
				Element = VirtualView, 
				Container = NativeView 
			};

			SetPage(VirtualView.CurrentPage, false, false);

			NativeView.Loaded += OnLoaded;
			NativeView.Unloaded += OnUnloaded;

			NativeView.DataContext = VirtualView.CurrentPage;

			// Move this somewhere else
			LookupRelevantParents();

			// Enforce consistency rules on toolbar (show toolbar if top-level page is Navigation Page)
			NativeView.ShouldShowToolbar = _parentFlyoutPage == null && _parentTabbedPage == null;
			if (_parentTabbedPage != null)
				VirtualView.Appearing += OnElementAppearing;

			VirtualView.PushRequested += OnPushRequested;
			VirtualView.PopRequested += OnPopRequested;
			VirtualView.PopToRootRequested += OnPopToRootRequested;
			VirtualView.InternalChildren.CollectionChanged += OnChildrenChanged;

			if (!string.IsNullOrEmpty(VirtualView.AutomationId))
				NativeView.SetValue(Microsoft.UI.Xaml.Automation.AutomationProperties.AutomationIdProperty, VirtualView.AutomationId);

			PushExistingNavigationStack();
		}

		protected override void DisconnectHandler(PageControl nativeView)
		{
			base.DisconnectHandler(nativeView);

			if (VirtualView == null)
				return;

			VirtualView.PushRequested -= OnPushRequested;
			VirtualView.PopRequested -= OnPopRequested;
			VirtualView.PopToRootRequested -= OnPopToRootRequested;
			VirtualView.InternalChildren.CollectionChanged -= OnChildrenChanged;
			NativeView.PointerPressed -= OnPointerPressed;
			NativeView.SizeChanged -= OnNativeSizeChanged;
			NativeView.Loaded -= OnLoaded;
			NativeView.Unloaded -= OnUnloaded;

			// from Dispose
			VirtualView?.SendDisappearing();

			NativeView.PointerPressed -= OnPointerPressed;
			NativeView.SizeChanged -= OnNativeSizeChanged;
			NativeView.Loaded -= OnLoaded;
			NativeView.Unloaded -= OnUnloaded;

			if (_parentTabbedPage != null)
				VirtualView.Appearing -= OnElementAppearing;

			SetPage(null, false, true);
			_previousPage = null;

			if (_parentTabbedPage != null)
				_parentTabbedPage.PropertyChanged -= MultiPagePropertyChanged;

			if (_parentFlyoutPage != null)
				_parentFlyoutPage.PropertyChanged -= MultiPagePropertyChanged;
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

			if (VirtualView.BarBackgroundColor.IsDefault() && defaultColor != null)
				return (WBrush)defaultColor;
			return Maui.ColorExtensions.ToNative(VirtualView.BarBackgroundColor);
		}

		static WBrush GetBarBackgroundBrush(NavigationPage navigationPage)
		{
			var barBackground = navigationPage.BarBackground;
			object defaultColor = GetDefaultColor();

			if (!Brush.IsNullOrEmpty(barBackground))
				return barBackground.ToBrush();

			if (defaultColor != null)
				return (WBrush)defaultColor;

			return null;
		}

		static WBrush GetBarForegroundBrush(NavigationPage navigationPage)
		{
			object defaultColor = Microsoft.UI.Xaml.Application.Current.Resources["ApplicationForegroundThemeBrush"];
			if (navigationPage.BarTextColor.IsDefault())
				return (WBrush)defaultColor;
			return Maui.ColorExtensions.ToNative(navigationPage.BarTextColor);
		}

		bool GetIsNavBarPossible()
		{
			return _showTitle;
		}

		void LookupRelevantParents()
		{
			IEnumerable<IPage> parentPages = VirtualView.GetParentPages();

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
			VirtualView?.SendBackButtonPressed();
		}

		void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateBackButton();
		}

		// TODO MAUI: hmmmmmm can we make this not be property changed based?
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

		void OnLoaded(object sender, RoutedEventArgs args)
		{
			if (VirtualView == null)
				return;

			VirtualView.SendAppearing();
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

			var point = e.GetCurrentPoint(NativeView);
			if (point == null)
				return;

			if (point.PointerDeviceType != PointerDeviceType.Mouse)
				return;

			if (point.Properties.IsXButton1Pressed)
			{
				e.Handled = true;
				OnBackClicked(NativeView, e);
			}
		}

		protected virtual void OnPopRequested(object sender, NavigationRequestedEventArgs e)
		{
			var newCurrent = VirtualView.Peek(1);
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
			VirtualView?.SendDisappearing();
		}

		void PushExistingNavigationStack()
		{
			foreach (var page in VirtualView.Pages)
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
					NativeView.TitleView?.Cleanup();
				}

				NativeView.Content = null;
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

			IViewHandler renderer = page.GetOrCreateHandler(this.MauiContext);

			UpdateTitleVisible();
			UpdateTitleOnParents();
			UpdateTitleView();

			SetupPageTransition(_transition, isAnimated, isPopping);

			NativeView.Content = renderer.NativeView;
			NativeView.DataContext = page;
		}

		protected virtual void SetupPageTransition(Transition transition, bool isAnimated, bool isPopping)
		{
			if (isAnimated && transition == null)
			{
				transition  = new EntranceThemeTransition();
				_transition = (EntranceThemeTransition)transition;
				NativeView.ContentTransitions = new TransitionCollection();
			}

			if (!isAnimated && NativeView.ContentTransitions?.Count > 0)
			{
				NativeView.ContentTransitions.Clear();
			}
			else if (isAnimated && NativeView.ContentTransitions.Contains(transition) == false)
			{
				NativeView.ContentTransitions.Clear();
				NativeView.ContentTransitions.Add(transition);
			}
		}

		void UpdateBackButtonTitle()
		{
			string title = null;
			if (_previousPage != null)
				title = NavigationPage.GetBackButtonTitle(_previousPage);

			NativeView.BackButtonTitle = title;
		}

		void UpdateContainerArea()
		{
			VirtualView.ContainerArea = new Rectangle(0, 0, NativeView.ContentWidth, NativeView.ContentHeight);
		}

		void UpdateTitleVisible()
		{
			UpdateTitleOnParents();

			bool showing = NativeView.TitleVisibility == Visibility.Visible;
			bool newValue = GetIsNavBarPossible() && NavigationPage.GetHasNavigationBar(_currentPage);
			if (showing == newValue)
				return;

			NativeView.TitleVisibility = newValue ? Visibility.Visible : Visibility.Collapsed;

			// Force ContentHeight/Width to update, doesn't work from inside PageControl for some reason
			NativeView.UpdateLayout();
			UpdateContainerArea();
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
			if (_currentPage == null)
			{
				return;
			}

			bool showBackButton = VirtualView.InternalChildren.Count > 1 && NavigationPage.GetHasBackButton(_currentPage);
			if (NativeVersion.IsDesktop)
			{
				//TODO MAUI: this means it's running as a desktop app
			}
			else
			{
				var navManager = SystemNavigationManager.GetForCurrentView();
				if(navManager != null)
					navManager.AppViewBackButtonVisibility = showBackButton ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
			}

			NativeView.SetBackButtonTitle(VirtualView);
		}

		void UpdateTitleOnParents()
		{
			if (VirtualView == null || _currentPage == null)
				return;

			ITitleProvider render = null;
			if (_parentTabbedPage != null)
			{
				render = _parentTabbedPage.Handler as ITitleProvider;
				if (render != null)
					render.ShowTitle = (_parentTabbedPage.CurrentPage == VirtualView) && NavigationPage.GetHasNavigationBar(_currentPage);
			}

			if (_parentFlyoutPage != null)
			{
				render = _parentFlyoutPage.Handler as ITitleProvider;
				if (render != null)
					render.ShowTitle = (_parentFlyoutPage.Detail == VirtualView) && NavigationPage.GetHasNavigationBar(_currentPage);
			}

			if (render != null && render.ShowTitle)
			{
				render.Title = _currentPage.Title;

				if (!Brush.IsNullOrEmpty(VirtualView.BarBackground))
					render.BarBackgroundBrush = GetBarBackgroundBrush(VirtualView);
				else
					render.BarBackgroundBrush = GetBarBackgroundColorBrush();

				render.BarForegroundBrush = GetBarForegroundBrush(VirtualView);
			}

			if (_showTitle || (render != null && render.ShowTitle))
			{
				ToolbarManager.UpdateToolbarItems(VirtualView)
					.FireAndForget((e)=> Log.Warning(nameof(NavigationPage), $"{e}"));
			}
		}

		void UpdatePadding()
		{
			NativeView.TitleInset = VirtualView.Padding.Left;
		}

		void UpdateTitleColor()
		{
			(this as ITitleProvider).BarForegroundBrush = GetBarForegroundBrush(VirtualView);
		}

		void UpdateNavigationBarBackground()
		{
			(this as ITitleProvider).BarBackgroundBrush = GetBarBackgroundBrush(VirtualView);
		}

		void UpdateTitleIcon() =>
			UpdateTitleIconAsync()
				.FireAndForget(errorCallback: (e) => Log.Warning(nameof(TitleIcon), $"{e}"));

		async Task UpdateTitleIconAsync()
		{
			var page = _currentPage;
			if (page == null)
				return;

			ImageSource source = NavigationPage.GetTitleIconImageSource(page);

			TitleIcon = await source.ToWindowsImageSourceAsync();

			if (NativeView == null || _currentPage != page)
				return;

			NativeView.TitleIcon = TitleIcon;

			if (_parentFlyoutPage != null && this is ITitleIconProvider parent)
				parent.TitleIcon = TitleIcon;

			NativeView.UpdateLayout();
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
				if (this is ITitleViewProvider parent)
					parent.TitleView = TitleView;
			}
			else if (_parentFlyoutPage == null)
				NativeView.TitleView = TitleView;

		}

		void UpdateToolbarPlacement()
		{
			if (NativeView == null)
			{
				return;
			}

			NativeView.ToolbarPlacement = VirtualView.OnThisPlatform().GetToolbarPlacement();
		}

		void UpdateToolbarDynamicOverflowEnabled()
		{
			if (NativeView == null)
			{
				return;
			}

			NativeView.ToolbarDynamicOverflowEnabled = VirtualView.OnThisPlatform().GetToolbarDynamicOverflowEnabled();
		}

		public static void MapPadding(NavigationPageHandler handler, NavigationPage view) =>
			handler.UpdatePadding();

		public static void MapTitleColor(NavigationPageHandler handler, NavigationPage view) => handler.UpdateTitleColor();

		public static void MapNavigationBarBackground(NavigationPageHandler handler, NavigationPage view) => handler.UpdateNavigationBarBackground();

		// TODO MAUI: Task Based Mappers?
		public static void MapTitleIcon(NavigationPageHandler handler, NavigationPage view) => handler.UpdateTitleIcon();

		public static void MapTitleView(NavigationPageHandler handler, NavigationPage view) => handler.UpdateTitleView();

		public static void MapToolbarPlacement(NavigationPageHandler handler, NavigationPage view) => handler.UpdateToolbarPlacement();

		public static void MapToolbarDynamicOverflowEnabled(NavigationPageHandler handler, NavigationPage view) => handler.UpdateToolbarDynamicOverflowEnabled();
	}
}
