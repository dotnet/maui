using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Windows.Foundation;

namespace Microsoft.Maui.Platform
{
	// This is needed by WinUI because of 
	// https://github.com/microsoft/microsoft-ui-xaml/issues/2698#issuecomment-648751713
	[Microsoft.UI.Xaml.Data.Bindable]
	public class MauiNavigationView : NavigationView
	{
		double _paneHeaderContentHeight;
		internal Grid? ItemsContainerGrid { get; private set; }
		internal Grid? PaneContentGrid { get; private set; }
		internal event EventHandler? FlyoutPaneSizeChanged;
		internal Size FlyoutPaneSize { get; private set; }

		public MauiNavigationView()
		{
			IsSettingsVisible = false;
			MenuItemsSource = null;
			IsPaneToggleButtonVisible = false;
			PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
			IsTitleBarAutoPaddingEnabled = false;
			RegisterPropertyChangedCallback(IsBackButtonVisibleProperty, BackButtonVisibleChanged);
			RegisterPropertyChangedCallback(OpenPaneLengthProperty, PaneLengthPropertyChanged);
		}

		void PaneLengthPropertyChanged(DependencyObject sender, DependencyProperty dp)
		{
			UpdateFlyoutPaneSize();
		}

		void BackButtonVisibleChanged(DependencyObject sender, DependencyProperty dp)
		{
			IsBackEnabled = (IsBackButtonVisible == NavigationViewBackButtonVisible.Visible);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// We currently use the "PaneFooter" property to set custom content on the flyout
			// But this content has a top margin of 4 to separate it from the bottom menu items
			// Because we aren't using bottom menu items we just zero out the margin.
			// The way we size the flyout content is by retrieving the height/width of the ItemsContainerGrid
			// Because the FooterContentBorder has a margin the height of the ItemsContainerGrid will increase by four 
			// everytime you set the PaneFooter Content which will lead to a layout cycle.
			// TLDR; if you comment this out you'll get a layout cycle crash because of how we're extracting the WxH 
			// to measure the flyout content
			((FrameworkElement)GetTemplateChild("FooterContentBorder")).Margin = new UI.Xaml.Thickness(0);

			// This is used to left pad the content/header when the backbutton is visible
			// Because our backbutton is inside the appbar title we don't care about padding 
			// the content and header by the size of the backbutton.
			if (GetTemplateChild("ContentLeftPadding") is Grid g)
				g.Visibility = UI.Xaml.Visibility.Collapsed;

			var HeaderContent = (ContentControl)GetTemplateChild("HeaderContent");
			Binding visibilityBinding = new Binding();
			visibilityBinding.Source = this;
			visibilityBinding.Path = new PropertyPath("Header.Visibility");
			visibilityBinding.Mode = BindingMode.TwoWay;
			visibilityBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			BindingOperations.SetBinding(HeaderContent, VisibilityProperty, visibilityBinding);

			Binding backgroundBinding = new Binding();
			backgroundBinding.Source = this;
			backgroundBinding.Path = new PropertyPath("Header.Background");
			backgroundBinding.Mode = BindingMode.TwoWay;
			backgroundBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			BindingOperations.SetBinding(HeaderContent, BackgroundProperty, backgroundBinding);

			Binding isBackButtonVisible = new Binding();
			isBackButtonVisible.Source = this;
			isBackButtonVisible.Path = new PropertyPath("Header.IsBackButtonVisible");
			isBackButtonVisible.Mode = BindingMode.TwoWay;
			isBackButtonVisible.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			BindingOperations.SetBinding(this, IsBackButtonVisibleProperty, isBackButtonVisible);

			PaneContentGrid = (Grid)GetTemplateChild("PaneContentGrid");
			PaneContentGrid.SizeChanged += OnPaneContentGridSizeChanged;

			// This is the height taken up by the backbutton/pane toggle button
			// we use this to offset the height of our flyout content
			((FrameworkElement)GetTemplateChild("PaneHeaderContentBorder")).SizeChanged += OnPaneHeaderContentBorderSizeChanged;
		}

		void OnPaneHeaderContentBorderSizeChanged(object sender, SizeChangedEventArgs e)
		{
			_paneHeaderContentHeight = ((FrameworkElement)sender).ActualHeight;
			UpdateFlyoutPaneSize();
		}

		void OnPaneContentGridSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateFlyoutPaneSize();
		}

		void UpdateFlyoutPaneSize()
		{
			if (PaneContentGrid == null)
				return;

			FlyoutPaneSize = new Size(OpenPaneLength, PaneContentGrid.ActualHeight - _paneHeaderContentHeight);
			FlyoutPaneSizeChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
