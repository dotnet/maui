using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, RefreshContainer>
	{
		bool _isLoaded;
		Deferral? _refreshCompletionDeferral;

		protected override RefreshContainer CreatePlatformView()
		{
			return new RefreshContainer
			{
				PullDirection = RefreshPullDirection.TopToBottom
			};
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

			base.DisconnectHandler(nativeView);
		}

		public static void MapIsRefreshing(RefreshViewHandler handler, IRefreshView refreshView)
			=> handler.UpdateIsRefreshing();

		public static void MapContent(RefreshViewHandler handler, IRefreshView refreshView)
			=> handler.UpdateContent();

		public static void MapRefreshColor(RefreshViewHandler handler, IRefreshView refreshView)
			=> handler.UpdateRefreshColor();

		public static void MapRefreshViewBackground(RefreshViewHandler handler, IView view)
			=> handler.UpdateBackground();

		void UpdateIsRefreshing()
		{
			if (!_isLoaded)
				return;

			if (!VirtualView?.IsRefreshing ?? false)
				CompleteRefresh();
			else if (_refreshCompletionDeferral == null)
				PlatformView?.RequestRefresh();
		}

		void UpdateContent()
		{
			PlatformView.Content = VirtualView.Content.ToPlatform(MauiContext!);
		}

		void UpdateRefreshColor()
		{
			if (VirtualView == null || PlatformView?.Visualizer == null)
				return;

			PlatformView.Visualizer.Foreground = VirtualView.RefreshColor != null	
				? VirtualView.RefreshColor.ToPlatform()	
				: (WBrush)UI.Xaml.Application.Current.Resources["DefaultTextForegroundThemeBrush"];
		}

		void UpdateBackground()
		{
			if (VirtualView == null || PlatformView?.Visualizer == null)
				return;

			if (VirtualView.Background != null)
				PlatformView.Visualizer.Background = VirtualView.Background.ToPlatform();
			else
				PlatformView.Visualizer.Background = Colors.White.ToPlatform();
		}

		// Telling the refresh to start before the control has been sized
		// causes no refresh circle to show up
		void OnLoaded(object sender, object args)
		{
			var refreshControl = sender as RefreshContainer;

			if (refreshControl == null || MauiContext == null)
				return;

			refreshControl.Loaded -= OnLoaded;
			MauiContext.Services
				.GetRequiredService<IDispatcher>()
				.Dispatch(() =>
				{
					_isLoaded = true;
					UpdateIsRefreshing();
				});
		}

		void OnRefresh(object sender, RefreshRequestedEventArgs args)
		{
			CompleteRefresh();
			_refreshCompletionDeferral = args.GetDeferral();

			if (VirtualView != null)
				VirtualView.IsRefreshing = true;
		}

		void CompleteRefresh()
		{
			if (_refreshCompletionDeferral != null)
			{
				_refreshCompletionDeferral.Complete();
				_refreshCompletionDeferral.Dispose();
				_refreshCompletionDeferral = null;
			}
		}
	}
}
