#nullable enable

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
		Page? _currentPage;
		Page? _previousPage;

		FlyoutPage? _parentFlyoutPage;
		TabbedPage? _parentTabbedPage;
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
				NativeViewValidation().ToolbarBackground = value;
				UpdateTitleOnParents();
			}
		}

		WBrush ITitleProvider.BarForegroundBrush
		{
			set
			{
				NativeViewValidation().TitleBrush = value;
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
			get { return _currentPage?.Title ?? String.Empty; }

			set { /*Not implemented but required by interface*/ }
		}

		public WImageSource TitleIcon { get; set; }

		public View? TitleView
		{
			get
			{
				if (_currentPage == null)
					return null;

				return NavigationPage.GetTitleView(_currentPage) as View;
			}
			set { /*Not implemented but required by interface*/ }
		}

		Task<CommandBar?> IToolbarProvider.GetCommandBarAsync()
		{
			return ((IToolbarProvider)NativeViewValidation())?.GetCommandBarAsync() ??
				Task<CommandBar?>.FromResult((CommandBar?)null);
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (VirtualView == null)
				return Size.Zero;

			var constraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);
			IViewHandler childRenderer = VirtualView.CurrentPage.Handler;
			FrameworkElement? child = childRenderer.NativeView as FrameworkElement;
			if (child == null)
				return Size.Zero;

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

			var virtualView = VirtualViewWithValidation();

			nativeView.PointerPressed += OnPointerPressed;
			nativeView.SizeChanged += OnNativeSizeChanged;
			Tracker = new BackgroundTracker<PageControl>(Control.BackgroundProperty) 
			{
				Element = virtualView, 
				Container = NativeView 
			};

			SetPage(virtualView.CurrentPage, false, false);

			nativeView.Loaded += OnLoaded;
			nativeView.Unloaded += OnUnloaded;

			nativeView.DataContext = virtualView.CurrentPage;

			// Move this somewhere else
			LookupRelevantParents();

			// Enforce consistency rules on toolbar (show toolbar if top-level page is Navigation Page)
			nativeView.ShouldShowToolbar = _parentFlyoutPage == null && _parentTabbedPage == null;
			if (_parentTabbedPage != null)
				virtualView.Appearing += OnElementAppearing;

			virtualView.PushRequested += OnPushRequested;
			virtualView.PopRequested += OnPopRequested;
			virtualView.PopToRootRequested += OnPopToRootRequested;
			virtualView.InternalChildren.CollectionChanged += OnChildrenChanged;

			if (!string.IsNullOrEmpty(virtualView.AutomationId))
				nativeView.SetValue(Microsoft.UI.Xaml.Automation.AutomationProperties.AutomationIdProperty, virtualView.AutomationId);

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
			nativeView.PointerPressed -= OnPointerPressed;
			nativeView.SizeChanged -= OnNativeSizeChanged;
			nativeView.Loaded -= OnLoaded;
			nativeView.Unloaded -= OnUnloaded;

			VirtualView.SendDisappearing();

			nativeView.PointerPressed -= OnPointerPressed;
			nativeView.SizeChanged -= OnNativeSizeChanged;
			nativeView.Loaded -= OnLoaded;
			nativeView.Unloaded -= OnUnloaded;

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

			if (VirtualViewWithValidation().BarBackgroundColor.IsDefault() && defaultColor != null)
				return (WBrush)defaultColor;

			return Maui.ColorExtensions.ToNative(VirtualViewWithValidation().BarBackgroundColor);
		}

		static WBrush? GetBarBackgroundBrush(NavigationPage navigationPage)
		{
			var barBackground = navigationPage.BarBackground;
			object defaultColor = GetDefaultColor();

			if (!Brush.IsNullOrEmpty(barBackground))
				return barBackground.ToBrush();

			if (navigationPage.BarBackgroundColor != null)
				return navigationPage.BarBackgroundColor.ToNative();

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
			IEnumerable<IPage> parentPages = VirtualViewWithValidation().GetParentPages();

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

		void MultiPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
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

		void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateBackButton();
		}

		// TODO MAUI: hmmmmmm can we make this not be property changed based?
		void OnCurrentPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
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

		void OnElementAppearing(object? sender, EventArgs e)
		{
			UpdateTitleVisible();
			UpdateBackButton();
		}

		void OnLoaded(object? sender, RoutedEventArgs args)
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

		void OnNativeSizeChanged(object? sender, SizeChangedEventArgs e)
		{
			UpdateContainerArea();
		}

		void OnPointerPressed(object? sender, PointerRoutedEventArgs e)
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
				OnBackClicked(NativeViewValidation(), e);
			}
		}

		protected virtual void OnPopRequested(object? sender, NavigationRequestedEventArgs e)
		{
			var newCurrent = VirtualViewWithValidation().Peek(1);
			SetPage(newCurrent, e.Animated, true);
		}

		protected virtual void OnPopToRootRequested(object? sender, NavigationRequestedEventArgs e)
		{
			SetPage(e.Page, e.Animated, true);
		}

		protected virtual void OnPushRequested(object? sender, NavigationRequestedEventArgs e)
		{
			SetPage(e.Page, e.Animated, false);
		}

		void OnUnloaded(object? sender, RoutedEventArgs args)
		{
			VirtualView?.SendDisappearing();
		}

		void PushExistingNavigationStack()
		{
			foreach (var page in VirtualViewWithValidation().Pages)
			{
				SetPage(page, false, false);
			}
		}

		void SetPage(Page? page, bool isAnimated, bool isPopping)
		{
			if (_currentPage != null)
			{
				if (isPopping)
				{
					_currentPage.Cleanup();
					NativeViewValidation().TitleView?.Cleanup();
				}

				NativeViewValidation().Content = null;
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

			NativeViewValidation().Content = renderer.NativeView;
			NativeViewValidation().DataContext = page;
		}

		protected virtual void SetupPageTransition(Transition transition, bool isAnimated, bool isPopping)
		{
			PageControl nativeView = NativeViewValidation();
			if (isAnimated && transition == null)
			{
				transition  = new EntranceThemeTransition();
				_transition = (EntranceThemeTransition)transition;
				nativeView.ContentTransitions = new TransitionCollection();
			}

			if (!isAnimated && nativeView.ContentTransitions?.Count > 0)
			{
				nativeView.ContentTransitions.Clear();
			}
			else if (isAnimated &&
				nativeView.ContentTransitions != null &&
				nativeView.ContentTransitions.Contains(transition) == false)
			{
				nativeView.ContentTransitions.Clear();
				nativeView.ContentTransitions.Add(transition);
			}
		}

		void UpdateBackButtonTitle()
		{
			string title;
			if (_previousPage != null)
				title = NavigationPage.GetBackButtonTitle(_previousPage);
			else
				title = String.Empty;

			NativeViewValidation().BackButtonTitle = title;
		}

		void UpdateContainerArea()
		{
			VirtualViewWithValidation().ContainerArea = new Rectangle(0, 0, NativeViewValidation().ContentWidth, NativeViewValidation().ContentHeight);
		}

		void UpdateTitleVisible()
		{
			UpdateTitleOnParents();

			bool showing = NativeViewValidation().TitleVisibility == Visibility.Visible;
			bool newValue = GetIsNavBarPossible() && NavigationPage.GetHasNavigationBar(_currentPage);
			if (showing == newValue)
				return;

			NativeViewValidation().TitleVisibility = newValue ? Visibility.Visible : Visibility.Collapsed;

			// Force ContentHeight/Width to update, doesn't work from inside PageControl for some reason
			NativeViewValidation().UpdateLayout();
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

			bool showBackButton = VirtualViewWithValidation().InternalChildren.Count > 1 && NavigationPage.GetHasBackButton(_currentPage);
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

			NativeView.SetBackButtonTitle(VirtualViewWithValidation());
		}

		void UpdateTitleOnParents()
		{
			if (VirtualView == null || _currentPage == null)
				return;

			ITitleProvider? render = null;
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
			NativeViewValidation().TitleInset = VirtualViewWithValidation().Padding.Left;
		}

		void UpdateTitleColor()
		{
			(this as ITitleProvider).BarForegroundBrush = GetBarForegroundBrush(VirtualViewWithValidation());
		}

		void UpdateNavigationBarBackground()
		{
			(this as ITitleProvider).BarBackgroundBrush = GetBarBackgroundBrush(VirtualViewWithValidation());
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
				NativeViewValidation().TitleView = TitleView;

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

		public static void MapBarTextColor(NavigationPageHandler handler, NavigationPage view) => handler.UpdateTitleColor();

		public static void MapBarBackground(NavigationPageHandler handler, NavigationPage view) => handler.UpdateNavigationBarBackground();

		// TODO MAUI: Task Based Mappers?
		public static void MapTitleIcon(NavigationPageHandler handler, NavigationPage view) => handler.UpdateTitleIcon();

		public static void MapTitleView(NavigationPageHandler handler, NavigationPage view) => handler.UpdateTitleView();

		public static void MapToolbarPlacement(NavigationPageHandler handler, NavigationPage view) => handler.UpdateToolbarPlacement();

		public static void MapToolbarDynamicOverflowEnabled(NavigationPageHandler handler, NavigationPage view) => handler.UpdateToolbarDynamicOverflowEnabled();
	}
}
