using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, RefreshContainer>
	{
		Deferral? _refreshCompletionDeferral;

		protected override RefreshContainer CreatePlatformView()
		{
			var refreshControl = new RefreshContainer
			{
				PullDirection = RefreshPullDirection.TopToBottom,
				Content = new ContentPanel()
			};

			SetRefreshColorCallback(refreshControl);
			return refreshControl;
		}

		protected override void ConnectHandler(RefreshContainer nativeView)
		{
			nativeView.Loaded += OnLoaded;
			nativeView.RefreshRequested += OnRefresh;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(RefreshContainer nativeView)
		{
			nativeView.Loaded -= OnLoaded;
			nativeView.RefreshRequested -= OnRefresh;

			CompleteRefresh();

			if (nativeView.Content is ContentPanel contentPanel)
			{
				contentPanel.Content = null;
				contentPanel.CrossPlatformLayout = null;
			}

			base.DisconnectHandler(nativeView);
		}

		public static void MapIsRefreshing(IRefreshViewHandler handler, IRefreshView refreshView)
			=> (handler as RefreshViewHandler)?.UpdateIsRefreshing();

		public static void MapContent(IRefreshViewHandler handler, IRefreshView refreshView)
			=> UpdateContent(handler);

		public static void MapRefreshColor(IRefreshViewHandler handler, IRefreshView refreshView)
			=> UpdateRefreshColor(handler);

		public static void MapRefreshViewBackground(IRefreshViewHandler handler, IView view)
			=> UpdateBackground(handler);

		internal static void MapIsRefreshEnabled(IRefreshViewHandler handler, IRefreshView refreshView)
			=> (handler as RefreshViewHandler)?.UpdateIsRefreshEnabled();

		void UpdateIsRefreshing()
		{
			if (PlatformView is not { } platform || !platform.IsLoaded())
				return;

			if (!VirtualView.IsRefreshing)
			{
				// The virtual view is no longer refreshing, so we complete the refresh
				CompleteRefresh();
			}
			else if (_refreshCompletionDeferral is null)
			{
				// The virtual view requested a refresh but we have not yet started a refresh,
				// so we request a refresh on the platform view
				platform.RequestRefresh();
			}
		}

		void UpdateIsRefreshEnabled()
		{
			if (PlatformView is not { } platform || !platform.IsLoaded())
				return;

			if (!VirtualView.IsRefreshEnabled)
			{
				// If the virtual view is not enabled for refresh, we complete any ongoing refresh
				CompleteRefresh();
			}
		}

		static void UpdateContent(IRefreshViewHandler handler)
		{
			IView? content;

			if (handler.VirtualView is IContentView cv && cv.PresentedContent is IView view)
			{
				content = view;
			}
			else
			{
				content = handler.VirtualView.Content;
			}

			var platformContent = content?.ToPlatform(handler.MauiContext!);
			if (handler.PlatformView.Content is ContentPanel contentPanel)
			{
				contentPanel.Content = platformContent;
				contentPanel.CrossPlatformLayout = (handler.VirtualView as ICrossPlatformLayout);
			}
			else
			{
				handler.PlatformView.Content = platformContent;
			}
		}

		static void UpdateRefreshColor(IRefreshViewHandler handler)
		{
			if (handler.VirtualView == null || handler.PlatformView?.Visualizer == null)
				return;

			handler.PlatformView.Visualizer.Foreground = handler.VirtualView.RefreshColor != null
				? handler.VirtualView.RefreshColor.ToPlatform()
				: (WBrush)UI.Xaml.Application.Current.Resources["DefaultTextForegroundThemeBrush"];
		}

		static void UpdateBackground(IRefreshViewHandler handler)
		{
			if (handler.PlatformView?.Visualizer == null)
				return;

			if (handler.VirtualView.Background != null)
				handler.PlatformView.Visualizer.Background = handler.VirtualView.Background.ToPlatform();
		}

		// Telling the refresh to start before the control has been sized
		// causes no refresh circle to show up
		void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (sender is not RefreshContainer refreshControl)
				return;

			refreshControl.Loaded -= OnLoaded;

			// If the virtual view requested a refresh, we need to trigger it now that the control
			// is loaded. This needs to be done on a dispatch as the control may need to be laid
			// out or the template applied first.
			refreshControl.DispatcherQueue.TryEnqueue(UpdateIsRefreshing);
		}

		void OnRefresh(object sender, RefreshRequestedEventArgs args)
		{
			// Finish any previous refresh if it was not completed
			CompleteRefresh();

			if (!VirtualView.IsRefreshEnabled)
			{
				// The refresh is not enabled, so we don't proceed with the refresh
				return;
			}

			// Store the deferral to complete the refresh later
			_refreshCompletionDeferral = args.GetDeferral();

			if (VirtualView != null && !VirtualView.IsRefreshing)
			{
				VirtualView.IsRefreshing = true;
			}
		}

		void CompleteRefresh()
		{
			_refreshCompletionDeferral?.Complete();
			_refreshCompletionDeferral?.Dispose();
			_refreshCompletionDeferral = null;
		}

		void SetRefreshColorCallback(RefreshContainer refreshControl)
		{
			long callbackToken = 0;
			callbackToken = refreshControl.RegisterPropertyChangedCallback(RefreshContainer.VisualizerProperty,
				(_, __) =>
				{
					if (refreshControl?.Visualizer == null)
						return;

					UpdateRefreshColor(this);
					refreshControl.UnregisterPropertyChangedCallback(RefreshContainer.VisualizerProperty, callbackToken);
				});
		}
	}
}
