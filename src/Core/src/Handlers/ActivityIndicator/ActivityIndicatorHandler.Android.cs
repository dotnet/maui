using Android.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, ProgressBar>
	{
		protected override ProgressBar CreateNativeView() => new ProgressBar(Context) { Indeterminate = true };

		public static void MapIsRunning(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.NativeView?.UpdateIsRunning(activityIndicator);
		}

		public static void MapColor(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.NativeView?.UpdateColor(activityIndicator);
		}
	}
}