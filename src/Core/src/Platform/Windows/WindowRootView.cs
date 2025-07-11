using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Win32;
using Windows.Foundation;
using FRect = Windows.Foundation.Rect;
using Rect32 = Windows.Graphics.RectInt32;
using ViewManagement = Windows.UI.ViewManagement;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Platform
{
	public partial class WindowRootView : ContentControl
	{
		public static readonly DependencyProperty AppTitleBarTemplateProperty
			= DependencyProperty.Register(nameof(AppTitleBarTemplate), typeof(object), typeof(WindowRootView),
				new PropertyMetadata(null, OnAppTitleBarTemplateChanged));

		double _appTitleBarHeight;
		bool _useCustomAppTitleBar;
		internal event EventHandler? OnApplyTemplateFinished;
		internal event EventHandler? OnWindowTitleBarContentSizeChanged;
		internal event EventHandler? ContentChanged;
		MauiToolbar? _toolbar;
		MenuBar? _menuBar;
		ITitleBar? _titleBar;
		FrameworkElement? _appTitleBar;
		bool _hasTitleBarImage;
		ViewManagement.UISettings _viewSettings;
		public event TypedEventHandler<NavigationView, NavigationViewBackRequestedEventArgs>? BackRequested;

		public WindowRootView()
		{
			IsTabStop = false;
			PassthroughTitlebarElements = new List<FrameworkElement>();
			_viewSettings = new ViewManagement.UISettings();
		}

		internal double AppTitleBarActualHeight => AppTitleBarContentControl?.ActualHeight ?? 0;
		internal ContentControl? AppTitleBarContentControl { get; private set; }
		internal FrameworkElement? AppTitleBarContainer { get; private set; }

		Image? AppFontIcon { get; set; }
		internal TextBlock? AppTitle { get; private set; }
		internal WindowId? AppWindowId { get; set; }

		public RootNavigationView? NavigationViewControl { get; private set; }

		internal MauiToolbar? Toolbar
		{
			get => _toolbar;
			set
			{
				_toolbar?.SetMenuBar(null);

				_toolbar = value;
				if (NavigationViewControl != null)
					NavigationViewControl.Toolbar = Toolbar;

				_toolbar?.SetMenuBar(MenuBar);
			}
		}

		internal MenuBar? MenuBar
		{
			get => _menuBar;
			set
			{
				_menuBar = value;
				Toolbar?.SetMenuBar(value);
			}
		}

		internal ITitleBar? TitleBar => _titleBar;

		public DataTemplate? AppTitleBarTemplate
		{
			get => (DataTemplate?)GetValue(AppTitleBarTemplateProperty);
			set => SetValue(AppTitleBarTemplateProperty, value);
		}

		internal FrameworkElement? AppTitleBar
		{
			get
			{
				if (_appTitleBar != null)
					return _appTitleBar;

				if (AppTitleBarContentControl is null)
					return null;

				var cp = AppTitleBarContentControl.GetFirstDescendant<ContentPresenter>();

				if (cp is null)
					return null;

				_appTitleBar = cp.GetFirstDescendant<FrameworkElement>();

				if (_appTitleBar is null)
				{
					_appTitleBar = cp;
				}

				return _appTitleBar;
			}
		}

		internal void UpdateAppTitleBar(int appTitleBarHeight, bool useCustomAppTitleBar, WThickness margin)
		{
			_useCustomAppTitleBar = useCustomAppTitleBar;
			WindowTitleBarContentControlMinHeight = appTitleBarHeight;

			double topMargin = appTitleBarHeight;
			if (AppTitleBarContentControl != null)
			{
				topMargin = Math.Min(appTitleBarHeight, AppTitleBarContentControl.ActualHeight);
			}

			if (useCustomAppTitleBar)
			{
				WindowTitleBarContentControlVisibility = UI.Xaml.Visibility.Visible;
			}
			else
			{
				WindowTitleBarContentControlVisibility = UI.Xaml.Visibility.Collapsed;
			}

			WindowTitleMargin = margin;
			UpdateRootNavigationViewMargins(topMargin);
			this.RefreshThemeResources();
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_viewSettings.ColorValuesChanged += ViewSettingsColorValuesChanged;

			AppTitleBarContainer = (FrameworkElement)GetTemplateChild("AppTitleBarContainer");
			AppTitleBarContentControl = (ContentControl?)GetTemplateChild("AppTitleBarContentControl") ??
				AppTitleBarContainer.GetDescendantByName<ContentControl>("AppTitleBarContentControl");

			if (AppTitleBarContentControl != null)
			{
				LoadAppTitleBarContainer();
			}
			else
			{
				AppTitleBarContainer.Loaded += OnAppTitleBarContainerLoaded;
			}

			OnApplyTemplateFinished?.Invoke(this, EventArgs.Empty);

			UpdateAppTitleBarMargins();
		}

		void OnAppTitleBarContainerLoaded(object sender, RoutedEventArgs e)
		{
			if (AppTitleBarContainer != null)
			{
				AppTitleBarContainer.Loaded -= OnAppTitleBarContainerLoaded;

				AppTitleBarContentControl =
					AppTitleBarContainer.GetDescendantByName<ContentControl>("AppTitleBarContentControl");
			}

			LoadAppTitleBarContainer();
		}

		void LoadAppTitleBarContainer()
		{
			if (AppTitleBarContentControl == null)
				return;

			if (AppTitleBar == null)
				AppTitleBarContentControl.Loaded += OnAppTitleBarContentControlLoaded;
			else
				LoadAppTitleBarControls();

			OnWindowTitleBarContentSizeChanged?.Invoke(AppTitleBarContentControl, EventArgs.Empty);
			AppTitleBarContentControl.SizeChanged += (sender, args) =>
			{
				OnWindowTitleBarContentSizeChanged?.Invoke(sender, EventArgs.Empty);
				UpdateTitleBarContentSize();
			};

			UpdateTitleBarContentSize();
		}

		internal void UpdateTitleBarContentSize()
		{
			if (AppTitleBarContentControl is null)
				return;

			if (_appTitleBarHeight != AppTitleBarContentControl.ActualHeight &&
				AppTitleBarContentControl.Visibility == UI.Xaml.Visibility.Visible)
			{
				UpdateRootNavigationViewMargins(AppTitleBarContentControl.ActualHeight);

				if (AppWindowId.HasValue)
				{
					AppWindow.GetFromWindowId(AppWindowId.Value).TitleBar.PreferredHeightOption =
						_appTitleBarHeight >= 48 ? TitleBarHeightOption.Tall : TitleBarHeightOption.Standard;
				}

				this.RefreshThemeResources();
			}

			var rectArray = new List<Rect32>();
			foreach (var child in PassthroughTitlebarElements)
			{
				var transform = child.TransformToVisual(null);
				var bounds = transform.TransformBounds(
					new FRect(0, 0, child.ActualWidth, child.ActualHeight));
				var rect = GetRect(bounds, XamlRoot.RasterizationScale);
				rectArray.Add(rect);
			}

			if (AppWindowId.HasValue)
			{
				var nonClientInputSrc =
					InputNonClientPointerSource.GetForWindowId(AppWindowId.Value);

				if (rectArray.Count > 0)
				{
					nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, [.. rectArray]);
				}
				else
				{
					nonClientInputSrc.ClearRegionRects(NonClientRegionKind.Passthrough);
				}
			}
		}

		private static Rect32 GetRect(FRect bounds, double scale)
		{
			return new Rect32(
				_X: (int)Math.Round(bounds.X * scale),
				_Y: (int)Math.Round(bounds.Y * scale),
				_Width: (int)Math.Round(bounds.Width * scale),
				_Height: (int)Math.Round(bounds.Height * scale)
			);
		}

		void OnAppTitleBarContentControlLoaded(object sender, RoutedEventArgs e)
		{
			LoadAppTitleBarControls();

			if (AppTitleBarContentControl != null)
				AppTitleBarContentControl.Loaded -= OnAppTitleBarContentControlLoaded;
		}

		void UpdateRootNavigationViewMargins(double margin)
		{
			if (_appTitleBarHeight == margin)
				return;

			_appTitleBarHeight = margin;
			NavigationViewControl?.UpdateAppTitleBar(_appTitleBarHeight);

			var contentMargin = new WThickness(0, _appTitleBarHeight, 0, 0);
			this.SetApplicationResource("NavigationViewContentMargin", contentMargin);
			this.SetApplicationResource("NavigationViewMinimalContentMargin", contentMargin);
			this.SetApplicationResource("NavigationViewBorderThickness", new WThickness(0));
		}

		void LoadAppTitleBarControls()
		{
			if (WindowTitleBarContent is not null && AppTitleBarContentControl is not null)
			{
				AppTitleBarContentControl.ContentTemplateSelector = null;
				AppTitleBarContentControl.Content = WindowTitleBarContent;
			}
			else if (AppTitleBar != null && AppFontIcon is null)
			{
				AppFontIcon = (Image?)AppTitleBar?.FindName("AppFontIcon");
				AppTitle = (TextBlock?)AppTitleBar?.FindName("AppTitle");

				if (AppFontIcon != null)
				{
					AppFontIcon.ImageOpened += OnImageOpened;
					AppFontIcon.ImageFailed += OnImageFailed;
				}

				ApplyTitlebarColorPrevalence();
			}

			UpdateAppTitleBarMargins();
		}

		private void ViewSettingsColorValuesChanged(ViewManagement.UISettings sender, object args)
		{
			ApplyTitlebarColorPrevalence();
		}

		void ApplyTitlebarColorPrevalence()
		{
			try
			{
				// Figure out if the "show accent color on title bars" setting is enabled
				using var dwmSubKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM\");
				var enableAccentColor = dwmSubKey?.GetValue("ColorPrevalence");
				if (enableAccentColor != null &&
					int.TryParse(enableAccentColor.ToString(), out var enableValue) &&
					_appTitleBar is Border border)
				{
					DispatcherQueue.TryEnqueue(() =>
					{
						border.Background = enableValue == 1 ?
							new SolidColorBrush(_viewSettings.GetColorValue(ViewManagement.UIColorType.Accent)) :
							new SolidColorBrush(UI.Colors.Transparent);

						if (NavigationViewControl != null && NavigationViewControl.ButtonHolderGrid != null)
							NavigationViewControl.ButtonHolderGrid.Background = border.Background;
					});
				}
			}
			catch (Exception) { }
		}

		ActionDisposable? _contentChanged;

		protected override void OnContentChanged(object oldContent, object newContent)
		{
			_contentChanged?.Dispose();
			_contentChanged = null;

			base.OnContentChanged(oldContent, newContent);

			if (newContent is RootNavigationView mnv)
			{
				NavigationViewControl = mnv;
				NavigationViewControl.DisplayModeChanged += OnNavigationViewControlDisplayModeChanged;
				NavigationViewControl.BackRequested += OnNavigationViewBackRequested;
				NavigationViewControl.Toolbar = Toolbar;
				NavigationViewControl.OnApplyTemplateFinished += OnNavigationViewControlOnApplyTemplateFinished;
				var backButtonToken = NavigationViewControl.RegisterPropertyChangedCallback(NavigationView.IsBackButtonVisibleProperty, AppBarNavigationIconsChanged);
				var paneToggleToken = NavigationViewControl.RegisterPropertyChangedCallback(NavigationView.IsPaneToggleButtonVisibleProperty, AppBarNavigationIconsChanged);
				var backButtonWidthToken = NavigationViewControl.RegisterPropertyChangedCallback(MauiNavigationView.NavigationBackButtonWidthProperty, AppBarNavigationIconsChanged);

				_contentChanged = new ActionDisposable(() =>
				{
					mnv.DisplayModeChanged -= OnNavigationViewControlDisplayModeChanged;
					mnv.BackRequested -= OnNavigationViewBackRequested;
					mnv.Toolbar = null;
					NavigationViewControl.UnregisterPropertyChangedCallback(NavigationView.IsBackButtonVisibleProperty, backButtonToken);
					NavigationViewControl.UnregisterPropertyChangedCallback(NavigationView.IsPaneToggleButtonVisibleProperty, paneToggleToken);
					NavigationViewControl.UnregisterPropertyChangedCallback(MauiNavigationView.NavigationBackButtonWidthProperty, backButtonWidthToken);
					NavigationViewControl = null;
				});

				if (_appTitleBarHeight > 0)
				{
					NavigationViewControl.UpdateAppTitleBar(_appTitleBarHeight, _useCustomAppTitleBar);
				}
			}

			ContentChanged?.Invoke(this, EventArgs.Empty);
		}

		void OnNavigationViewControlOnApplyTemplateFinished(object? sender, EventArgs e)
		{
			if (NavigationViewControl != null)
			{
				NavigationViewControl.OnApplyTemplateFinished -= OnNavigationViewControlOnApplyTemplateFinished;
				NavigationViewControl.ButtonHolderGrid!.SizeChanged += OnButtonHolderGridSizeChanged;
			}

			ContentChanged?.Invoke(this, EventArgs.Empty);
		}

		void OnNavigationViewBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) =>
			BackRequested?.Invoke(sender, args);

		void OnNavigationViewControlDisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
		{
			UpdateAppTitleBarMargins();
		}

		void AppBarNavigationIconsChanged(DependencyObject sender, DependencyProperty dp)
		{
			UpdateAppTitleBarMargins();
		}

		void OnImageOpened(object sender, RoutedEventArgs e)
		{
			_hasTitleBarImage = true;
			UpdateAppTitleBarMargins();
		}

		void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
		{
			_hasTitleBarImage = false;
			UpdateAppTitleBarMargins();
		}

		void UpdateAppTitleBarMargins()
		{
			if (NavigationViewControl?.ButtonHolderGrid is not Grid buttonHolderGrid)
			{
				return;
			}

			WThickness currMargin = WindowTitleBarContainerMargin;
			var leftIndent = buttonHolderGrid.ActualWidth;
			WindowTitleBarContainerMargin = new WThickness(leftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);

			// If the AppIcon loads correctly then we set a margin for the text from the image
			if (_hasTitleBarImage)
			{
				WindowTitleIconVisibility = UI.Xaml.Visibility.Visible;
			}
			else
			{
				WindowTitleIconVisibility = UI.Xaml.Visibility.Collapsed;
			}
		}

		void OnButtonHolderGridSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateAppTitleBarMargins();
		}

		internal void SetTitleBar(ITitleBar? titlebar, IMauiContext? mauiContext)
		{
			if (WindowTitleBarContent is not null)
			{
				WindowTitleBarContent.LayoutUpdated -= PlatformView_LayoutUpdated;
			}

			if (_titleBar is INotifyPropertyChanged p)
			{
				p.PropertyChanged -= TitlebarPropChanged_PropertyChanged;
			}

			_titleBar = titlebar;

			if (_titleBar is null || mauiContext is null)
			{
				UpdateBackgroundColorForButtons();
				if (AppTitleBarContentControl is not null)
				{
					AppTitleBarContentControl.Content = null;
					UpdateAppTitleBarTemplate();
				}
				return;
			}

			var handler = _titleBar?.ToHandler(mauiContext);
			if (handler is not null &&
				handler.PlatformView is not null)
			{
				WindowTitleBarContent = handler.PlatformView;

				// This will handle all size changed events when leading/trailing/main content
				// changes size or is added
				WindowTitleBarContent.LayoutUpdated += PlatformView_LayoutUpdated;

				// Override the template selector and content
				if (AppTitleBarContentControl is not null)
				{
					AppTitleBarContentControl.ContentTemplateSelector = null;
					AppTitleBarContentControl.Content = WindowTitleBarContent;
				}

				// To handle when leading/trailing/main content is added/removed
				if (_titleBar is INotifyPropertyChanged tpc)
				{
					tpc.PropertyChanged += TitlebarPropChanged_PropertyChanged;
				}

				UpdateBackgroundColorForButtons();
				SetTitleBarInputElements();
			}
		}

		internal void SetTitleBarVisibility(UI.Xaml.Visibility visibility)
		{
			// Set default and custom titlebar container visibility
			if (AppTitleBarContainer is not null)
			{
				AppTitleBarContainer.Visibility = visibility;
			}

			// Set the back/flyout button container visibility
			if (NavigationViewControl is not null &&
				NavigationViewControl.ButtonHolderGrid is not null)
			{
				NavigationViewControl.ButtonHolderGrid.Visibility = visibility;
			}
		}

		private void TitlebarPropChanged_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (_titleBar is not null && _titleBar.Handler?.MauiContext is not null)
			{
				SetTitleBarInputElements();

				if (e.PropertyName == "BackgroundColor")
				{
					UpdateBackgroundColorForButtons();
				}
			}
		}

		private void UpdateBackgroundColorForButtons()
		{
			if (NavigationViewControl?.ButtonHolderGrid is not null)
			{
				if (_titleBar?.Background is SolidPaint bg)
				{
					NavigationViewControl.ButtonHolderGrid.Background = new SolidColorBrush(bg.Color.ToWindowsColor());
				}
				else
				{
					NavigationViewControl.ButtonHolderGrid.Background = new SolidColorBrush(UI.Colors.Transparent);
				}
			}
		}

		private void PlatformView_LayoutUpdated(object? sender, object e)
		{
			UpdateTitleBarContentSize();
		}

		private void SetTitleBarInputElements()
		{
			var mauiContext = _titleBar?.Handler?.MauiContext;
			if (mauiContext is null || _titleBar is null)
			{
				return;
			}

			var passthroughElements = new List<FrameworkElement>();
			foreach (var element in _titleBar.PassthroughElements)
			{
				if (element is IContentView container && container.PresentedContent is not null)
				{
					var platformView = container.PresentedContent.ToHandler(mauiContext).PlatformView;
					if (platformView is not null)
					{
						passthroughElements.Add(platformView);
					}
				}
				else
				{
					var platformView = element.ToHandler(mauiContext).PlatformView;
					if (platformView is not null)
					{
						passthroughElements.Add(platformView);
					}
				}
			}
			PassthroughTitlebarElements = passthroughElements;
		}

		void UpdateAppTitleBarTemplate()
		{
			// Ensure the default Window Title template is reapplied when switching from a TitleBar.
			// The ContentTemplateSelector is reset to the default when ContentTemplate is null, restoring proper title display.
			if (AppTitleBarContentControl is not null && AppTitleBarContentControl.ContentTemplateSelector is null)
			{
				AppTitleBarContentControl.ContentTemplateSelector =
				(DataTemplateSelector)Application.Current.Resources["MauiAppTitleBarTemplateSelector"];
			}
		}

		static void OnAppTitleBarTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((WindowRootView)d)._appTitleBar = null;
		}

		internal static readonly DependencyProperty TitleProperty =
			DependencyProperty.Register(
				nameof(WindowTitle),
				typeof(String),
				typeof(WindowRootView),
				new PropertyMetadata(null));

		internal String? WindowTitle
		{
			get => (String?)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		internal static readonly DependencyProperty WindowTitleBarContentProperty =
			DependencyProperty.Register(
				nameof(WindowTitleBarContent),
				typeof(FrameworkElement),
				typeof(WindowRootView),
				new PropertyMetadata(null));

		internal FrameworkElement WindowTitleBarContent
		{
			get => (FrameworkElement)GetValue(WindowTitleBarContentProperty);
			set => SetValue(WindowTitleBarContentProperty, value);
		}

		internal static readonly DependencyProperty WindowTitleForegroundProperty =
			DependencyProperty.Register(
				nameof(WindowTitleForeground),
				typeof(UI.Xaml.Media.Brush),
				typeof(WindowRootView),
				new PropertyMetadata(null));

		internal UI.Xaml.Media.Brush? WindowTitleForeground
		{
			get => (UI.Xaml.Media.Brush?)GetValue(WindowTitleForegroundProperty);
			set => SetValue(WindowTitleForegroundProperty, value);
		}

		internal static readonly DependencyProperty WindowTitleBarContainerMarginProperty =
			DependencyProperty.Register(
				nameof(WindowTitleBarContainerMargin),
				typeof(WThickness),
				typeof(WindowRootView),
				new PropertyMetadata(new WThickness(0)));

		internal WThickness WindowTitleBarContainerMargin
		{
			get => (WThickness)GetValue(WindowTitleBarContainerMarginProperty);
			set => SetValue(WindowTitleBarContainerMarginProperty, value);
		}

		internal static readonly DependencyProperty WindowTitleMarginProperty =
			DependencyProperty.Register(
				nameof(WindowTitleMargin),
				typeof(WThickness),
				typeof(WindowRootView),
				new PropertyMetadata(new WThickness(0)));

		internal WThickness WindowTitleMargin
		{
			get => (WThickness)GetValue(WindowTitleMarginProperty);
			set => SetValue(WindowTitleMarginProperty, value);
		}

		internal static readonly DependencyProperty WindowTitleIconVisibilityProperty =
			DependencyProperty.Register(
				nameof(WindowTitleIconVisibility),
				typeof(UI.Xaml.Visibility),
				typeof(WindowRootView),
				new PropertyMetadata(UI.Xaml.Visibility.Collapsed));

		internal UI.Xaml.Visibility WindowTitleIconVisibility
		{
			get => (UI.Xaml.Visibility)GetValue(WindowTitleIconVisibilityProperty);
			set => SetValue(WindowTitleIconVisibilityProperty, value);
		}

		internal static readonly DependencyProperty WindowTitleBarContentControlVisibilityProperty =
			DependencyProperty.Register(
				nameof(WindowTitleBarContentControlVisibility),
				typeof(UI.Xaml.Visibility),
				typeof(WindowRootView),
				new PropertyMetadata(UI.Xaml.Visibility.Visible));

		internal UI.Xaml.Visibility WindowTitleBarContentControlVisibility
		{
			get => (UI.Xaml.Visibility)GetValue(WindowTitleBarContentControlVisibilityProperty);
			set => SetValue(WindowTitleBarContentControlVisibilityProperty, value);
		}

		internal static readonly DependencyProperty WindowTitleBarContentControlMinHeightProperty =
			DependencyProperty.Register(
				nameof(WindowTitleBarContentControlMinHeight),
				typeof(double),
				typeof(WindowRootView),
				new PropertyMetadata(0d));

		internal double WindowTitleBarContentControlMinHeight
		{
			get => (double)GetValue(WindowTitleBarContentControlMinHeightProperty);
			set => SetValue(WindowTitleBarContentControlMinHeightProperty, value);
		}

		internal IEnumerable<FrameworkElement> PassthroughTitlebarElements { get; set; }
	}
}