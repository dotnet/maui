using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WImage = Microsoft.UI.Xaml.Controls.Image;

namespace Microsoft.Maui.Platform
{
	public partial class MauiToolbar
	{
		public static readonly DependencyProperty IsBackButtonVisibleProperty
			= DependencyProperty.Register(nameof(IsBackButtonVisible), typeof(NavigationViewBackButtonVisible), typeof(MauiToolbar),
				new PropertyMetadata(NavigationViewBackButtonVisible.Collapsed, OnIsBackButtonVisiblePropertyChanged));

		MenuBar? _menuBar;
		public MauiToolbar()
		{
			InitializeComponent();
			titleIcon.Visibility = UI.Xaml.Visibility.Collapsed;
			textBlockBorder.Visibility = UI.Xaml.Visibility.Collapsed;
			menuContent.Visibility = UI.Xaml.Visibility.Collapsed;
			titleView.Visibility = UI.Xaml.Visibility.Collapsed;
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

		internal WBrush? TitleColor
		{
			get => title.Foreground;
			set => title.Foreground = value;
		}

		internal CommandBar CommandBar => commandBar;


		internal UI.Xaml.Thickness ContentGridMargin
		{
			get => contentGrid.Margin;
			set => contentGrid.Margin = value;
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
		static void OnIsBackButtonVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}

		internal void SetMenuBar(MenuBar? menuBar)
		{
			_menuBar = menuBar;
			UpdateMenuBar();
		}

		void UpdateMenuBar()
		{
			if (menuContent == null)
				return;

			menuContent.Content = _menuBar;

			if (_menuBar == null || _menuBar.Items.Count == 0)
				menuContent.Visibility = UI.Xaml.Visibility.Collapsed;
			else
				menuContent.Visibility = UI.Xaml.Visibility.Visible;
		}
	}
}
