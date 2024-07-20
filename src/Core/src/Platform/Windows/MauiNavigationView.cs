using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WGridLength = Microsoft.UI.Xaml.GridLength;
using WRectangle = Microsoft.UI.Xaml.Shapes.Rectangle;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Platform
{
	// This is needed by WinUI because of 
	// https://github.com/microsoft/microsoft-ui-xaml/issues/2698#issuecomment-648751713
	[Microsoft.UI.Xaml.Data.Bindable]
	public partial class MauiNavigationView : NavigationView
	{
		private int _currentFlyoutBehavior = -1;

		internal StackPanel? TopNavArea { get; private set; }
		internal ItemsRepeater? TopNavMenuItemsHost { get; private set; }
		internal Grid? PaneContentGrid { get; private set; }
		internal event EventHandler? OnApplyTemplateFinished;
		internal SplitView? RootSplitView { get; private set; }
		internal ScrollViewer? MenuItemsScrollViewer { get; private set; }
		internal Grid? ContentPaneTopPadding { get; private set; }
		internal Grid? PaneToggleButtonGrid { get; private set; }
		internal Grid? ButtonHolderGrid { get; private set; }
		internal Grid? ContentGrid { get; private set; }
		internal Button? NavigationViewBackButton { get; private set; }
		internal Button? TogglePaneButton { get; private set; }
		internal Button? NavigationViewCloseButton { get; private set; }
		internal ColumnDefinition? PaneHeaderCloseButtonColumn { get; private set; }
		internal ColumnDefinition? PaneHeaderToggleButtonColumn { get; private set; }
		internal ContentControl? PaneCustomContentBorder { get; private set; }
		internal RowDefinition? ItemsContainerRow { get; private set; }
		internal RowDefinition? PaneContentGridToggleButtonRow { get; private set; }
		internal RowDefinition? PaneHeaderContentBorderRow { get; private set; }

		// The NavigationView occasionally likes to switch back to "LeftMinimal"
		// So we use this to switch it back whenver that happens.
		internal NavigationViewPaneDisplayMode? PinPaneDisplayModeTo
		{
			get => _pinPaneDisplayModeTo;
			set
			{
				_pinPaneDisplayModeTo = value;
				UpdateToPinnedDisplayMode();

			}
		}

		public MauiNavigationView()
		{
			this.RegisterPropertyChangedCallback(MenuItemsSourceProperty, (_, __) => UpdateMenuItemsContainerHeight());
			this.RegisterPropertyChangedCallback(MenuItemsProperty, (_, __) => UpdateMenuItemsContainerHeight());
			RegisterPropertyChangedCallback(PaneDisplayModeProperty, PaneDisplayModeChanged);
		}

		void PaneDisplayModeChanged(DependencyObject sender, DependencyProperty dp) =>
			UpdateToPinnedDisplayMode();

		void UpdateToPinnedDisplayMode()
		{
			if (PinPaneDisplayModeTo != null && PinPaneDisplayModeTo.Value != this.PaneDisplayMode)
			{
				PaneDisplayMode = PinPaneDisplayModeTo.Value;
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			MenuItemsScrollViewer = (ScrollViewer)GetTemplateChild("MenuItemsScrollViewer");
			PaneContentGrid = (Grid)GetTemplateChild("PaneContentGrid");
			RootSplitView = (SplitView)GetTemplateChild("RootSplitView");
			TopNavArea = (StackPanel)GetTemplateChild("TopNavArea");
			TopNavMenuItemsHost = (ItemsRepeater)GetTemplateChild("TopNavMenuItemsHost");
			ContentPaneTopPadding = (Grid)GetTemplateChild("ContentPaneTopPadding");
			PaneToggleButtonGrid = (Grid)GetTemplateChild("PaneToggleButtonGrid");
			ButtonHolderGrid = (Grid)GetTemplateChild("ButtonHolderGrid");
			ContentGrid = (Grid)GetTemplateChild("ContentGrid");
			NavigationViewBackButton = (Button)GetTemplateChild("NavigationViewBackButton");
			TogglePaneButton = (Button)GetTemplateChild("TogglePaneButton");
			NavigationViewCloseButton = (Button)GetTemplateChild("NavigationViewCloseButton");
			PaneHeaderCloseButtonColumn = (ColumnDefinition)GetTemplateChild("PaneHeaderCloseButtonColumn");
			PaneHeaderToggleButtonColumn = (ColumnDefinition)GetTemplateChild("PaneHeaderToggleButtonColumn");
			PaneCustomContentBorder = (ContentControl)GetTemplateChild("PaneCustomContentBorder");
			ItemsContainerRow = (RowDefinition)GetTemplateChild("ItemsContainerRow");
			PaneContentGridToggleButtonRow = (RowDefinition)GetTemplateChild("PaneContentGridToggleButtonRow");
			PaneHeaderContentBorderRow = (RowDefinition)GetTemplateChild("PaneHeaderContentBorderRow");

			UpdateNavigationBackButtonSize();
			UpdateNavigationViewBackButtonMargin();
			UpdateNavigationViewButtonHolderGridMargin();
			OnApplyTemplateCore();
			OnApplyTemplateFinished?.Invoke(this, EventArgs.Empty);

			TogglePaneButton.RegisterPropertyChangedCallback(Button.HeightProperty, (_, __) => UpdatePaneToggleButtonSize());
			TogglePaneButton.RegisterPropertyChangedCallback(Button.WidthProperty, (_, __) => UpdatePaneToggleButtonSize());

			NavigationViewBackButton.RegisterPropertyChangedCallback(Button.HeightProperty, (_, __) => UpdateNavigationBackButtonSize());
			NavigationViewBackButton.RegisterPropertyChangedCallback(Button.WidthProperty, (_, __) => UpdateNavigationBackButtonSize());

			NavigationViewCloseButton.RegisterPropertyChangedCallback(Button.HeightProperty, (_, __) => UpdateNavigationBackButtonSize());
			NavigationViewCloseButton.RegisterPropertyChangedCallback(Button.WidthProperty, (_, __) => UpdateNavigationBackButtonSize());


			// These columns create a left padding on the PaneHeader
			// So this code just removes that padding
			PaneHeaderCloseButtonColumn.RegisterPropertyChangedCallback(ColumnDefinition.WidthProperty, (_, __) => PaneHeaderCloseButtonColumn.Width = new WGridLength(0));
			PaneHeaderToggleButtonColumn.RegisterPropertyChangedCallback(ColumnDefinition.WidthProperty, (_, __) => PaneHeaderToggleButtonColumn.Width = new WGridLength(0));
			PaneHeaderToggleButtonColumn.Width = new WGridLength(0);
			PaneHeaderCloseButtonColumn.Width = new WGridLength(0);

			// When the NavigationView is in locked mode the min height on the PaneHeader row gets set to 40
			// Which creates space between the title bar and the top of the flyout content
			PaneContentGridToggleButtonRow.MinHeight = 0;
			PaneContentGridToggleButtonRow.RegisterPropertyChangedCallback(RowDefinition.MinHeightProperty, (_, __) =>
				PaneContentGridToggleButtonRow.MinHeight = 0);

			PaneHeaderContentBorderRow.MinHeight = 0;
			PaneHeaderContentBorderRow.RegisterPropertyChangedCallback(RowDefinition.MinHeightProperty, (_, __) =>
				PaneHeaderContentBorderRow.MinHeight = 0);

			// WinUI has this set to -1,3,-1,3 but I'm not really sure why
			// It causes the content to not be flush up against the title bar
			PaneContentGrid.Margin = new WThickness(0, 0, 0, 0);
			UpdateMenuItemsContainerHeight();
		}


		void UpdateMenuItemsContainerHeight()
		{
			if (ItemsContainerRow == null || PaneContentGrid == null)
				return;

			// If we're not using the menu items then let the custom content row 
			// occupy all available space
			if (MenuItems?.Count > 0 || MenuItemsSource != null)
			{
				ItemsContainerRow.Height = new WGridLength(1, UI.Xaml.GridUnitType.Star);
				PaneContentGrid.RowDefinitions[4].Height = new WGridLength(1, UI.Xaml.GridUnitType.Auto);
			}
			else
			{
				// This changes the menu items grid so it just sizes to its content
				// By default it was always filling the entire container but we don't want that
				ItemsContainerRow.Height = new WGridLength(1, UI.Xaml.GridUnitType.Auto);

				// this forces the custom content to take up the entire height that it can
				PaneContentGrid.RowDefinitions[4].Height = new WGridLength(1, UI.Xaml.GridUnitType.Star);
			}
		}

		private protected virtual void OnApplyTemplateCore()
		{
		}

		internal void UpdatePaneDisplayModeFromFlyoutBehavior(FlyoutBehavior flyoutBehavior)
		{
			if (_currentFlyoutBehavior == (int)flyoutBehavior)
			{
				return;
			}

			_currentFlyoutBehavior = (int)flyoutBehavior;
			switch (flyoutBehavior)
			{
				case FlyoutBehavior.Flyout:
					IsPaneToggleButtonVisible = true;
					// Workaround for
					// https://github.com/microsoft/microsoft-ui-xaml/issues/6493
					PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
					PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
					break;
				case FlyoutBehavior.Locked:
					PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
					IsPaneToggleButtonVisible = false;
					break;
				case FlyoutBehavior.Disabled:
					PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
					IsPaneToggleButtonVisible = false;
					IsPaneOpen = false;
					break;
			}
		}

		#region Toolbar
		internal static readonly DependencyProperty ToolbarProperty
			= DependencyProperty.Register(nameof(Toolbar), typeof(UIElement), typeof(MauiNavigationView),
				new PropertyMetadata(null, (d, _) => ((RootNavigationView)d).ToolbarChanged()));

		internal static UIElement? Toolbar
		{
			get => (UIElement?)GetValue(ToolbarProperty);
			set => SetValue(ToolbarProperty, value);
		}

		protected private virtual void ToolbarChanged()
		{
			Header = MauiNavigationView.Toolbar;
		}

		#endregion

		#region NavigationViewBackButtonMargin
		public static readonly DependencyProperty NavigationViewBackButtonMarginProperty
			= DependencyProperty.Register(nameof(NavigationViewBackButtonMargin), typeof(WThickness), typeof(MauiNavigationView),
				new PropertyMetadata(new WThickness(4, 2, 0, 2), NavigationViewBackButtonMarginChanged));

		public static WThickness NavigationViewBackButtonMargin
		{
			get => (WThickness)GetValue(NavigationViewBackButtonMarginProperty);
			set => SetValue(NavigationViewBackButtonMarginProperty, value);
		}

		static void NavigationViewBackButtonMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MauiNavigationView)d).UpdateNavigationViewBackButtonMargin();
		}

		void UpdateNavigationViewBackButtonMargin()
		{
			if (NavigationViewBackButton != null)
				NavigationViewBackButton.Margin = MauiNavigationView.NavigationViewBackButtonMargin;

			if (NavigationViewCloseButton != null)
				NavigationViewCloseButton.Margin = MauiNavigationView.NavigationViewBackButtonMargin;
		}
		#endregion

		#region NavigationViewButtonHolderGridMargin
		internal static readonly DependencyProperty NavigationViewButtonHolderGridMarginProperty
			= DependencyProperty.Register(nameof(NavigationViewButtonHolderGridMargin), typeof(WThickness), typeof(MauiNavigationView),
				new PropertyMetadata((WThickness)Application.Current.Resources["NavigationViewButtonHolderGridMargin"], NavigationViewButtonHolderGridMarginChanged));

		internal static WThickness NavigationViewButtonHolderGridMargin
		{
			get => (WThickness)GetValue(NavigationViewButtonHolderGridMarginProperty);
			set => SetValue(NavigationViewButtonHolderGridMarginProperty, value);
		}

		static void NavigationViewButtonHolderGridMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MauiNavigationView)d).UpdateNavigationViewButtonHolderGridMargin();
		}

		void UpdateNavigationViewButtonHolderGridMargin()
		{
			if (ButtonHolderGrid != null)
				ButtonHolderGrid.Margin = MauiNavigationView.NavigationViewButtonHolderGridMargin;
		}
		#endregion

		#region NavigationBackButtonHeight/Width
		internal static double DefaultNavigationBackButtonHeight => (double)Application.Current.Resources["NavigationBackButtonHeight"];
		internal static double DefaultNavigationBackButtonWidth => (double)Application.Current.Resources["NavigationBackButtonWidth"];

		internal static readonly DependencyProperty NavigationBackButtonHeightProperty
			= DependencyProperty.Register(nameof(NavigationBackButtonHeight), typeof(double), typeof(MauiNavigationView),
				new PropertyMetadata(DefaultNavigationBackButtonHeight, OnNavigationBackButtonSizeChanged));

		internal static double NavigationBackButtonHeight
		{
			get => (double)GetValue(NavigationBackButtonHeightProperty);
			set => SetValue(NavigationBackButtonHeightProperty, value);
		}

		internal static readonly DependencyProperty NavigationBackButtonWidthProperty
			= DependencyProperty.Register(nameof(NavigationBackButtonWidth), typeof(double), typeof(MauiNavigationView),
				new PropertyMetadata(DefaultNavigationBackButtonWidth, OnNavigationBackButtonSizeChanged));

		internal static double NavigationBackButtonWidth
		{
			get => (double)GetValue(NavigationBackButtonWidthProperty);
			set => SetValue(NavigationBackButtonWidthProperty, value);
		}

		static void OnNavigationBackButtonSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MauiNavigationView)d).UpdateNavigationBackButtonSize();
		}


		void UpdateNavigationBackButtonSize()
		{
			if (NavigationViewBackButton != null && NavigationViewCloseButton != null)
			{
				if (NavigationViewBackButton.Height != MauiNavigationView.NavigationBackButtonHeight)
					NavigationViewBackButton.Height = MauiNavigationView.NavigationBackButtonHeight;

				if (NavigationViewBackButton.Width != MauiNavigationView.NavigationBackButtonWidth)
					NavigationViewBackButton.Width = MauiNavigationView.NavigationBackButtonWidth;

				if (NavigationViewCloseButton.Height != MauiNavigationView.NavigationBackButtonHeight)
					NavigationViewCloseButton.Height = MauiNavigationView.NavigationBackButtonHeight;

				if (NavigationViewCloseButton.Width != MauiNavigationView.NavigationBackButtonWidth)
					NavigationViewCloseButton.Width = MauiNavigationView.NavigationBackButtonWidth;
			}
		}
		#endregion

		#region Flyout Custom Content
		internal static readonly DependencyProperty FlyoutCustomContentProperty
			= DependencyProperty.Register(nameof(FlyoutCustomContent), typeof(UIElement), typeof(MauiNavigationView),
				new PropertyMetadata(null, (d, _) => ((RootNavigationView)d).UpdateFlyoutCustomContent()));

		internal static UIElement? FlyoutCustomContent
		{
			get => (UIElement?)GetValue(FlyoutCustomContentProperty);
			set => SetValue(FlyoutCustomContentProperty, value);
		}

		protected private virtual void UpdateFlyoutCustomContent()
		{
			PaneCustomContent = MauiNavigationView.FlyoutCustomContent;
		}
		#endregion

		#region PaneToggleButton
		internal static double DefaultPaneToggleButtonHeight => (double)Application.Current.Resources["PaneToggleButtonHeight"];

		// The resource is set to 40 but it appears that the NavigationView manually sets the width to 48
		internal static double DefaultPaneToggleButtonWidth => 48;//(double)Application.Current.Resources["PaneToggleButtonWidth"];

		internal static WThickness DefaultPaneToggleButtonPadding => (WThickness)Application.Current.Resources["NavigationViewItemButtonMargin"];

		internal static readonly DependencyProperty PaneToggleButtonPaddingProperty
			= DependencyProperty.Register(nameof(PaneToggleButtonPadding), typeof(WThickness), typeof(MauiNavigationView),
				new PropertyMetadata(DefaultPaneToggleButtonPadding, OnPaneToggleButtonSizeChanged));

		internal static WThickness PaneToggleButtonPadding
		{
			get => (WThickness)GetValue(PaneToggleButtonPaddingProperty);
			set => SetValue(PaneToggleButtonPaddingProperty, value);
		}

		internal static readonly DependencyProperty PaneToggleButtonHeightProperty
			= DependencyProperty.Register(nameof(PaneToggleButtonHeight), typeof(double), typeof(MauiNavigationView),
				new PropertyMetadata(DefaultPaneToggleButtonHeight, OnPaneToggleButtonSizeChanged));

		internal static double PaneToggleButtonHeight
		{
			get => (double)GetValue(PaneToggleButtonHeightProperty);
			set => SetValue(PaneToggleButtonHeightProperty, value);
		}

		internal static readonly DependencyProperty PaneToggleButtonWidthProperty
			= DependencyProperty.Register(nameof(PaneToggleButtonWidth), typeof(double), typeof(MauiNavigationView),
				new PropertyMetadata(DefaultPaneToggleButtonWidth, OnPaneToggleButtonSizeChanged));
		private NavigationViewPaneDisplayMode? _pinPaneDisplayModeTo;

		internal static double PaneToggleButtonWidth
		{
			get => (double)GetValue(PaneToggleButtonWidthProperty);
			set => SetValue(PaneToggleButtonWidthProperty, value);
		}

		static void OnPaneToggleButtonSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MauiNavigationView)d).UpdatePaneToggleButtonSize();
		}


		void UpdatePaneToggleButtonSize()
		{
			if (TogglePaneButton != null)
			{
				if (MauiNavigationView.PaneToggleButtonHeight != TogglePaneButton.Height)
				{
					TogglePaneButton.Height = MauiNavigationView.PaneToggleButtonHeight;
					var togglePaneButtonMinHeight = Math.Min((double)Application.Current.Resources["PaneToggleButtonHeight"], MauiNavigationView.PaneToggleButtonHeight);
					if (TogglePaneButton.MinHeight != togglePaneButtonMinHeight)
						TogglePaneButton.MinHeight = togglePaneButtonMinHeight;
				}

				if (TogglePaneButton.GetFirstDescendant<Grid>() is Grid grid)
				{
					if (grid.Height != MauiNavigationView.PaneToggleButtonHeight)
						grid.Height = MauiNavigationView.PaneToggleButtonHeight;

					// The row definition is bound to PaneToggleButtonHeight
					// the height is bound to MinHeight of the button
					if (grid.RowDefinitions[0].Height.Value != MauiNavigationView.PaneToggleButtonHeight)
						grid.RowDefinitions[0].Height = new WGridLength(MauiNavigationView.PaneToggleButtonHeight);
				}

				if (MauiNavigationView.PaneToggleButtonWidth != TogglePaneButton.Width)
					TogglePaneButton.Width = MauiNavigationView.PaneToggleButtonWidth;

				var togglePaneButtonMinWidth = Math.Min((double)Application.Current.Resources["PaneToggleButtonWidth"], MauiNavigationView.PaneToggleButtonWidth);
				if (TogglePaneButton.MinWidth != togglePaneButtonMinWidth)
					TogglePaneButton.MinWidth = togglePaneButtonMinWidth;

				if (TogglePaneButton.Padding != MauiNavigationView.PaneToggleButtonPadding)
					TogglePaneButton.Padding = MauiNavigationView.PaneToggleButtonPadding;
			}
		}
		#endregion
	}
}