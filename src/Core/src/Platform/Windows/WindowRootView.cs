using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Platform
{
	public partial class WindowRootView : ContentControl
	{
		public static readonly DependencyProperty AppTitleBarTemplateProperty
			= DependencyProperty.Register(nameof(AppTitleBarTemplate), typeof(object), typeof(WindowRootView),
				new PropertyMetadata(null, OnAppTitleBarTemplateChanged));

		double _appTitleBarHeight;
		internal event EventHandler? OnAppTitleBarChanged;
		internal event EventHandler? OnApplyTemplateFinished;
		internal event EventHandler? ContentChanged;
		string? _windowTitle;
		FrameworkElement? _appTitleBar;
		bool _hasTitleBarImage = false;
		public event TypedEventHandler<NavigationView, NavigationViewBackRequestedEventArgs>? BackRequested;

		public WindowRootView()
		{
		}

		internal ContentControl? AppTitleBarContentControl { get; private set; }
		internal FrameworkElement? AppTitleBarContainer { get; private set; }

		Image? AppFontIcon { get; set; }
		internal TextBlock? AppTitle { get; private set; }

		public RootNavigationView? NavigationViewControl { get; private set; }

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

				if (AppTitleBarContentControl == null)
					return null;

				var cp = AppTitleBarContentControl.GetFirstDescendant<ContentPresenter>();

				if (cp == null)
					return null;

				_appTitleBar = cp.GetFirstDescendant<FrameworkElement>();

				if (_appTitleBar != null)
				{
					OnAppTitleBarChanged?.Invoke(this, EventArgs.Empty);
				}
				else
				{
					_appTitleBar = cp;
					OnAppTitleBarChanged?.Invoke(this, EventArgs.Empty);
				}

				return _appTitleBar;
			}
		}

		internal void UpdateAppTitleBar(Graphics.Rect captionButtonRect, bool useCustomAppTitleBar)
		{
			if (AppTitleBarContentControl != null)
			{
				AppTitleBarContentControl.MinHeight = captionButtonRect.Height;

				if (AppTitleBarContentControl.ActualHeight <= 0)
					_appTitleBarHeight = captionButtonRect.Height;
				else
					_appTitleBarHeight = AppTitleBarContentControl.ActualHeight;

				if (useCustomAppTitleBar)
				{
					AppTitleBarContentControl.Visibility = UI.Xaml.Visibility.Visible;
				}
				else
				{
					AppTitleBarContentControl.Visibility = UI.Xaml.Visibility.Collapsed;
				}
			}
			else
			{
				_appTitleBarHeight = captionButtonRect.Height;
			}

			NavigationViewControl?.UpdateAppTitleBar(_appTitleBarHeight, useCustomAppTitleBar);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

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
			SetWindowTitle(_windowTitle);

			void OnAppTitleBarContainerLoaded(object sender, RoutedEventArgs e)
			{
				AppTitleBarContainer.Loaded -= OnAppTitleBarContainerLoaded;

				AppTitleBarContentControl =
					AppTitleBarContainer.GetDescendantByName<ContentControl>("AppTitleBarContentControl");

				LoadAppTitleBarContainer();
			}
		}

		void LoadAppTitleBarContainer()
		{
			if (AppTitleBarContentControl == null)
				return;

			if (AppTitleBar == null)
				AppTitleBarContentControl.Loaded += OnAppTitleBarContentControlLoaded;
			else
				LoadAppTitleBarControls();

			AppTitleBarContentControl.SizeChanged += (_, __) =>
			{
				if (_appTitleBarHeight != AppTitleBarContentControl.ActualHeight)
				{
					_appTitleBarHeight = AppTitleBarContentControl.ActualHeight;
					NavigationViewControl?.UpdateAppTitleBar(_appTitleBarHeight);
				}
			};

			void OnAppTitleBarContentControlLoaded(object sender, RoutedEventArgs e)
			{
				LoadAppTitleBarControls();
				AppTitleBarContentControl.Loaded -= OnAppTitleBarContentControlLoaded;
			}
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

			SetWindowTitle(_windowTitle);
			UpdateAppTitleBarMargins();
		}

		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);
			if (newContent is RootNavigationView mnv)
			{
				NavigationViewControl = mnv;
				NavigationViewControl.DisplayModeChanged += OnNavigationViewControlDisplayModeChanged;
				NavigationViewControl.BackRequested += OnNavigationViewBackRequested;
				NavigationViewControl.OnApplyTemplateFinished += OnNavigationViewControlOnApplyTemplateFinished;
				NavigationViewControl.RegisterPropertyChangedCallback(NavigationView.IsBackButtonVisibleProperty, AppBarNavigationIconsChanged);
				NavigationViewControl.RegisterPropertyChangedCallback(NavigationView.IsPaneToggleButtonVisibleProperty, AppBarNavigationIconsChanged);
				NavigationViewControl.RegisterPropertyChangedCallback(MauiNavigationView.NavigationBackButtonWidthProperty, AppBarNavigationIconsChanged);

				ContentChanged?.Invoke(this, EventArgs.Empty);

				if (_appTitleBarHeight > 0)
				{
					NavigationViewControl.UpdateAppTitleBar(_appTitleBarHeight);
				}

				var thing = NavigationViewControl.ButtonHolderGrid;
			}
		}

		void OnNavigationViewControlOnApplyTemplateFinished(object? sender, EventArgs e)
		{
			if (NavigationViewControl != null)
			{
				NavigationViewControl.OnApplyTemplateFinished -= OnNavigationViewControlOnApplyTemplateFinished;
				NavigationViewControl.ButtonHolderGrid!.SizeChanged += OnButtonHolderGridSizeChanged;
			}
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

		internal void SetWindowTitle(string? title)
		{
			_windowTitle = title;
			if (AppTitle != null)
				AppTitle.Text = title;
		}

		void UpdateAppTitleBarMargins()
		{
			if (NavigationViewControl?.ButtonHolderGrid == null)
			{
				return;
			}

			if (AppTitleBarContentControl == null || AppTitleBarContainer == null)
				return;

			WThickness currMargin = AppTitleBarContainer.Margin;
			var leftIndent = NavigationViewControl.ButtonHolderGrid.ActualWidth;
			AppTitleBarContainer.Margin = new WThickness(leftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);

			// If the AppIcon loads correctly then we set a margin for the text from the image
			if (_hasTitleBarImage)
			{
				if (AppTitle != null)
					AppTitle.Margin = new WThickness(12, 0, 0, 0);

				if (AppFontIcon != null)
					AppFontIcon.Visibility = UI.Xaml.Visibility.Visible;
			}
			else
			{
				// If there is no AppIcon then we hide the image and the layout already
				// has a margin set

				if (AppTitle != null)
					AppTitle.Margin = new WThickness(0);

				if (AppFontIcon != null)
					AppFontIcon.Visibility = UI.Xaml.Visibility.Collapsed;
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
	}
}