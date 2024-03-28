using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Windows.Foundation;
using WGridLength = Microsoft.UI.Xaml.GridLength;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Platform
{
	// This is needed by WinUI because of 
	// https://github.com/microsoft/microsoft-ui-xaml/issues/2698#issuecomment-648751713
	[Microsoft.UI.Xaml.Data.Bindable]
	public partial class RootNavigationView : MauiNavigationView
	{
		double AppBarTitleHeight => _useCustomAppTitleBar ? _appBarTitleHeight : 0;
		double _appBarTitleHeight;
		bool _useCustomAppTitleBar;
		readonly FlyoutPanel _flyoutPanel = new FlyoutPanel();

		public RootNavigationView()
		{
			IsSettingsVisible = false;
			IsPaneToggleButtonVisible = false;
			PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
			IsTitleBarAutoPaddingEnabled = false;
			IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;

			RegisterPropertyChangedCallback(IsBackButtonVisibleProperty, BackButtonVisibleChanged);
			RegisterPropertyChangedCallback(OpenPaneLengthProperty, PaneLengthPropertyChanged);
			RegisterPropertyChangedCallback(HeaderProperty, HeaderPropertyChanged);
			RegisterPropertyChangedCallback(PaneFooterProperty, HeaderPropertyChanged);
			RegisterPropertyChangedCallback(PaneDisplayModeProperty, PaneDisplayModeChanged);

			this.PaneOpened += (_, __) => UpdatePaneContentGridMargin();
			this.DisplayModeChanged += (_, __) => UpdateNavigationAndPaneButtonHolderGridStyles();
		}

		internal new MauiToolbar? Toolbar
		{
			get => MauiNavigationView.Toolbar as MauiToolbar;
			set
			{
				if (MauiNavigationView.Toolbar == value)
					return;

				MauiNavigationView.Toolbar = value;

				if (value != null)
				{
					value.NavigationViewBackButton = NavigationViewBackButton;
					value.TogglePaneButton = TogglePaneButton;
				}

				UpdateToolbarPlacement();
				UpdateHeaderPropertyBinding();
			}
		}

		internal Size FlyoutPaneSize
		{
			get;
			private set;
		}

		void PaneDisplayModeChanged(DependencyObject sender, DependencyProperty dp)
		{
			UpdateToolbarPlacement();
			UpdatePaneContentGridMargin();
		}


		private protected override void ToolbarChanged()
		{
			if (Toolbar is MauiToolbar mauiToolbar)
			{
				Toolbar = mauiToolbar;
				UpdateToolbarPlacement();
			}
			else
			{
				// By default MauiNavigationView always sets 
				// NavigationView.Header to the Toolbar
				// This lets us pivot based on the type of pane display mode				
				base.ToolbarChanged();
			}

		}

		void UpdateToolbarPlacement()
		{
			if (TopNavArea != null)
			{
				if (PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
				{
					// The TopNavArea has a background set which makes the window action buttons unclickable
					// So this offsets the TopNavArea by the size of the AppTitleBar
					TopNavArea.Margin = new UI.Xaml.Thickness(0, AppBarTitleHeight, 0, 0);
					Header = null;
					PaneFooter = Toolbar;

					if (Toolbar != null)
					{
						MauiToolbar.ContentGridMargin = new UI.Xaml.Thickness(0, 0, 4, 0);
						MauiToolbar.TextBlockBorderVerticalAlignment = VerticalAlignment.Center;
					}
				}
				else if (PaneFooter == Toolbar || Header == null)
				{
					TopNavArea.Margin = new UI.Xaml.Thickness(0, 0, 0, 0);

					// We only null out the PaneFooter if we're moving the HeaderControl from the
					// Footer to the Header. Which means we're popping off a TabbedPage and
					// moving to a ContentPage
					// If the RootView is a FlyoutPage then the Header will be part of the FlyoutPage
					// And the PaneFooter will be the Flyout Content
					if (PaneFooter == Toolbar)
						PaneFooter = null;

					Header = Toolbar;

					if (Toolbar != null)
					{
						MauiToolbar.ContentGridMargin = new UI.Xaml.Thickness(0, 0, 0, 0);
						MauiToolbar.TextBlockBorderVerticalAlignment = VerticalAlignment.Top;
					}
				}
			}
		}

		void HeaderPropertyChanged(DependencyObject sender, DependencyProperty dp) =>
			UpdateHeaderPropertyBinding();

		void UpdateHeaderPropertyBinding()
		{
			Binding isBackButtonVisible = new Binding();
			isBackButtonVisible.Source = Toolbar;
			isBackButtonVisible.Path = new PropertyPath("IsBackButtonVisible");
			isBackButtonVisible.Mode = BindingMode.OneWay;
			isBackButtonVisible.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			BindingOperations.SetBinding(this, IsBackButtonVisibleProperty, isBackButtonVisible);

			Binding isBackEnabled = new Binding();
			isBackEnabled.Source = Toolbar;
			isBackEnabled.Path = new PropertyPath("IsBackEnabled");
			isBackEnabled.Mode = BindingMode.OneWay;
			isBackEnabled.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			BindingOperations.SetBinding(this, IsBackEnabledProperty, isBackEnabled);

			var HeaderContent = (ContentControl)GetTemplateChild("HeaderContent");

			if (HeaderContent != null)
			{
				Binding visibilityBinding = new Binding();
				visibilityBinding.Source = Toolbar;
				visibilityBinding.Path = new PropertyPath("Visibility");
				visibilityBinding.Mode = BindingMode.OneWay;
				visibilityBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
				BindingOperations.SetBinding(HeaderContent, ContentControl.VisibilityProperty, visibilityBinding);

				Binding backgroundBinding = new Binding();
				backgroundBinding.Source = Toolbar;
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

			IsBackEnabled = (IsBackButtonVisible == NavigationViewBackButtonVisible.Visible) &&
				(Toolbar?.IsBackEnabled ?? true);

			UpdateToolbarPlacement();
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

			if (Toolbar != null)
				UpdateHeaderPropertyBinding();

			PaneContentGrid!.SizeChanged += OnPaneContentGridSizeChanged;

			// This is the height taken up by the backbutton/pane toggle button
			// we use this to offset the height of our flyout content
			PaneContentGrid.RowDefinitions[1]
				.RegisterPropertyChangedCallback(RowDefinition.HeightProperty, PaneContentTopPaddingChanged);

			ButtonHolderGrid!.SizeChanged += (_, args) =>
			{
				UpdateNavigationAndPaneButtonHolderGridStyles();
			};

			TogglePaneButton!.SizeChanged += (_, args) =>
			{
				UpdateNavigationAndPaneButtonHolderGridStyles();
			};

			UpdateToolbarPlacement();

			if (Toolbar != null)
			{
				Toolbar.NavigationViewBackButton = NavigationViewBackButton;
				Toolbar.TogglePaneButton = TogglePaneButton;
			}
		}

		// Change this to use binding
		void PaneContentTopPaddingChanged(DependencyObject sender, DependencyProperty dp)
		{
			UpdatePaneContentGridMargin();
		}

		internal void UpdateAppTitleBar(double appTitleBarHeight)
		{
			UpdateAppTitleBar(appTitleBarHeight, _useCustomAppTitleBar);
		}

		internal void UpdateAppTitleBar(double appTitleBarHeight, bool useCustomAppTitleBar)
		{
			if (_useCustomAppTitleBar == useCustomAppTitleBar && appTitleBarHeight == _appBarTitleHeight)
				return;

			_useCustomAppTitleBar = useCustomAppTitleBar;
			_appBarTitleHeight = appTitleBarHeight;
			UpdateNavigationAndPaneButtonHolderGridStyles();
		}

		void UpdateNavigationAndPaneButtonHolderGridStyles()
		{
			var buttonHeight = Math.Min(_appBarTitleHeight, DefaultNavigationBackButtonHeight);
			var buttonRatio = buttonHeight / DefaultNavigationBackButtonHeight;

			MauiNavigationView.NavigationBackButtonHeight = buttonHeight;
			MauiNavigationView.NavigationBackButtonWidth = DefaultNavigationBackButtonWidth * buttonRatio;

			var paneToggleHeight = Math.Min(_appBarTitleHeight, DefaultPaneToggleButtonHeight);
			var paneToggleRatio = paneToggleHeight / DefaultPaneToggleButtonHeight;

			MauiNavigationView.PaneToggleButtonHeight = paneToggleHeight;
			MauiNavigationView.PaneToggleButtonWidth = DefaultPaneToggleButtonWidth * paneToggleRatio;

			if (PaneDisplayMode == NavigationViewPaneDisplayMode.LeftMinimal ||
				PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
			{
				MauiNavigationView.NavigationViewButtonHolderGridMargin = new WThickness(0, 0, 0, 0);
				MauiNavigationView.NavigationViewBackButtonMargin = new WThickness(0, 0, 0, 0);
				MauiNavigationView.PaneToggleButtonPadding = new WThickness();
			}
			else if (PaneDisplayMode == NavigationViewPaneDisplayMode.LeftCompact ||
					PaneDisplayMode == NavigationViewPaneDisplayMode.Left ||
					DisplayMode == NavigationViewDisplayMode.Compact)
			{
				MauiNavigationView.NavigationViewButtonHolderGridMargin = new WThickness(0, 0, 0, 0);
				if (IsPaneToggleButtonVisible)
					MauiNavigationView.NavigationViewBackButtonMargin = new WThickness(4, 0, 0, 2);
				else
					MauiNavigationView.NavigationViewBackButtonMargin = new WThickness(4, 0, 0, 0);

				MauiNavigationView.PaneToggleButtonPadding = new WThickness(4, 2, 4, 2);
			}

			UpdatePaneContentGridMargin();
			UpdateToolbarPlacement();
		}

		// This updates the amount of space between the top of the window
		// And the beginning of the flyout content.
		// The PaneContentGrid takes up the entire height of the window and
		// it uses a RowDefinition to offset the content from the top of the window
		// so that the content is below the pane toggle and back button
		void UpdatePaneContentGridMargin()
		{
			if (ButtonHolderGrid == null || ContentPaneTopPadding == null || PaneContentGrid == null)
				return;

			var height = Math.Max(ButtonHolderGrid.ActualHeight, _appBarTitleHeight);
			if (PaneContentGrid.RowDefinitions[1].Height.Value != height)
			{
				PaneContentGrid.RowDefinitions[1].Height = new WGridLength(height);
				ContentPaneTopPadding.Height = 0;
			}
			// this ensures that when we are showing the entire left pane that it will fill the width of the container
			switch (PaneDisplayMode)
			{
				case NavigationViewPaneDisplayMode.Left:
					PaneContentGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
					break;
				default:
					PaneContentGrid.HorizontalAlignment = HorizontalAlignment.Left;
					break;
			}
		}

		void OnPaneContentGridSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateFlyoutPaneSize();
			UpdatePaneContentGridMargin();
		}

		void UpdateFlyoutPaneSize()
		{
			if (PaneContentGrid == null)
				return;

			var newHeight = PaneContentGrid.ActualHeight - PaneContentGrid.RowDefinitions[1].Height.Value;
			if (newHeight < 0)
				return;

			var newSize = new Size(OpenPaneLength, newHeight);
			if (newSize == FlyoutPaneSize)
				return;

			FlyoutPaneSize = newSize;

			_flyoutPanel.ContentWidth = FlyoutPaneSize.Width;
			_flyoutPanel.InvalidateMeasure();
		}

		void ReplacePaneMenuItemsWithCustomContent(UIElement? customContent)
		{
			_flyoutPanel.Children.Clear();

			if (customContent == null)
			{
				PaneCustomContent = null;
			}
			else
			{
				_flyoutPanel.Children.Add(customContent);
				PaneCustomContent = _flyoutPanel;
			}
		}

		internal Maui.IView? FlyoutView
		{
			get => _flyoutPanel.FlyoutView;
			set => _flyoutPanel.FlyoutView = value;
		}

		// We use a container because if we just assign our Flyout to the PaneFooter on the NavigationView 
		// The measure call passes in PositiveInfinity for the measurements which causes the layout system
		// to crash. So we use this Panel to facilitate more constrained measuring values
		class FlyoutPanel : Panel
		{
			public Maui.IView? FlyoutView { get; set; }

			public FlyoutPanel()
			{
			}

			public double ContentWidth { get; set; }

			FrameworkElement? FlyoutContent =>
				Children.Count > 0 ? (FrameworkElement?)Children[0] : null;

			protected override Size MeasureOverride(Size availableSize)
			{
				if (FlyoutContent == null)
					return new Size(0, 0);

				if (ContentWidth == 0)
					return new Size(0, 0);

				if (FlyoutView != null)
					FlyoutView.Measure(ContentWidth, availableSize.Height);
				else
					FlyoutContent.Measure(new Size(ContentWidth, availableSize.Height));

				return FlyoutContent.DesiredSize;
			}

			protected override Size ArrangeOverride(Size finalSize)
			{
				if (FlyoutContent == null)
					return new Size(0, 0);

				if (finalSize.Width * finalSize.Height == 0 &&
					FlyoutContent.ActualWidth * FlyoutContent.ActualHeight == 0)
				{
					return finalSize;
				}

				if (FlyoutView != null)
					FlyoutView.Arrange(new Graphics.Rect(0, 0, finalSize.Width, finalSize.Height));
				else
					FlyoutContent.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

				return new Size(FlyoutContent.ActualWidth, FlyoutContent.ActualHeight);
			}
		}

		private protected override void UpdateFlyoutCustomContent()
		{
			ReplacePaneMenuItemsWithCustomContent(MauiNavigationView.FlyoutCustomContent as UIElement);
		}
	}
}
