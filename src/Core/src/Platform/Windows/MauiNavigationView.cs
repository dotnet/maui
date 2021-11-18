using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WThickness = Microsoft.UI.Xaml.Thickness;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using System.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui
{
	// This is needed by WinUI because of 
	// https://github.com/microsoft/microsoft-ui-xaml/issues/2698#issuecomment-648751713
	[Microsoft.UI.Xaml.Data.Bindable]
	public class MauiNavigationView : NavigationView
	{
		internal WGrid? ContentTopPadding { get; private set; }
		internal WGrid? PaneToggleButtonGrid { get; private set; }
		internal ContentControl? HeaderContent { get; private set; }
		internal Button? NavigationViewBackButton { get; private set; }
		WThickness? DefaultHeaderContentMargin { get; set; }
		internal WThickness? HeaderContentMargin { get; set; }

		WindowHeader _windowHeader;
		public MauiNavigationView()
		{
			IsPaneVisible = false;
			IsPaneToggleButtonVisible = false;
			PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;
			Header = (_windowHeader = new WindowHeader());
			IsBackEnabled = false;
			_windowHeader.Visibility = UI.Xaml.Visibility.Collapsed;
			IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			PaneToggleButtonGrid = (WGrid)GetTemplateChild("PaneToggleButtonGrid");
			ContentTopPadding = (WGrid)GetTemplateChild("ContentTopPadding");
			HeaderContent = (ContentControl)GetTemplateChild("HeaderContent");
			NavigationViewBackButton = (Button)GetTemplateChild("NavigationViewBackButton");

			// HeaderContent is set to a MinHeight of 48 so we have to collapse it if we
			// don't want it to take up any space
			HeaderContent.Visibility = _windowHeader.Visibility;

			// Read comment on MarginPropertyChanged
			var currentMargin = HeaderContent.Margin;
			HeaderContentMargin = new WThickness(
				0,
				0,
				currentMargin.Right,
				currentMargin.Bottom);

			HeaderContent.Margin = HeaderContentMargin.Value;
			HeaderContent.RegisterPropertyChangedCallback(ContentControl.MarginProperty, MarginPropertyChanged);
		}

		// Something inside the NavigationView gets really excited to change the margin on the header content
		// This causes the header to offset from the top of the screen which makes everything look off when you want to color
		// the top nav bar. AFAICT this margin isn't bound to any theme resource properties
		void MarginPropertyChanged(DependencyObject sender, DependencyProperty dp)
		{
			if (HeaderContent != null &&
				HeaderContentMargin != null &&
				HeaderContent.Margin != HeaderContentMargin.Value)
			{
				HeaderContent.Margin = HeaderContentMargin.Value;
			}
		}

		internal void UpdateBarBackgroundBrush(WBrush? brush)
		{

			if (brush == null)
				return;

			if (PaneToggleButtonGrid != null)
				PaneToggleButtonGrid.Background = brush;

			if (Header is WindowHeader windowHeader)
				windowHeader.CommandBar.Background = brush;

			// This is code that I'm excited I got to work but it'd be nice if it didn't exist
			// The back button on the NavigationView is part of a different view hierarchy than the Header CommandBar
			// When you click the "more" button on the command bar it expands vertically using a clip geometry
			// This code applies that same clip geometry (+ animation) to the container of the Back Button
			// This is mainly relevant when the user wants to color the BarBackground
			if (PaneToggleButtonGrid != null &&
				PaneToggleButtonGrid.Clip == null &&
				_windowHeader?.LayoutRootClip != null &&
				_windowHeader.LayoutRoot != null &&
				NavigationViewBackButton != null)
			{
				_windowHeader.TextBlockBorder.Height = _windowHeader.ActualHeight;
				PaneToggleButtonGrid.Height = _windowHeader.LayoutRoot.ActualHeight;

				RectangleGeometry rectangleGeometry = new RectangleGeometry();
				TranslateTransform translateTransform = new TranslateTransform();
				rectangleGeometry.Transform = translateTransform;

				Binding rectBinding = new Binding();
				rectBinding.Source = _windowHeader.LayoutRootClip;
				rectBinding.Path = new PropertyPath("Rect");
				rectBinding.Mode = BindingMode.OneWay;
				BindingOperations.SetBinding(rectangleGeometry, RectangleGeometry.RectProperty, rectBinding);

				Binding translateBinding = new Binding();
				translateBinding.Source = _windowHeader.LayoutRootClip.Transform;
				translateBinding.Path = new PropertyPath("Y");
				translateBinding.Mode = BindingMode.OneWay;
				BindingOperations.SetBinding(translateTransform, TranslateTransform.YProperty, translateBinding);

				PaneToggleButtonGrid.Clip = rectangleGeometry;
			}
		}
	}
}
