#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;
using WVisibility = Microsoft.UI.Xaml.Visibility;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using Microsoft.UI.Xaml.Media;
using WImage = Microsoft.UI.Xaml.Controls.Image;

namespace Microsoft.Maui.Platform
{
	public partial class WindowHeader
	{
		public static readonly DependencyProperty IsBackButtonVisibleProperty
			= DependencyProperty.Register(nameof(IsBackButtonVisible), typeof(NavigationViewBackButtonVisible), typeof(WindowHeader), 
				new PropertyMetadata(default(NavigationViewBackButtonVisible), OnIsBackButtonVisibleChanged));

		static void OnIsBackButtonVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}

		//internal TranslateTransform? ClipGeometryTransform { get; private set; }
		//internal RectangleGeometry? LayoutRootClip { get; private set; }
		//internal Grid? LayoutRoot { get; private set; }

		public WindowHeader()
		{
			InitializeComponent();
		}

		//protected override void OnApplyTemplate()
		//{
		//	base.OnApplyTemplate();
		//	ClipGeometryTransform = (TranslateTransform)GetTemplateChild("ClipGeometryTransform");
		//	LayoutRoot = (Grid)GetTemplateChild("LayoutRoot");
		//	LayoutRootClip = LayoutRoot.Clip;
		//}

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
	}
}
