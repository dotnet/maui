using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Microsoft.Maui.Handlers
{

	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, MauiNavigationView>
	{
		readonly FlyoutPanel _flyoutPanel = new FlyoutPanel();

		protected override MauiNavigationView CreateNativeView()
		{
			var navigationView = new MauiNavigationView();
			navigationView.PaneFooter = _flyoutPanel;
			return navigationView;
		}

		protected override void ConnectHandler(MauiNavigationView nativeView)
		{
			nativeView.FlyoutPaneSizeChanged += OnFlyoutPaneSizeChanged;
			nativeView.PaneOpened += OnPaneOepened;
		}

		protected override void DisconnectHandler(MauiNavigationView nativeView)
		{
			nativeView.FlyoutPaneSizeChanged += OnFlyoutPaneSizeChanged;
			nativeView.PaneOpened -= OnPaneOepened;
		}

		void OnFlyoutPaneSizeChanged(object? sender, EventArgs e)
		{
			_flyoutPanel.Height = NativeView.FlyoutPaneSize.Height;
			_flyoutPanel.Width = NativeView.FlyoutPaneSize.Width;
		}

		void OnPaneOepened(NavigationView sender, object args)
		{
			VirtualView.IsPresented = sender.IsPaneOpen;
		}

		void UpdateDetail()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView.Detail.ToNative(MauiContext);

			NativeView.Content = VirtualView.Detail.GetNative(true);
		}
		
		void UpdateFlyout()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView.Flyout.ToNative(MauiContext);
			
			_flyoutPanel.Children.Clear();

			if(VirtualView.Flyout.GetNative(true) is UIElement element)
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
			var template = handler.NativeView.TemplateSettings;
			if (flyoutView.Width != -1)
				handler.NativeView.OpenPaneLength = flyoutView.Width;
			else
				handler.NativeView.OpenPaneLength = 540;
				// At some point this Template Setting is going to show up with a bump to winui
				//handler.NativeView.OpenPaneLength = handler.NativeView.TemplateSettings.OpenPaneWidth;

		}

		public static void MapFlyoutBehavior(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			var nativeView = handler.NativeView;

			switch (flyoutView.FlyoutBehavior)
			{
				case FlyoutBehavior.Flyout:
					nativeView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
					nativeView.IsPaneToggleButtonVisible = true;
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
					return Size.Empty;

				FlyoutContent.Measure(availableSize);
				return FlyoutContent.DesiredSize;
			}

			protected override Size ArrangeOverride(Size finalSize)
			{
				if (FlyoutContent == null)
					return Size.Empty;

				FlyoutContent.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
				return new Size(FlyoutContent.ActualWidth, FlyoutContent.ActualHeight);
			}
		}
	}
}
