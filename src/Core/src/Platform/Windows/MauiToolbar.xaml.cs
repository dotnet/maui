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
		}

		internal string? Title
		{
			get => title.Text;
			set => title.Text = value;
		}

		internal WImage? TitleIconImage
		{
			get => titleIcon;
		}

		internal WImageSource? TitleIconImageSource
		{
			get => titleIcon.Source;
			set => titleIcon.Source = value;
		}

		internal object? TitleView
		{
			get => titleView.Content;
			set => titleView.Content = value;
		}

		internal WBrush? TitleColor
		{
			get => title.Foreground;
			set => title.Foreground = value;
		}

		internal CommandBar CommandBar => commandBar;

		internal WGrid ContentGrid => contentGrid;

		internal Border TextBlockBorder => textBlockBorder;

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
		}

	}
}
