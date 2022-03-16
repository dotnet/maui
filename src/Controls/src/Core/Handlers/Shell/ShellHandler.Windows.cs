using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : ViewHandler<Shell, ShellView>
	{
		ScrollViewer _scrollViewer;
		double? headerHeight = null;
		double? headerOffset = null;

		protected override ShellView CreatePlatformView()
		{
			var shellView = new ShellView();
			shellView.SetElement(VirtualView);
			return shellView;
		}

		protected override void ConnectHandler(ShellView platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is MauiNavigationView mauiNavigationView)
				mauiNavigationView.OnApplyTemplateFinished += OnApplyTemplateFinished;

			platformView.PaneOpened += OnPaneOpened;
			platformView.PaneOpening += OnPaneOpening;
			platformView.PaneClosing += OnPaneClosing;
			platformView.ItemInvoked += OnMenuItemInvoked;
		}

		protected override void DisconnectHandler(ShellView platformView)
		{
			base.DisconnectHandler(platformView);

			if (platformView is MauiNavigationView mauiNavigationView)
				mauiNavigationView.OnApplyTemplateFinished -= OnApplyTemplateFinished;

			platformView.PaneOpened -= OnPaneOpened;
			platformView.PaneOpening -= OnPaneOpening;
			platformView.PaneClosing -= OnPaneClosing;
			platformView.ItemInvoked -= OnMenuItemInvoked;
		}

		void OnMenuItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
		{
			var item = args.InvokedItemContainer?.DataContext as Element;
			if (item != null)
				(VirtualView as IShellController)?.OnFlyoutItemSelected(item);
		}

		void OnApplyTemplateFinished(object sender, System.EventArgs e)
		{
			if (PlatformView == null)
				return;

			_scrollViewer = PlatformView.MenuItemsScrollViewer;

			UpdateValue(nameof(Shell.FlyoutHeaderBehavior));
		}

		void OnPaneOpened(UI.Xaml.Controls.NavigationView sender, object args)
		{
			PlatformView.UpdateFlyoutBackdrop();
		}

		void OnPaneClosing(UI.Xaml.Controls.NavigationView sender, UI.Xaml.Controls.NavigationViewPaneClosingEventArgs args)
		{
			args.Cancel = true;
			VirtualView.FlyoutIsPresented = false;
		}

		void OnPaneOpening(UI.Xaml.Controls.NavigationView sender, object args)
		{
			UpdateValue(nameof(Shell.FlyoutBackground));
			UpdateValue(nameof(Shell.FlyoutVerticalScrollMode));
			PlatformView.UpdateFlyoutBackdrop();
			PlatformView.UpdateFlyoutPosition();
			VirtualView.FlyoutIsPresented = true;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			if (PlatformView.Element != view)
				PlatformView.SetElement((Shell)view);
		}

		public static void MapFlyoutBackdrop(ShellHandler handler, Shell view)
		{
			if (Brush.IsNullOrEmpty(view.FlyoutBackdrop))
				handler.PlatformView.FlyoutBackdrop = null;
			else
				handler.PlatformView.FlyoutBackdrop = view.FlyoutBackdrop;
		}

		public static void MapCurrentItem(ShellHandler handler, Shell view)
		{
			handler.PlatformView.SwitchShellItem(view.CurrentItem, true);
		}

		public static void MapFlyoutBackground(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdatePaneBackground(
				!Brush.IsNullOrEmpty(view.FlyoutBackground) ?
					view.FlyoutBackground :
					view.FlyoutBackgroundColor?.AsPaint());
		}

		public static void MapFlyoutVerticalScrollMode(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateFlyoutVerticalScrollMode((WScrollMode)(int)view.FlyoutVerticalScrollMode);
		}

		public static void MapFlyout(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.ReplacePaneMenuItemsWithCustomContent(flyoutView.Flyout);
		}

		public static void MapIsPresented(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.IsPaneOpen = flyoutView.IsPresented;
		}

		public static void MapFlyoutWidth(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateFlyoutWidth(flyoutView);
		}

		public static void MapFlyoutBehavior(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateFlyoutBehavior(flyoutView);
		}

		public static void MapFlyoutFooter(ShellHandler handler, Shell view)
		{
			if (handler.PlatformView.PaneFooter == null)
				handler.PlatformView.PaneFooter = new ShellFooterView(view);
		}

		public static void MapFlyoutHeader(ShellHandler handler, Shell view)
		{
			if (handler.PlatformView.PaneCustomContent == null)
				handler.PlatformView.PaneCustomContent = new ShellHeaderView(view);
		}

		public static void MapFlyoutHeaderBehavior(ShellHandler handler, Shell view)
		{
			handler.UpdateFlyoutHeaderBehavior(view);
		}

		public static void MapItems(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateMenuItemSource();
		}

		public static void MapFlyoutItems(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateMenuItemSource();
		}

		void UpdateFlyoutHeaderBehavior(Shell view)
		{
			var flyoutHeader = (ShellHeaderView)PlatformView.PaneCustomContent;

			if (view.FlyoutHeaderBehavior == FlyoutHeaderBehavior.Default ||
				view.FlyoutHeaderBehavior == FlyoutHeaderBehavior.Fixed)
			{
				var defaultHeight = headerHeight;
				var defaultTranslateY = headerOffset;

				UpdateFlyoutHeaderTransformation(flyoutHeader, defaultHeight, defaultTranslateY);
				return;
			}

			var menuItemsScrollViewer = _scrollViewer;

			if (menuItemsScrollViewer != null)
			{
				double? topAreaHeight = null;

				menuItemsScrollViewer.ViewChanged += (sender, args) =>
				{
					if (headerHeight == null)
						headerHeight = flyoutHeader.ActualHeight;

					if (headerOffset == null)
					{
						if (flyoutHeader.RenderTransform is CompositeTransform compositeTransform)
							headerOffset = compositeTransform.TranslateY;
						else
							headerOffset = 0;
					}

					switch (view.FlyoutHeaderBehavior)
					{
						case FlyoutHeaderBehavior.Scroll:
							var scrollHeight = Math.Max(headerHeight.Value - menuItemsScrollViewer.VerticalOffset, 0);
							var scrollTranslateY = -menuItemsScrollViewer.VerticalOffset;

							UpdateFlyoutHeaderTransformation(flyoutHeader, scrollHeight, scrollTranslateY);
							break;
						case FlyoutHeaderBehavior.CollapseOnScroll:
							var topNavArea = (StackPanel)PlatformView.TopNavArea;
							if (topAreaHeight == null)
								topAreaHeight = Math.Max(topNavArea.ActualHeight, 50.0f);

							var calculatedHeight = headerHeight.Value - menuItemsScrollViewer.VerticalOffset;
							var collapseOnScrollHeight = calculatedHeight < topAreaHeight.Value ? topAreaHeight.Value : calculatedHeight;

							var offsetY = -menuItemsScrollViewer.VerticalOffset;
							var maxOffsetY = -topAreaHeight.Value;
							var collapseOnScrollTranslateY = offsetY < maxOffsetY ? maxOffsetY : offsetY;

							UpdateFlyoutHeaderTransformation(flyoutHeader, collapseOnScrollHeight, collapseOnScrollTranslateY);
							break;
					}
				};
			}
		}

		void UpdateFlyoutHeaderTransformation(ShellHeaderView flyoutHeader, double? height, double? translationY)
		{
			if (translationY.HasValue)
			{
				flyoutHeader.RenderTransform = new CompositeTransform
				{
					TranslateY = translationY.Value
				};
			}

			if (height.HasValue)
			{
				flyoutHeader.Height = height.Value;
			}
		}
	}
}
