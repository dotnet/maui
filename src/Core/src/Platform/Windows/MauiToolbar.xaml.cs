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

		public MauiToolbar()
		{
			InitializeComponent();
			titleIcon.Visibility = UI.Xaml.Visibility.Collapsed;
			textBlockBorder.Visibility = UI.Xaml.Visibility.Collapsed;
			menuContent.Visibility = UI.Xaml.Visibility.Collapsed;
			titleView.Visibility = UI.Xaml.Visibility.Collapsed;
		}

		internal static string? Title
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

		internal static WImage? TitleIconImage
		{
			get => titleIcon;
		}

		internal static WImageSource? TitleIconImageSource
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

		internal static UI.Xaml.Thickness TitleViewMargin
		{
			get => titleView.Margin;
			set => titleView.Margin = value;
		}

		internal static object? TitleView
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

			_menuBarForeground = brush;
			UpdateMenuBarForeground();
		}

		internal static CommandBar CommandBar => commandBar;


		internal static UI.Xaml.Thickness ContentGridMargin
		{
			get => contentGrid.Margin;
			set => contentGrid.Margin = value;
		}

		internal static VerticalAlignment TextBlockBorderVerticalAlignment
		{
			get => textBlockBorder.VerticalAlignment;
			set => textBlockBorder.VerticalAlignment = value;
		}

		public static NavigationViewBackButtonVisible IsBackButtonVisible
		{
			get => (NavigationViewBackButtonVisible)GetValue(IsBackButtonVisibleProperty);
			set => SetValue(IsBackButtonVisibleProperty, value);
		}

		public static bool IsBackEnabled
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

			// MenuBarItems currently don't respect the Foreground property due to https://github.com/microsoft/microsoft-ui-xaml/issues/7070
			// Work around this by setting the Button's colors in the MenuBar's ResourceDictionary

			ResourceDictionary dictionary = _menuBar.Resources;
			WBrush? menuForegroundBrush = _menuBarForeground;
			if (menuForegroundBrush is null)
			{
				dictionary.Remove("ButtonForeground");
				dictionary.Remove("ButtonForegroundPointerOver");
				dictionary.Remove("ButtonForegroundPressed");
				dictionary.Remove("ButtonForegroundDisabled");
			}
			else
			{
				dictionary["ButtonForeground"] = menuForegroundBrush;
				dictionary["ButtonForegroundPointerOver"] = menuForegroundBrush;
				dictionary["ButtonForegroundPressed"] = menuForegroundBrush;
				dictionary["ButtonForegroundDisabled"] = menuForegroundBrush;
			}
		}
	}
}
