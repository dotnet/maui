using Tizen.UIExtensions.NUI.GraphicsView;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, ActivityIndicator>
	{
		protected override ActivityIndicator CreatePlatformView() => new ActivityIndicator();

		public static partial void MapIsRunning(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateIsRunning(activityIndicator);
		}

		public static partial void MapColor(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateColor(activityIndicator);
		}
	}
}