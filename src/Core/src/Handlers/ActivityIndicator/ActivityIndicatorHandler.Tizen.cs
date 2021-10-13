using NView = Tizen.NUI.BaseComponents.View;
using Tizen.UIExtensions.NUI.GraphicsView;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, ActivityIndicator>
	{
		protected override ActivityIndicator CreateNativeView() => new ActivityIndicator();

		public static void MapIsRunning(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateIsRunning(activityIndicator);
		}

		public static void MapColor(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateColor(activityIndicator);
		}
	}
}