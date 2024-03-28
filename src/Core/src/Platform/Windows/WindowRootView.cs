using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Win32;
using Windows.Foundation;
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
		FrameworkElement? _appTitleBar;
		bool _hasTitleBarImage;
		ViewManagement.UISettings _viewSettings;
		public event TypedEventHandler<NavigationView, NavigationViewBackRequestedEventArgs>? BackRequested;

		public WindowRootView()
		{
			IsTabStop = false;
			_viewSettings = new ViewManagement.UISettings();
		}

		internal double AppTitleBarActualHeight => AppTitleBarContentControl?.ActualHeight ?? 0;
		internal ContentControl? AppTitleBarContentControl { get; private set; }
		internal FrameworkElement? AppTitleBarContainer { get; private set; }

		Image? AppFontIcon { get; set; }
		internal TextBlock? AppTitle { get; private set; }

		public RootNavigationView? NavigationViewControl { get; private set; }

		internal MauiToolbar? Toolbar
		{
			get => _toolbar;
			set
			{
				if (_toolbar != null)
					_toolbar.SetMenuBar(null);

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

		public static DataTemplate? AppTitleBarTemplate
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

				UpdateAppTitleBarTransparency();

				return _appTitleBar;
			}
		}

		internal void UpdateAppTitleBar(int appTitleBarHeight, bool useCustomAppTitleBar)
		{
			_useCustomAppTitleBar = useCustomAppTitleBar;
			WindowRootView.WindowTitleBarContentControlMinHeight = appTitleBarHeight;

			double topMargin = appTitleBarHeight;
			if (AppTitleBarContentControl != null)
			{
				topMargin = Math.Min(appTitleBarHeight, AppTitleBarContentControl.ActualHeight);
			}

			if (useCustomAppTitleBar)
			{
				WindowRootView.WindowTitleBarContentControlVisibility = UI.Xaml.Visibility.Visible;
			}
			else
			{
				WindowRootView.WindowTitleBarContentControlVisibility = UI.Xaml.Visibility.Collapsed;
			}

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
				if (sender is not FrameworkElement fe)
					return;

				if (_appTitleBarHeight != fe.ActualHeight)
				{
					UpdateRootNavigationViewMargins(fe.ActualHeight);
					this.RefreshThemeResources();
				}
			};
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
			if (AppTitleBar == null)
				return;

			if (AppFontIcon != null)
				return;

			AppFontIcon = (Image?)AppTitleBar?.FindName("AppFontIcon");
			AppTitle = (TextBlock?)AppTitleBar?.FindName("AppTitle");

			if (AppFontIcon != null)
			{
				AppFontIcon.ImageOpened += OnImageOpened;
				AppFontIcon.ImageFailed += OnImageFailed;
			}

			ApplyTitlebarColorPrevalence();
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

			WThickness currMargin = WindowRootView.WindowTitleBarContainerMargin;
			var leftIndent = buttonHolderGrid.ActualWidth;
			WindowRootView.WindowTitleBarContainerMargin = new WThickness(leftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);

			// If the AppIcon loads correctly then we set a margin for the text from the image
			if (_hasTitleBarImage)
			{
				WindowRootView.WindowTitleMargin = new WThickness(12, 0, 0, 0);
				WindowRootView.WindowTitleIconVisibility = UI.Xaml.Visibility.Visible;
			}
			else
			{
				WindowRootView.WindowTitleMargin = new WThickness(0);
				WindowRootView.WindowTitleIconVisibility = UI.Xaml.Visibility.Collapsed;
			}
		}

		void OnButtonHolderGridSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateAppTitleBarMargins();
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

		internal static String? WindowTitle
		{
			get => (String?)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		// Once we switch the TitleBar over to using replaceable IViews we won't need this
		// but for the sake of first just getting us converted over to the new TitleBar
		// APIs
		bool _setTitleBarBackgroundToTransparent = true;
		internal void SetTitleBarBackgroundToTransparent(bool value)
		{
			_setTitleBarBackgroundToTransparent = value;
			if (value)
			{
				UpdateAppTitleBarTransparency();
			}
			else
			{
				_appTitleBar?.RefreshThemeResources();
			}
		}

		void UpdateAppTitleBarTransparency()
		{
			if (_setTitleBarBackgroundToTransparent && _appTitleBar is Border border)
				border.Background = null;
		}

		internal static readonly DependencyProperty WindowTitleForegroundProperty =
			DependencyProperty.Register(
				nameof(WindowTitleForeground),
				typeof(UI.Xaml.Media.Brush),
				typeof(WindowRootView),
				new PropertyMetadata(null));

		internal static UI.Xaml.Media.Brush? WindowTitleForeground
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

		internal static WThickness WindowTitleBarContainerMargin
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

		internal static WThickness WindowTitleMargin
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

		internal static UI.Xaml.Visibility WindowTitleIconVisibility
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

		internal static UI.Xaml.Visibility WindowTitleBarContentControlVisibility
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

		internal static double WindowTitleBarContentControlMinHeight
		{
			get => (double)GetValue(WindowTitleBarContentControlMinHeightProperty);
			set => SetValue(WindowTitleBarContentControlMinHeightProperty, value);
		}
	}
}