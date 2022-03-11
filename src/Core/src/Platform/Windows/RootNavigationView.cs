using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Windows.Foundation;

namespace Microsoft.Maui.Platform
{
	// This is needed by WinUI because of 
	// https://github.com/microsoft/microsoft-ui-xaml/issues/2698#issuecomment-648751713
	[Microsoft.UI.Xaml.Data.Bindable]
	public class RootNavigationView : MauiNavigationView
	{
		double _paneHeaderContentHeight;
		MauiToolbar? _headerControl;
		int _appBarTitleHeight = 48;

		internal Size FlyoutPaneSize { get; private set; }
		internal MauiToolbar? HeaderControl
		{
			get => _headerControl;
			set
			{
				_headerControl = value;
				UpdateTopNavAreaMargin();
			}
		}

		public RootNavigationView()
		{
			IsSettingsVisible = false;
			MenuItemsSource = null;
			IsPaneToggleButtonVisible = false;
			PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
			IsTitleBarAutoPaddingEnabled = false;
			IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
			RegisterPropertyChangedCallback(IsBackButtonVisibleProperty, BackButtonVisibleChanged);
			RegisterPropertyChangedCallback(OpenPaneLengthProperty, PaneLengthPropertyChanged);
			RegisterPropertyChangedCallback(HeaderProperty, HeaderPropertyChanged);
			RegisterPropertyChangedCallback(PaneFooterProperty, HeaderPropertyChanged);
			RegisterPropertyChangedCallback(PaneDisplayModeProperty, PaneDisplayModeChanged);
		}

		void PaneDisplayModeChanged(DependencyObject sender, DependencyProperty dp)
		{
			UpdateTopNavAreaMargin();
			UpdateFlyoutPanelMargin();
		}

		void UpdateTopNavAreaMargin()
		{
			if (TopNavArea != null)
			{
				if (PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
				{
					// The TopNavArea has a background set which makes the window action buttons unclickable
					// So this offsets the TopNavArea by the size of the AppTitleBar
					TopNavArea.Margin = new UI.Xaml.Thickness(0, _appBarTitleHeight, 0, 0);
					Header = null;
					PaneFooter = HeaderControl;

					if (HeaderControl != null)
					{
						HeaderControl.ContentGridMargin = new UI.Xaml.Thickness(0, 0, 4, 0);
						HeaderControl.TextBlockBorderVerticalAlignment = VerticalAlignment.Center;
					}
				}
				else if (PaneFooter == HeaderControl || Header == null)
				{
					TopNavArea.Margin = new UI.Xaml.Thickness(0, 0, 0, 0);

					// We only null out the PaneFooter if we're moving the HeaderControl from the
					// Footer to the Header. Which means we're popping off a TabbedPage and
					// moving to a ContentPage
					// If the RootView is a FlyoutPage then the Header will be part of the FlyoutPage
					// And the PaneFooter will be the Flyout Content
					if (PaneFooter == HeaderControl)
						PaneFooter = null;

					Header = HeaderControl;

					if (HeaderControl != null)
					{
						HeaderControl.ContentGridMargin = new UI.Xaml.Thickness(0, 0, 0, 0);
						HeaderControl.TextBlockBorderVerticalAlignment = VerticalAlignment.Top;
					}
				}
			}
		}

		void HeaderPropertyChanged(DependencyObject sender, DependencyProperty dp) =>
			UpdateHeaderPropertyBinding();

		void UpdateHeaderPropertyBinding()
		{
			Binding isBackButtonVisible = new Binding();
			isBackButtonVisible.Source = HeaderControl;
			isBackButtonVisible.Path = new PropertyPath("IsBackButtonVisible");
			isBackButtonVisible.Mode = BindingMode.OneWay;
			isBackButtonVisible.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			BindingOperations.SetBinding(this, IsBackButtonVisibleProperty, isBackButtonVisible);

			var HeaderContent = (ContentControl)GetTemplateChild("HeaderContent");

			if (HeaderContent != null)
			{
				Binding visibilityBinding = new Binding();
				visibilityBinding.Source = HeaderControl;
				visibilityBinding.Path = new PropertyPath("Visibility");
				visibilityBinding.Mode = BindingMode.OneWay;
				visibilityBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
				BindingOperations.SetBinding(HeaderContent, ContentControl.VisibilityProperty, visibilityBinding);

				Binding backgroundBinding = new Binding();
				backgroundBinding.Source = HeaderControl;
				backgroundBinding.Path = new PropertyPath("Background");
				backgroundBinding.Mode = BindingMode.OneWay;
				backgroundBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
				BindingOperations.SetBinding(HeaderContent, ContentControl.BackgroundProperty, backgroundBinding);
			}
		}

		void PaneLengthPropertyChanged(DependencyObject sender, DependencyProperty dp)
		{
			UpdateFlyoutPaneSize();
		}

		void BackButtonVisibleChanged(DependencyObject sender, DependencyProperty dp)
		{
			if (IsBackButtonVisible == NavigationViewBackButtonVisible.Auto)
				IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;

			IsBackEnabled = (IsBackButtonVisible == NavigationViewBackButtonVisible.Visible);
			UpdateFlyoutPanelMargin();
			UpdateTopNavAreaMargin();
		}


		private protected override void OnApplyTemplateCore()
		{
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

			if (HeaderControl != null)
				UpdateHeaderPropertyBinding();

			PaneContentGrid!.SizeChanged += OnPaneContentGridSizeChanged;

			// This is the height taken up by the backbutton/pane toggle button
			// we use this to offset the height of our flyout content
			((FrameworkElement)GetTemplateChild("PaneHeaderContentBorder")).SizeChanged += OnPaneHeaderContentBorderSizeChanged;

			UpdateTopNavAreaMargin();
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
			_flyoutPanel.Height = FlyoutPaneSize.Height;
			_flyoutPanel.Width = FlyoutPaneSize.Width;
			UpdateFlyoutPanelMargin();
		}

		readonly FlyoutPanel _flyoutPanel = new FlyoutPanel();

		internal void ReplacePaneMenuItemsWithCustomContent(IView? customContent)
		{
			_flyoutPanel.Children.Clear();

			if (customContent == null)
			{
				PaneFooter = null;
			}
			else
			{
				if (customContent.ToPlatform() is UIElement element)
					_flyoutPanel.Children.Add(element);

				PaneFooter = _flyoutPanel;
			}
		}

		internal void UpdateFlyoutPanelMargin()
		{
			// The left pane on NavigationView currently doesn't account for a custom title bar
			// If you hide the backbutton and pane toggle button it will shift content up into the custom title
			// bar. There currently isn't a property associated with this padding it's just set inside the
			// source code on the PaneContentGrid

			if (ContentPaneTopPadding != null &&
				ButtonHolderGrid != null && 
				PaneContentGrid != null)
			{
				// PaneContentGrid is the top most container on the SplitView
				// This tells us the spacing that's been placed around it so that we
				// can offset our content relative to the AppTitleBar
				var leftPaneMarginHeight = PaneContentGrid.Margin.Top;

				if (ButtonHolderGrid.ActualHeight == 0)
				{
					ContentPaneTopPadding.Height = _appBarTitleHeight - leftPaneMarginHeight;
				}
				else
				{
					ContentPaneTopPadding.Height = ButtonHolderGrid.Margin.Top +
						ButtonHolderGrid.Margin.Bottom - leftPaneMarginHeight;
				}
			}
		}

		// We use a container because if we just assign our Flyout to the PaneFooter on the NavigationView 
		// The measure call passes in PositiveInfinity for the measurements which causes the layout system
		// to crash. So we use this Panel to facilitate more constrained measuring values
		class FlyoutPanel : Panel
		{
			public FlyoutPanel()
			{
				Height = 0;
				Width = 0;
			}

			FrameworkElement? FlyoutContent =>
				Children.Count > 0 ? (FrameworkElement?)Children[0] : null;

			protected override Size MeasureOverride(Size availableSize)
			{
				if (FlyoutContent == null)
					return new Size(0, 0);

				FlyoutContent.Measure(availableSize);
				return FlyoutContent.DesiredSize;
			}

			protected override Size ArrangeOverride(Size finalSize)
			{
				if (FlyoutContent == null)
					return new Size(0, 0);

				FlyoutContent.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
				return new Size(FlyoutContent.ActualWidth, FlyoutContent.ActualHeight);
			}
		}
	}
}
