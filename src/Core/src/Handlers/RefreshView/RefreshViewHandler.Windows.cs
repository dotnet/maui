using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Core;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, MauiRefreshContainer>
	{
		bool _isLoaded;
		Deferral? _refreshCompletionDeferral;

		protected override MauiRefreshContainer CreateNativeView()
		{
			return new MauiRefreshContainer
			{
				PullDirection = RefreshPullDirection.TopToBottom
			};
		}

		protected override void ConnectHandler(MauiRefreshContainer nativeView)
		{
			nativeView.Loaded += OnLoaded;
			nativeView.RefreshRequested += OnRefresh;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiRefreshContainer nativeView)
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
				NativeView?.RequestRefresh();
		}

		void UpdateContent() =>  
			NativeView.UpdateContent(VirtualView.Content, MauiContext);

		void UpdateRefreshColor()
		{
			if (VirtualView == null || NativeView?.Visualizer == null)
				return;

			NativeView.Visualizer.Foreground = VirtualView.RefreshColor != null	
				? VirtualView.RefreshColor.ToNative()	
				: (WBrush)UI.Xaml.Application.Current.Resources["DefaultTextForegroundThemeBrush"];
		}

		void UpdateBackground()
		{
			if (VirtualView == null || NativeView?.Visualizer == null)
				return;

			if (VirtualView.Background != null)
				NativeView.Visualizer.Background = VirtualView.Background.ToNative();
			else
				NativeView.Visualizer.Background = Colors.White.ToNative();
		}

		// Telling the refresh to start before the control has been sized
		// causes no refresh circle to show up
		void OnLoaded(object sender, object args)
		{
			var refreshControl = sender as RefreshContainer;

			if (refreshControl == null)
				return;

			refreshControl.Loaded -= OnLoaded;
			_ = refreshControl.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
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