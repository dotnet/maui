using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Microsoft.Maui.Handlers
{
	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, RootNavigationView>
	{
		readonly FlyoutPanel _flyoutPanel = new FlyoutPanel();
		long? _registerCallbackToken;
		NavigationRootManager? _navigationRootManager;
		protected override RootNavigationView CreateNativeView()
		{
			var navigationView = new RootNavigationView();

			navigationView.PaneFooter = _flyoutPanel;
			return navigationView;
		}

		protected override void ConnectHandler(RootNavigationView nativeView)
		{
			_navigationRootManager = MauiContext?.GetNavigationRootManager();
			nativeView.FlyoutPaneSizeChanged += OnFlyoutPaneSizeChanged;
			nativeView.PaneOpened += OnPaneOepened;
			_registerCallbackToken = nativeView.RegisterPropertyChangedCallback(NavigationView.IsBackButtonVisibleProperty, BackButtonVisibleChanged);
		}

		protected override void DisconnectHandler(RootNavigationView nativeView)
		{
			nativeView.FlyoutPaneSizeChanged += OnFlyoutPaneSizeChanged;
			nativeView.PaneOpened -= OnPaneOepened;

			if(_registerCallbackToken != null)
			{
				nativeView.UnregisterPropertyChangedCallback(NavigationView.IsBackButtonVisibleProperty, _registerCallbackToken.Value);
				_registerCallbackToken = null;
			}
		}

		void OnFlyoutPaneSizeChanged(object? sender, EventArgs e)
		{
			_flyoutPanel.Height = NativeView.FlyoutPaneSize.Height;
			_flyoutPanel.Width = NativeView.FlyoutPaneSize.Width;
			UpdateFlyoutPanelMargin();
		}


		void BackButtonVisibleChanged(DependencyObject sender, DependencyProperty dp)
			=> UpdateFlyoutPanelMargin();

		void UpdateFlyoutPanelMargin()
		{
			// The left pane on NavigationView currently doesn't account for a custom title bar
			// If you hide the backbutton and pane toggle button it will shift content up into the custom title
			// bar. There currently isn't a property associated with this padding it's just set inside the
			// source code on the PaneContentGrid
			if (NativeView.IsBackButtonVisible == NavigationViewBackButtonVisible.Collapsed &&
				NativeView.PaneDisplayMode == NavigationViewPaneDisplayMode.Left)
			{
				_flyoutPanel.Margin = new UI.Xaml.Thickness(
					_flyoutPanel.Margin.Left,
					40,
					_flyoutPanel.Margin.Right,
					_flyoutPanel.Margin.Bottom);
			}
			else
			{
				_flyoutPanel.Margin = new UI.Xaml.Thickness(
					_flyoutPanel.Margin.Left,
					0,
					_flyoutPanel.Margin.Right,
					_flyoutPanel.Margin.Bottom);
			}
		}

		void OnPaneOepened(NavigationView sender, object args)
		{
			VirtualView.IsPresented = sender.IsPaneOpen;
		}

		void UpdateDetail()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView.Detail.ToPlatform(MauiContext);

			NativeView.Content = VirtualView.Detail.ToPlatform();
		}

		void UpdateFlyout()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView.Flyout.ToPlatform(MauiContext);

			_flyoutPanel.Children.Clear();

			if (VirtualView.Flyout.ToPlatform() is UIElement element)
				_flyoutPanel.Children.Add(element);
		}

		public static void MapDetail(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.UpdateDetail();
		}

		public static void MapFlyout(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.UpdateFlyout();
		}

		public static void MapIsPresented(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.NativeView.IsPaneOpen = flyoutView.IsPresented;
		}

		public static void MapFlyoutWidth(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (flyoutView.Width >= 0)
				handler.NativeView.OpenPaneLength = flyoutView.Width;
			else
				handler.NativeView.OpenPaneLength = 320;
			// At some point this Template Setting is going to show up with a bump to winui
			//handler.NativeView.OpenPaneLength = handler.NativeView.TemplateSettings.OpenPaneWidth;

		}

		public static void MapFlyoutBehavior(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			var nativeView = handler.NativeView;

			switch (flyoutView.FlyoutBehavior)
			{
				case FlyoutBehavior.Flyout:
					nativeView.IsPaneToggleButtonVisible = true;
					// Workaround for
					// https://github.com/microsoft/microsoft-ui-xaml/issues/6493
					nativeView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
					nativeView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
					break;
				case FlyoutBehavior.Locked:
					nativeView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
					nativeView.IsPaneToggleButtonVisible = false;
					break;
				case FlyoutBehavior.Disabled:
					nativeView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
					nativeView.IsPaneToggleButtonVisible = false;
					nativeView.IsPaneOpen = false;
					break;

			}

			handler.UpdateFlyoutPanelMargin();
		}

		public static void MapIsGestureEnabled(FlyoutViewHandler handler, IFlyoutView view)
		{
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
