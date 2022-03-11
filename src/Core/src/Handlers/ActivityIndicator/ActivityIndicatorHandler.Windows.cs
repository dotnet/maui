#nullable enable

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, MauiActivityIndicator>
	{
		protected override MauiActivityIndicator CreatePlatformView() => new MauiActivityIndicator
		{
			IsIndeterminate = true,
			//Style = UI.Xaml.Application.Current.Resources["MauiActivityIndicatorStyle"] as UI.Xaml.Style
		};

		public static void MapIsRunning(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateIsRunning(activityIndicator);
		}

		public static void MapColor(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateColor(activityIndicator);
		}

		public static void MapWidth(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			if (handler.PlatformView is MauiActivityIndicator platformView)
			{
				platformView.UpdateWidth(activityIndicator);
			}
		}

		public static void MapHeight(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			if (handler.PlatformView is MauiActivityIndicator platformView)
			{
				platformView.UpdateHeight(activityIndicator);
			}
		}
	}
}