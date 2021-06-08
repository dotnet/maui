using Gtk;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, Spinner>
	{
		protected override Spinner CreateNativeView()
		{
			return new();
		}

		public static void MapIsRunning(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.NativeView?.UpdateIsRunning(activityIndicator);
		}

		
		public static void MapColor(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.NativeView?.SetForegroundColor(activityIndicator.Color);
			
		}
	}
}
