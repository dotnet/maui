using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui.Platform
{
	public partial class MauiToolbar
	{
		public static readonly DependencyProperty IsBackButtonVisibleProperty
			= DependencyProperty.Register(nameof(IsBackButtonVisible), typeof(NavigationViewBackButtonVisible), typeof(MauiToolbar),
				new PropertyMetadata(NavigationViewBackButtonVisible.Collapsed));

		public static readonly DependencyProperty IsBackEnabledProperty
			= DependencyProperty.Register(nameof(IsBackEnabled), typeof(bool), typeof(MauiToolbar),
				new PropertyMetadata(true));

		MenuBar? _menuBar;
		WBrush? _menuBarForeground;
		private Button? _navigationViewBackButton;
		private Button? _togglePaneButton;
		private Graphics.Color? _iconColor;

		// CommandBar template parts — used to compute the content grid's available width.
		ItemsControl? _commandBarPrimaryItemsControl;
		Button? _commandBarMoreButton;

		public MauiToolbar()
		{
			InitializeComponent();
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// Clean up subscriptions from any previous template application.
			SizeChanged -= OnToolbarSizeChanged;
			if (_commandBarPrimaryItemsControl != null)
				_commandBarPrimaryItemsControl.SizeChanged -= OnCommandAreaSizeChanged;
			if (_commandBarMoreButton != null)
				_commandBarMoreButton.SizeChanged -= OnCommandAreaSizeChanged;

			_commandBarPrimaryItemsControl = GetTemplateChild("PrimaryItemsControl") as ItemsControl;
			_commandBarMoreButton = GetTemplateChild("MoreButton") as Button;

			SizeChanged += OnToolbarSizeChanged;
			if (_commandBarPrimaryItemsControl != null)
				_commandBarPrimaryItemsControl.SizeChanged += OnCommandAreaSizeChanged;
			if (_commandBarMoreButton != null)
				_commandBarMoreButton.SizeChanged += OnCommandAreaSizeChanged;
		}

		void OnToolbarSizeChanged(object sender, SizeChangedEventArgs e) => UpdateContentGridWidth();
		void OnCommandAreaSizeChanged(object sender, SizeChangedEventArgs e) => UpdateContentGridWidth();

		// WinUI no longer implicitly stretches CommandBar.Content to the available width after
		// LayoutPanel gained MauiLayoutAutomationPeer (IsControlElementCore = true). The CommandBar
		// template now measures the content panel to its own desired size instead of the space left
		// over after PrimaryCommands and the MoreButton. This method restores the intended stretch
		// behaviour by explicitly computing and setting contentGrid.Width.
		void UpdateContentGridWidth()
		{
			if (ActualWidth == 0)
				return;

			var primaryWidth = _commandBarPrimaryItemsControl?.ActualWidth ?? 0;
			var moreWidth = _commandBarMoreButton?.ActualWidth ?? 0;
			var available = System.Math.Max(0d,
			 ActualWidth - primaryWidth - moreWidth
			 - contentGrid.Margin.Left - contentGrid.Margin.Right);

			contentGrid.Width = available;
		}

		internal string? Title
		{
			get => title.Text;
			set
			{
				title.Text = value;

				if (!string.IsNullOrWhiteSpace(value))
					textBlockBorder.Visibility = UI.Xaml.Visibility.Visible;
				else
					textBlockBorder.Visibility = UI.Xaml.Visibility.Collapsed;
			}
		}

		internal WImage? TitleIconImage
		{
			get => titleIcon;
		}

		internal WImageSource? TitleIconImageSource
		{
			get => titleIcon.Source;
			set
			{
				titleIcon.Source = value;

				if (value != null)
					titleIcon.Visibility = UI.Xaml.Visibility.Visible;
				else
					titleIcon.Visibility = UI.Xaml.Visibility.Collapsed;
			}
		}

		internal UI.Xaml.Thickness TitleViewMargin
		{
			get => titleView.Margin;
			set => titleView.Margin = value;
		}

		internal object? TitleView
		{
			get => titleView.Content;
			set
			{
				titleView.Content = value;

				if (value != null)
					titleView.Visibility = UI.Xaml.Visibility.Visible;
				else
					titleView.Visibility = UI.Xaml.Visibility.Collapsed;
			}
		}

		internal void SetBarTextColor(WBrush? brush)
		{
			if (brush != null)
			{
				title.Foreground = brush;
			}
			else
			{
				title.ClearValue(CommandBar.ForegroundProperty);
			}

			_menuBarForeground = brush;
			UpdateMenuBarForeground();
		}

		internal void SetBarBackground(WBrush? brush)
		{
			this.Background = brush;

			// Set CommandBarBackgroundOpen to the same color as the background.
			// This is necessary because CommandBarBackgroundOpen defines the background color of the CommandBar when it is open.
			commandBar.Resources["CommandBarBackgroundOpen"] = brush;
		}

		internal CommandBar CommandBar => commandBar;


		internal UI.Xaml.Thickness ContentGridMargin
		{
			get => contentGrid.Margin;
			set
			{
				contentGrid.Margin = value;
				// Re-apply the explicit width since the margin contributes to the used space.
				UpdateContentGridWidth();
			}
		}

		internal VerticalAlignment TextBlockBorderVerticalAlignment
		{
			get => textBlockBorder.VerticalAlignment;
			set => textBlockBorder.VerticalAlignment = value;
		}

		public NavigationViewBackButtonVisible IsBackButtonVisible
		{
			get => (NavigationViewBackButtonVisible)GetValue(IsBackButtonVisibleProperty);
			set => SetValue(IsBackButtonVisibleProperty, value);
		}

		public bool IsBackEnabled
		{
			get => (bool)GetValue(IsBackEnabledProperty);
			set => SetValue(IsBackEnabledProperty, value);
		}
		internal Button? NavigationViewBackButton
		{
			get => _navigationViewBackButton;
			set
			{
				_navigationViewBackButton = value;
				UpdateIconColor();
			}
		}

		internal Button? TogglePaneButton
		{
			get => _togglePaneButton;
			set
			{
				_togglePaneButton = value;
				UpdateIconColor();
			}
		}

		internal Graphics.Color? IconColor
		{
			get => _iconColor;
			set
			{
				_iconColor = value;
				UpdateIconColor();
			}
		}

		void UpdateIconColor()
		{
			if (IconColor != null)
			{
				TogglePaneButton?.SetApplicationResource("NavigationViewButtonForegroundPointerOver", IconColor.ToPlatform());
				TogglePaneButton?.SetApplicationResource("NavigationViewButtonForegroundDisabled", IconColor.ToPlatform());
				TogglePaneButton?.SetApplicationResource("NavigationViewButtonForegroundPressed", IconColor.ToPlatform());

				NavigationViewBackButton?.SetApplicationResource("NavigationViewButtonForegroundPointerOver", IconColor.ToPlatform());
				NavigationViewBackButton?.SetApplicationResource("NavigationViewButtonForegroundDisabled", IconColor.ToPlatform());
				NavigationViewBackButton?.SetApplicationResource("NavigationViewButtonForegroundPressed", IconColor.ToPlatform());

				NavigationViewBackButton?.UpdateForegroundColor(IconColor);
				TogglePaneButton?.UpdateForegroundColor(IconColor);

			}
			else
			{
				TogglePaneButton?.SetApplicationResource("NavigationViewButtonForegroundPointerOver", null);
				TogglePaneButton?.SetApplicationResource("NavigationViewButtonForegroundDisabled", null);
				TogglePaneButton?.SetApplicationResource("NavigationViewButtonForegroundPressed", null);

				NavigationViewBackButton?.SetApplicationResource("NavigationViewButtonForegroundPointerOver", null);
				NavigationViewBackButton?.SetApplicationResource("NavigationViewButtonForegroundDisabled", null);
				NavigationViewBackButton?.SetApplicationResource("NavigationViewButtonForegroundPressed", null);

				NavigationViewBackButton?.ClearValue(Button.ForegroundProperty);
				TogglePaneButton?.ClearValue(Button.ForegroundProperty);
			}
		}

		internal bool HasMenuBarContent => _menuBar is not null && _menuBar.Items.Count > 0;

		internal void SetMenuBar(MenuBar? menuBar)
		{
			_menuBar = menuBar;

			menuContent.Content = _menuBar;
			UpdateMenuBarForeground();

			if (_menuBar == null || _menuBar.Items.Count == 0)
				menuContent.Visibility = UI.Xaml.Visibility.Collapsed;
			else
				menuContent.Visibility = UI.Xaml.Visibility.Visible;
		}

		void UpdateMenuBarForeground()
		{
			if (_menuBar is null)
				return;

			ResourceDictionary dictionary = _menuBar.Resources;
			WBrush? menuForegroundBrush = _menuBarForeground;
			if (menuForegroundBrush is null)
			{
				dictionary.Remove("MenuBarItemForeground");
				dictionary.Remove("ButtonForegroundPointerOver");
				dictionary.Remove("ButtonForegroundPressed");
				dictionary.Remove("ButtonForegroundDisabled");
			}
			else
			{
				dictionary["MenuBarItemForeground"] = menuForegroundBrush;
				dictionary["ButtonForegroundPointerOver"] = menuForegroundBrush;
				dictionary["ButtonForegroundPressed"] = menuForegroundBrush;
				dictionary["ButtonForegroundDisabled"] = menuForegroundBrush;
			}
		}
	}
}
